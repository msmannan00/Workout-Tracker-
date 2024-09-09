using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WorkoutLogController : MonoBehaviour, PageController
{
    public InputField workoutNameText;
    public TMP_InputField workoutNotes;
    public TextMeshProUGUI timerText;
    public Transform content;

    private int exerciseCounter = 0;
    public List<ExerciseInformationModel> exercises = new List<ExerciseInformationModel>();
    private List<ExerciseDataItem> exerciseDataItems = new List<ExerciseDataItem>();
    public DefaultTempleteModel templeteModel = new DefaultTempleteModel();

    private bool isTemplateCreator;
    private bool isTimerRunning = false;
    private float elapsedTime = 0f;
    private Coroutine timerCoroutine;
    private Color enabledColor = Color.white;
    private Color disabledColor = Color.gray;
    Action<object> callback;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        this.callback = callback;
        isTemplateCreator = (bool)data["isTemplateCreator"];

        if (data.ContainsKey("dataTemplate"))
        {
            DefaultTempleteModel dataTemplate = (DefaultTempleteModel)data["dataTemplate"];
            templeteModel.templeteName= dataTemplate.templeteName;
            workoutNotes.text = dataTemplate.templeteNotes;
            List<ExerciseTypeModel> list = new List<ExerciseTypeModel>();
            foreach (var exerciseType in dataTemplate.exerciseTemplete)
            {
                list.Add(exerciseType);
                print(exerciseType.name);
            }
            OnExerciseAdd(list);
            if(workoutNameText!=null)
                workoutNameText.text = dataTemplate.templeteName;
            workoutNotes.onValueChanged.AddListener(OnNotesChange);
        }
        OnToggleWorkout();
    }

    private void Start()
    {
        timerText.color = disabledColor;
        if (workoutNameText != null)
        {
            workoutNameText.onEndEdit.AddListener(OnNameChanged);
        }
    }

    public void OnToggleWorkout()
    {
        if (templeteModel.exerciseTemplete.Count == 0)
        {
            return;
        }

        isTimerRunning = !isTimerRunning;

        if (isTimerRunning)
        {
            timerText.color = enabledColor;

            if (timerCoroutine == null)
            {
                timerCoroutine = StartCoroutine(TimerCoroutine());
            }
        }
        else
        {
            timerText.color = disabledColor;

            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            timerText.text = /*"Timer: " +*/ string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return null;
        }
    }

    public void OnNameChanged(string name)
    {
        templeteModel.templeteName = name;
    }

    public void AddExerciseButton()
    {
        Dictionary<string, object> mData = new Dictionary<string, object>
        {
            { "isWorkoutLog", true }
        };
        StateManager.Instance.OpenStaticScreen("exercise", gameObject, "exerciseScreen", mData, true, OnExerciseAdd);
    }

    private void OnNotesChange(string input)
    {
        templeteModel.templeteNotes = input;
    }

    public void OnExerciseAdd(object data)
    {
        //List<object> dataList = data as List<object>;
        if (data == null)
        {
            print("data null");
        }

        if (data is List<ExerciseDataItem> dataList)
        {
            foreach (object item in dataList)
            {
                ExerciseTypeModel typeModel;

                if (item is ExerciseDataItem dataItem)
                {
                    typeModel = new ExerciseTypeModel
                    {
                        name = dataItem.exerciseName,
                        exerciseModel = new List<ExerciseModel>(),
                        index = exerciseCounter++
                    };

                    templeteModel.exerciseTemplete.Add(typeModel);

                    exerciseDataItems.Add(dataItem);
                }
                else
                {
                    typeModel = (ExerciseTypeModel)item;
                    templeteModel.exerciseTemplete.Add(typeModel);
                }

                Dictionary<string, object> mData = new Dictionary<string, object>
                {
                    { "data", typeModel }, { "isWorkoutLog", true }
                };

                GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/workoutLog/workoutLogScreenDataModel");
                GameObject exerciseObject = Instantiate(exercisePrefab, content);
                int childCount = content.childCount;
                exerciseObject.transform.SetSiblingIndex(childCount - 2);
                exerciseObject.GetComponent<workoutLogScreenDataModel>().onInit(mData, OnRemoveIndex);
            }
        }
        else if (data is List<ExerciseTypeModel> dataList2)
        {
            foreach (object item in dataList2)
            {
                ExerciseTypeModel typeModel;

                typeModel = (ExerciseTypeModel)item;
                templeteModel.exerciseTemplete.Add(typeModel);

                Dictionary<string, object> mData = new Dictionary<string, object>
                {
                    { "data", typeModel },
                    { "isWorkoutLog", true }
                };

                GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/workoutLog/workoutLogScreenDataModel");
                GameObject exerciseObject = Instantiate(exercisePrefab, content);
                exerciseObject.GetComponent<workoutLogScreenDataModel>().onInit(mData, OnRemoveIndex);
            }
        }
        else { print("null"); }
    }
    private void OnRemoveIndex(object data)
    {
        if (isTemplateCreator)
        {
            int index = (int)data;

            for (int i = 0; i < templeteModel.exerciseTemplete.Count; i++)
            {
                if (templeteModel.exerciseTemplete[i].index == index)
                {
                    templeteModel.exerciseTemplete.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void Finish()
    {
        isTimerRunning = false;
        var historyTemplate = new HistoryTempleteModel
        {
            templeteName = templeteModel.templeteName,
            dateTime = DateTime.Now,
            completedTime = (int)elapsedTime,
            totalWeight = CalculateTotalWeight(templeteModel),
            prs = 0 // Assuming PRs are not tracked here. Adjust as needed.
        };

        // Populate HistoryExerciseTypeModel list
        foreach (var exerciseType in templeteModel.exerciseTemplete)
        {
            var historyExerciseType = new HistoryExerciseTypeModel
            {
                exerciseName = exerciseType.name,
                index = exerciseType.index,
                isWeightExercise = exerciseType.isWeigtExercise,
                exerciseModel = new List<HistoryExerciseModel>()
            };
            // Populate HistoryExerciseModel list but only add exercises where toggle is true
            foreach (var exercise in exerciseType.exerciseModel)
            {
                print("bool "+exercise.toggle);
                if (exercise.toggle) // Only add exercise if toggle is true
                {
                    var historyExercise = new HistoryExerciseModel
                    {
                        weight = exercise.weight,
                        reps = exercise.reps,
                        time = exercise.time
                    };

                    historyExerciseType.exerciseModel.Add(historyExercise);
                }
            }

            // Only add the exerciseType if it has any exercises with toggle true
            if (historyExerciseType.exerciseModel.Count > 0)
            {
                historyTemplate.exerciseTypeModel.Add(historyExerciseType);
            }
        }
        if (historyTemplate.exerciseTypeModel.Count > 0)
        {
            userSessionManager.Instance.historyData.exerciseTempleteModel.Add(historyTemplate);
            userSessionManager.Instance.SaveHistory();
        }
        //return historyTemplate;


        foreach (var exerciseType in templeteModel.exerciseTemplete)
        {
            foreach (var exercise in exerciseType.exerciseModel)
            {
                exercise.setID = 1;
                exercise.previous = "-";
                exercise.weight = 0;
                exercise.lbs = 0;
                exercise.reps = 0;
            }
        }

        //if (isTemplateCreator && templeteModel.exerciseTemplete.Count > 0)
        //{
        //userSessionManager.Instance.excerciseData.exerciseTemplete.Add(templeteModel);
        int index = GetIndexByTempleteName(templeteModel.templeteName);
        userSessionManager.Instance.excerciseData.exerciseTemplete.RemoveAt(index);
        userSessionManager.Instance.excerciseData.exerciseTemplete.Insert(index, templeteModel);
            StateManager.Instance.HandleBackAction(gameObject);
            this.callback.Invoke(null);
            userSessionManager.Instance.SaveExcerciseData();
        //}
        OnBack();
    }

    public void OnBack()
    {
        StateManager.Instance.HandleBackAction(gameObject);
    }
    private int CalculateTotalWeight(DefaultTempleteModel defaultTemplate)
    {
        int totalWeight = 0;

        foreach (var exerciseType in defaultTemplate.exerciseTemplete)
        {
            if (exerciseType.isWeigtExercise)
            {
                foreach (var exercise in exerciseType.exerciseModel)
                {
                    totalWeight += exercise.weight * exercise.reps;
                }
            }
        }

        return totalWeight;
    }
    public int GetIndexByTempleteName(string name)
    {
        return userSessionManager.Instance.excerciseData.exerciseTemplete.FindIndex(t => t.templeteName == name);
    }
}
