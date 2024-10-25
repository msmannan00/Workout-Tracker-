using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompleteWorkoutController : MonoBehaviour, IPrefabInitializer
{
    public TextMeshProUGUI workoutNameText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI totalTimeText;
    public TextMeshProUGUI totalWeightText;
    public Transform content;
    public ParticleSystem particleComplete;
    public GameObject workoutScreen;
    public void InitPrefab(Action<List<object>> onFinish, List<object> data)
    {
        HistoryTempleteModel historyWorkout = (HistoryTempleteModel)data[0];
        workoutScreen = (GameObject)data[1];
        workoutNameText.text=historyWorkout.templeteName.ToUpper();
        string savedDate = historyWorkout.dateTime;
        DateTime parsedDate = DateTime.ParseExact(savedDate, "MMM dd, yyyy", System.Globalization.CultureInfo.InvariantCulture);
        string formattedDate = parsedDate.ToString("dddd, dd MMMM yyyy");
        dateText.text=formattedDate;
        if (historyWorkout.completedTime > 60)
        {
            totalTimeText.text = ((int)historyWorkout.completedTime / 60).ToString() + "m";
        }
        else
        {
            totalTimeText.text = historyWorkout.completedTime.ToString() + "s";
        }
        totalWeightText.text=historyWorkout.totalWeight.ToString();
        foreach(HistoryExerciseTypeModel exercise in historyWorkout.exerciseTypeModel)
        {
            GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/complete/completeScreenDataModel");
            GameObject newExerciseObject = Instantiate(exercisePrefab, content);
            newExerciseObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = exercise.exerciseName.ToUpper();
            switch (exercise.exerciseType)
            {
                case ExerciseType.RepsOnly:
                    ShowOnlyReps(exercise, newExerciseObject, newExerciseObject.transform.GetChild(0).gameObject);
                    break;
                case ExerciseType.TimeBased:
                    ShowOnlyTime(exercise, newExerciseObject, newExerciseObject.transform.GetChild(0).gameObject);
                    break;
                case ExerciseType.TimeAndMiles:
                    ShowTimeAndMile(exercise, newExerciseObject, newExerciseObject.transform.GetChild(0).gameObject);
                    break;
                case ExerciseType.WeightAndReps:
                    ShowWeightAndReps(exercise, newExerciseObject, newExerciseObject.transform.GetChild(0).gameObject);
                    break;
            }
        }
        Invoke("onParticleSystem", 0.5f);
        Destroy(workoutScreen);
    }
    void onParticleSystem()
    {
        particleComplete.Play();
        userSessionManager.Instance.CheckAchievementStatus();
    }
    public void Back()
    {
        StateManager.Instance.OpenStaticScreen("history", workoutScreen, "historyScreen", null, isfooter: true);
        PopupController.Instance.ClosePopup("completeWorkoutPopup");
        StateManager.Instance.OpenFooter(null, null, false);
    }
    void ShowOnlyReps(HistoryExerciseTypeModel exercise,GameObject parent, GameObject prefab)
    {
        foreach(HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text=textObj.GetComponent<TextMeshProUGUI>();
            text.text = data.reps.ToString();
            text.fontSize = 14;
        }
    }
    void ShowOnlyTime(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.fontSize = 14;
            if (data.time > 60)
            {
                text.text = ((int)data.time / 60).ToString() + " m";
            }
            else
            {
                text.text = data.time.ToString() + " s";
            }
        }
    }
    void ShowWeightAndReps(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = data.weight.ToString() + " kg x " + data.reps.ToString();
            text.fontSize = 14;
        }
    }
    void ShowTimeAndMile(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            string time = "";
            if (data.time > 60)
            {
                time = ((int)data.time / 60).ToString() + " m";
            }
            else
            {
                time = data.time.ToString() + " s";
            }
            text.text = data.mile.ToString() + " mile x " + time;
            text.fontSize = 14;
        }
    }

}
