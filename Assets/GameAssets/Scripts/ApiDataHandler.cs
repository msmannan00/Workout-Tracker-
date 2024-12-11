using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;

public class ApiDataHandler : GenericSingletonClass<ApiDataHandler>
{
    [SerializeField]
    private ExerciseData exerciseData = new ExerciseData(); // firebase
    [SerializeField]
    private AchievementData achievementData = new AchievementData();    //firebase
    [SerializeField]
    private PersonalBestData personalBestData = new PersonalBestData(); //firebase
    [SerializeField]
    private TemplateData templateData = new TemplateData(); //firebase
    [SerializeField]
    private HistoryModel historyData = new HistoryModel();  //firebase
    [SerializeField]
    private MeasurementModel measurementData = new MeasurementModel();  //firebase
    [SerializeField]
    private MeasurementHistory measurementHistory = new MeasurementHistory();   //firebase
    [SerializeField]
    private ExerciseNotesHistory notesHistory = new ExerciseNotesHistory(); //firebase
    [SerializeField]
    private string userName;

    [Header("Theme Settings")]
    public Theme gameTheme;


    public void SaveTheme(Theme theme)
    {
        PreferenceManager.Instance.SetInt("SelectedTheme", (int)theme);
        gameTheme = theme;
        PreferenceManager.Instance.Save();
    }
    public Theme LoadTheme()
    {
        int savedTheme = PlayerPrefs.GetInt("SelectedTheme", (int)Theme.Light);
        return (Theme)savedTheme;
    }
    public void SaveWeight(int weight)
    {
        PreferenceManager.Instance.SetInt("Weight", weight);
        PreferenceManager.Instance.Save();
    }
    public int GetWeight()
    {
         return PlayerPrefs.GetInt("Weight", 0);
    }
    public void SaveTemplateData()
    {
        string json = JsonUtility.ToJson(templateData);
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId)
            .Child("workoutTempletes").SetRawJsonValueAsync(json).ContinueWith(task => {
                if (task.IsCompleted)
                {
                    Debug.Log("workoutTempletes added");
                }
                else
                {
                    Debug.LogError("Failed to add exercise: " + task.Exception);
                }
            });
    }
    public void ReplaceExerciseTemplate(DefaultTempleteModel newTemplate, int templateIndex)
    {

        // Convert the new template to JSON
        string jsonUpdatedTemplate = JsonUtility.ToJson(newTemplate);
        // Update the specific template node in Firebase
        FirebaseManager.Instance.databaseReference.Child("users")
            .Child(FirebaseManager.Instance.user.UserId).Child("workoutTempletes").Child("exerciseTemplete")
            .Child(templateIndex.ToString()).SetRawJsonValueAsync(jsonUpdatedTemplate).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Workout template replaced successfully.");
                }
                else
                {
                    Debug.LogError("Failed to replace workout template: " + task.Exception);
                }
            });
    }
    public void AddExerciseTemplate(DefaultTempleteModel template,int index)
    {
        string json = JsonUtility.ToJson(template);
        FirebaseManager.Instance.databaseReference.Child("users")
           .Child(FirebaseManager.Instance.user.UserId).Child("workoutTempletes").Child("exerciseTemplete")
           .Child(index.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
           {
               if (task.IsCompleted)
               {
                   Debug.Log("Workout template added.");
               }
               else
               {
                   Debug.LogError("Failed to add workout template: " + task.Exception);
               }
           });
    }
    public void DeleteExerciseTemplate(int templateIndex)
    {
        // Reference to the workoutTempletes node in Firebase
        var reference = FirebaseManager.Instance.databaseReference
            .Child("users")
            .Child(FirebaseManager.Instance.user.UserId)
            .Child("workoutTempletes").Child("exerciseTemplete");

        // Remove the template at the specific index in Firebase
        reference.Child(templateIndex.ToString())  // Use the index as the key
            .RemoveValueAsync().ContinueWith(task => {
                if (task.IsCompleted)
                {
                    Debug.Log("Workout template deleted successfully.");
                }
                else
                {
                    Debug.LogError("Failed to delete workout template: " + task.Exception);
                }
            });
    }

    public void LoadTemplateData()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/"+ FirebaseManager.Instance.user.UserId +"/workoutTempletes", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId  + "/workoutTempletes", _data =>
                { 
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        templateData = (TemplateData)LoadData(jsonData, typeof(TemplateData));
                        print("from firebase");
                    }
                });
               
                Debug.Log("Data exists at the path.");
            }
            else
            {
                CreateRandomDefaultEntry();

                Debug.Log("No data found at the path.");
            }
        });

        
    }

    public void LoadExercises()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/exercises", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId , _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        exerciseData = (ExerciseData)LoadData(jsonData, typeof(ExerciseData));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }
            else
            {
                TextAsset exerciseJsonFile = Resources.Load<TextAsset>("data/exercise");
                string exerciseJson = exerciseJsonFile.text;
                this.exerciseData = JsonUtility.FromJson<ExerciseData>(exerciseJson);
                FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId)
                .SetRawJsonValueAsync(exerciseJson).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("exercise added.");
                    }
                    else
                    {
                        Debug.LogError("Failed to add exercises: " + task.Exception);
                    }
                });
            }
        });


    }
    public void SaveExerciseData(ExerciseDataItem exercise,int index)
    {
        string json = JsonUtility.ToJson(exercise);

        // Reference the "exercises" node directly
        var exercisesRef = FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("exercises").Child(index.ToString());

        exercisesRef.SetRawJsonValueAsync(json).ContinueWith(addTask =>
        {
            if (addTask.IsCompleted)
            {
                Debug.Log("Exercise added at index: " + index);
            }
            else
            {
                Debug.LogError("Failed to add exercise: " + addTask.Exception);
            }
        });
    }
    public object LoadData(string jsonData, Type targetType)
    {
        return JsonUtility.FromJson(jsonData, targetType);
    }
    
    

    public void CreateRandomDefaultEntry()
    {
        ExerciseTypeModel back1 = new ExerciseTypeModel
        {
            index = 0,
            name = "Deadlifts (Barbell)",
            categoryName = "Glutes",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel back2 = new ExerciseTypeModel
        {
            index = 0,
            name = "Seated narrow grip row (cable)",
            categoryName = "Lats",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel chest = new ExerciseTypeModel
        {
            index = 0,
            name = "Bench Press (Barbell)",
            categoryName = "Chest",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel running = new ExerciseTypeModel
        {
            index = 0,
            name = "Running",
            categoryName = "Cardio",
            exerciseType = ExerciseType.TimeAndMiles,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel jumpRope = new ExerciseTypeModel
        {
            index = 0,
            name = "Jump Rope",
            categoryName = "Cardio",
            exerciseType = ExerciseType.RepsOnly,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel spiderCurls = new ExerciseTypeModel
        {
            index = 0,
            name = "Spider Curls",
            categoryName = "Biceps",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel bicepCulDumbbell = new ExerciseTypeModel
        {
            index = 0,
            name = "Bicep Curl (Dumbbell)",
            categoryName = "Biceps",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel bicepCurlMachine = new ExerciseTypeModel
        {
            index = 0,
            name = "Bicep Curl (Machine)",
            categoryName = "Biceps",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseModel defaultExerciseModel1 = new ExerciseModel
        {
            setID = 1,
            previous = "-",
            weight = 0,
            rir = 0,
            reps = 0
        };
        back1.exerciseModel.Add(defaultExerciseModel1);
        back1.exerciseModel.Add(defaultExerciseModel1);
        back1.exerciseModel.Add(defaultExerciseModel1);
        back2.exerciseModel.Add(defaultExerciseModel1);
        chest.exerciseModel.Add(defaultExerciseModel1);
        running.exerciseModel.Add(defaultExerciseModel1);
        running.exerciseModel.Add(defaultExerciseModel1);
        jumpRope.exerciseModel.Add(defaultExerciseModel1);
        jumpRope.exerciseModel.Add(defaultExerciseModel1);
        jumpRope.exerciseModel.Add(defaultExerciseModel1);
        spiderCurls.exerciseModel.Add(defaultExerciseModel1);
        spiderCurls.exerciseModel.Add(defaultExerciseModel1);
        bicepCulDumbbell.exerciseModel.Add(defaultExerciseModel1);
        bicepCulDumbbell.exerciseModel.Add(defaultExerciseModel1);
        bicepCurlMachine.exerciseModel.Add(defaultExerciseModel1);
        bicepCurlMachine.exerciseModel.Add(defaultExerciseModel1);


        DefaultTempleteModel chestAndBack = new DefaultTempleteModel
        {
            templeteName = "Chest And Back",
            exerciseTemplete = new List<ExerciseTypeModel> { back1, back2, chest }
        };
        DefaultTempleteModel runingAndJumpRope = new DefaultTempleteModel
        {
            templeteName = "Runing And Jump Rope",
            exerciseTemplete = new List<ExerciseTypeModel> { running, jumpRope }
        };
        DefaultTempleteModel bicep = new DefaultTempleteModel
        {
            templeteName = "Biceps",
            exerciseTemplete = new List<ExerciseTypeModel> { bicepCulDumbbell, bicepCurlMachine, spiderCurls }
        };
        templateData.exerciseTemplete.Clear();
        templateData.exerciseTemplete.Add(chestAndBack);
        templateData.exerciseTemplete.Add(runingAndJumpRope);
        templateData.exerciseTemplete.Add(bicep);
        print("create templete");
        SaveTemplateData();
    }
    public void LoadMeasurementData()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/measurements", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/measurements", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        measurementData = (MeasurementModel)LoadData(jsonData, typeof(MeasurementModel));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }

        });
    }
    public void LoadMeasurementHistory()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/measurementHistory", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/measurementHistory", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        measurementHistory = (MeasurementHistory)LoadData(jsonData, typeof(MeasurementHistory));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }

        });
    }
    public void SaveMeasurementData()
    {
        string json = JsonUtility.ToJson(measurementData);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/measurements";

        FirebaseManager.Instance.databaseReference.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });
    }
    public void SaveMeasurementHistory(MeasurementHistoryItem item,int index)
    {
        string json = JsonUtility.ToJson(item);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/measurementHistory/measurmentHistory";

        FirebaseManager.Instance.databaseReference.Child(path).Child(index.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });
    }
   
    public void SetMeasurementHistory(MeasurementHistoryItem item)
    {
        measurementHistory.measurmentHistory.Add(item);
    }
    public MeasurementModel getMeasurementData()
    {
        return measurementData;
    }
    public MeasurementHistory getMeasurementHistory()
    {
        return measurementHistory;
    }
    
    public void LoadHistory()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/workoutHistory", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/workoutHistory", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        historyData = (HistoryModel)LoadData(jsonData, typeof(HistoryModel));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }
            
        });
    }
    public void SaveHistory(HistoryTempleteModel item,int index)
    {
        string json = JsonUtility.ToJson(item);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/workoutHistory/exerciseTempleteModel";

        FirebaseManager.Instance.databaseReference.Child(path).Child(index.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });


    }
    public void LoadNotesHistory()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/exerciseNotes", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/exerciseNotes", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        notesHistory = (ExerciseNotesHistory)LoadData(jsonData, typeof(ExerciseNotesHistory));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }

        });
    }
    public void SaveNotesHistory(ExerciseNotesHistoryItem item, int index)
    {
        string json = JsonUtility.ToJson(item);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/exerciseNotes/exercises";

        FirebaseManager.Instance.databaseReference.Child(path).Child(index.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });
    }
    public ExerciseNotesHistory getNotesHistory()
    {
        return notesHistory;
    }
    
    public void LoadAchievements()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/achievements", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/achievements", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        achievementData = (AchievementData)LoadData(jsonData, typeof(AchievementData));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }
            else
            {
                TextAsset achievementJsonFile = Resources.Load<TextAsset>("data/achievement");
                string achievementJson = achievementJsonFile.text;
                this.achievementData = JsonUtility.FromJson<AchievementData>(achievementJson);

                FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("achievements")
                .SetRawJsonValueAsync(achievementJson).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("exercise added.");
                    }
                    else
                    {
                        Debug.LogError("Failed to add exercises: " + task.Exception);
                    }
                });
            }

        });
    }

    public void SaveAchievementData()
    {
        string json = JsonUtility.ToJson(achievementData);
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("achievements")
               .SetRawJsonValueAsync(json).ContinueWith(task =>
               {
                   if (task.IsCompleted)
                   {
                       Debug.Log("exercise added.");
                   }
                   else
                   {
                       Debug.LogError("Failed to add exercises: " + task.Exception);
                   }
               });
    }
    public void LoadPersonalBest()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/personalBest", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/personalBest", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        personalBestData = (PersonalBestData)LoadData(jsonData, typeof(PersonalBestData));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }
            else
            {
                TextAsset achievementJsonFile = Resources.Load<TextAsset>("data/personalBest");
                string achievementJson = achievementJsonFile.text;
                this.personalBestData = JsonUtility.FromJson<PersonalBestData>(achievementJson);

                FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("personalBest")
                .SetRawJsonValueAsync(achievementJson).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("exercise added.");
                    }
                    else
                    {
                        Debug.LogError("Failed to add exercises: " + task.Exception);
                    }
                });
            }

        });
    }

    public void SavePersonalBestData()
    {
        string json = JsonUtility.ToJson(personalBestData);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/personalBest";

        FirebaseManager.Instance.databaseReference.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });
    }




    public void LoadDataFromFirebase()
    {
        LoadHistory();
        LoadTemplateData();
        LoadExercises();
        LoadMeasurementData();
        LoadMeasurementHistory();
        LoadNotesHistory();
        LoadAchievements();
        LoadPersonalBest();
        LoadCoins();
        LoadUserStreak();
        gameTheme = LoadTheme();
    }

    public ExerciseData getExerciseData()
    {
        return this.exerciseData;
    }
    public AchievementData getAchievementData()
    {
        return this.achievementData;
    }
    public PersonalBestData getPersonalBestData()
    {
        return this.personalBestData;
    }
    public HistoryModel getHistoryData()
    {
        return this.historyData;
    }
    public TemplateData getTemplateData()
    {
        return this.templateData;
    }
   
    
   
    public void RemovePersonalBestData(PersonalBestDataItem item)
    {
        personalBestData.exercises.Remove(item);
    }
    public void SetPersonalBestData(PersonalBestDataItem item)
    {
        personalBestData.exercises.Add(item);
    }
    public void SetJoiningDate(DateTime date)
    {
        string dateInString= date.ToString("MMM / yyyy");
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("joiningDate")
            .SetValueAsync(dateInString).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    userSessionManager.Instance.joiningDate = dateInString;
                    Debug.Log("weekly goal seted");
                }
                else
                {
                    Debug.LogError("Failed to save weekly goal: " + task.Exception);
                }
            });
    }
    public void SetWeeklyGoal(int goal)
    {
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("weeklyGoal")
            .SetValueAsync(goal).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    userSessionManager.Instance.weeklyGoal = goal;
                    Debug.Log("weekly goal seted");
                }
                else
                {
                    Debug.LogError("Failed to save weekly goal: " + task.Exception);
                }
            });
    }



    //----------------------------------------------------------------------------------------------------------------------------------------------------------

    public void LoadCoins()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/coins", result => {
            print(result);
            if (result)
            {
                print("if");
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/coins", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string coin = data.Value.ToString();  // Directly get the value as string
                        userSessionManager.Instance.currentCoins = int.Parse(coin); ;
                        Debug.Log("Coins retrieved: " + coin);
                    }
                });
            }
            else
            {
                SetCoinsToFirebase(0);
            }
        });
    }
    public void SetCoinsToFirebase(int coin)
    {
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("coins")
                .SetValueAsync(coin).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        userSessionManager.Instance.currentCoins = coin;
                        Debug.Log("coin seted: " + coin);
                    }
                    else
                    {
                        Debug.LogError("Failed to save coin: " + task.Exception);
                    }
                });
    }
    public void LoadUserStreak()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/streak", result => {
            print(result);
            if (result)
            {
                print("if");
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/streak", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string streak = data.Value.ToString();  // Directly get the value as string
                        userSessionManager.Instance.userStreak = int.Parse(streak); ;
                        print("Streak retrieved: " + streak);
                    }
                });
            }
            else
            {
                SetUserStreakToFirebase(0);
            }
        });
    }
    public void SetUserStreakToFirebase(int streak)
    {
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("streak")
                .SetValueAsync(streak).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        userSessionManager.Instance.currentCoins = streak;
                        Debug.Log("coin seted: " + streak);
                    }
                    else
                    {
                        Debug.LogError("Failed to save coin: " + task.Exception);
                    }
                });
    }


    public void SetCharacterLevel(int level)
    {
        PreferenceManager.Instance.SetInt("CharacterLevel", level);
    }
    public int GetCharacterLevel()
    {
        return PreferenceManager.Instance.GetInt("CharacterLevel", 0);
    }


    public void SetWeightUnit(int unit)
    {
        PreferenceManager.Instance.SetInt("WeightUnit", unit);
    }
    public int GetWeightUnit()
    {
        return PreferenceManager.Instance.GetInt("WeightUnit", 1);
    }
   
    public void SetBadgeName(string name)
    {
        string badgeName= name.Replace(" ", "");
        PreferenceManager.Instance.SetString("BadgeName", badgeName);
    }
    public string GetBadgeName()
    {
        return PreferenceManager.Instance.GetString("BadgeName", "TheGorillaBadge");
    }

    public void SetBuyedCloths(int count)
    {
        PreferenceManager.Instance.SetInt("BuyedCloths", count);
    }
    public int GetBuyedCloths()
    {
        return PreferenceManager.Instance.GetInt("BuyedCloths", 0);
    }
    public void SetCreatedWorkoutTempleteCount(int count)
    {
        PreferenceManager.Instance.SetInt("CreatedWorkoutTempleteCount", count);
    }
    public int GetCreatedWorkoutTempleteCount()
    {
        return PreferenceManager.Instance.GetInt("CreatedWorkoutTempleteCount", 0);
    }
    public void SetRemoveFriendCount(int count)
    {
        PreferenceManager.Instance.SetInt("RemoveFriendCount", count);
    }
    public int GetRemoveFriendCount()
    {
        return PreferenceManager.Instance.GetInt("RemoveFriendCount", 0);
    }
    
    public void SetAddFriendCount(int count)
    {
        PreferenceManager.Instance.SetInt("AddFriendCount", count);
    }
    public int GetAddFriendCount()
    {
        return PreferenceManager.Instance.GetInt("AddFriendCount", 0);
    }




    public void AddItemToHistoryData(HistoryTempleteModel item)
    {
        historyData.exerciseTempleteModel.Add(item);
    }
    public void AddItemToTemplateData(DefaultTempleteModel item)
    {
        templateData.exerciseTemplete.Add(item);
    }
   
    
//-------------------------------------------------------------------------------------------------------------------------------------------------------------


    public void SetCurrentWeekStartDate(DateTime startDate)
    {
        string startDateString = startDate.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString("CurrentWeekStartDate", startDateString);
        PlayerPrefs.Save();
    }

    // Method to get the start date of the current week
    public DateTime GetCurrentWeekStartDate()
    {
        string startDateString = PlayerPrefs.GetString("CurrentWeekStartDate", "");

        if (!string.IsNullOrEmpty(startDateString))
        {
            return DateTime.Parse(startDateString);
        }

        // If no start date is set, return the start of this week as a default
        DateTime today = DateTime.Now;
        int daysSinceMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
        if (daysSinceMonday < 0) daysSinceMonday += 7;
        return today.AddDays(-daysSinceMonday);
    }

    // Method to check if the week has changed and update the start date automatically
    public void CheckAndUpdateWeekStartDate()
    {
        DateTime storedWeekStartDate = GetCurrentWeekStartDate();
        DateTime currentWeekStartDate = GetStartOfCurrentWeek();

        // If the stored week start date is not the same as the current week start date, update it
        if (storedWeekStartDate != currentWeekStartDate)
        {
            SetCurrentWeekStartDate(currentWeekStartDate);
        }
    }

    // Helper method to get the start date of the current week
    public DateTime GetStartOfCurrentWeek()
    {
        return DateTime.Now;
        //DateTime today = DateTime.Now;
        //int daysSinceMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
        //if (daysSinceMonday < 0) daysSinceMonday += 7;
        //return today.AddDays(-daysSinceMonday);
    }
    


//------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    
    public int GetCompletedAchievements()
    {
        int completedCount = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            bool isAchievementCompleted = achievement.achievementData.All(item => item.isCompleted);
            if (isAchievementCompleted)
            {
                completedCount++;
            }
        }

        return completedCount;
    }
    public int GetTotalAchievements()
    {
        return achievementData.achievements.Count;
    }
    public (int, int) GetRankedCompletedAchievements()
    {
        int totalCount = 0;
        int completedCount = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            if (achievement.achievementData.Count > 1)
            {
                bool isAchievementCompleted = achievement.achievementData.All(item => item.isCompleted);
                if (isAchievementCompleted)
                {
                    completedCount++;
                }
                totalCount++;
            }
        }

        return (completedCount, totalCount);
    }
    public (int, int) GetMilestoneCompletedAchievements()
    {
        int totalCount = 0;
        int completedCount = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            if (achievement.achievementData.Count == 1)
            {
                bool isAchievementCompleted = achievement.achievementData.All(item => item.isCompleted);
                if (isAchievementCompleted)
                {
                    completedCount++;
                }
                totalCount++;
            }
        }

        return (completedCount, totalCount);
    }
    public int GetTotalTrophys()
    {
        int count = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            count = count + achievement.achievementData.Count;
        }

        return count;
    }
    public int GetCompletedTrophys()
    {
        int count = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            foreach (AchievementTemplateDataItem item in achievement.achievementData)
            {
                if (item.isCompleted)
                {
                    count++;
                }
            }
        }

        return count;
    }
    public (int, int) GetRankedCompletedTrophys()
    {
        int total = 0;
        int count = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            if (achievement.achievementData.Count > 1)
            {
                foreach (AchievementTemplateDataItem item in achievement.achievementData)
                {
                    if (item.isCompleted)
                    {
                        count++;
                    }
                    total++;
                }
            }
        }

        return (count, total);
    }
    public (int, int) GetMilestoneCompletedTrophys()
    {
        int total = 0;
        int count = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            if (achievement.achievementData.Count == 1)
            {
                foreach (AchievementTemplateDataItem item in achievement.achievementData)
                {
                    if (item.isCompleted)
                    {
                        count++;
                    }
                    total++;
                }
            }
        }

        return (count, total);
    }


   

    
}