using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using System;

public class userSessionManager : GenericSingletonClass<userSessionManager>
{
    public string mProfileUsername;
    public string mProfileID;
    public bool mSidebar = false;
    public int currentWeight;
    public GameObject currentScreen;
    [Header("Theme Settings")]
    private Theme gameTheme;
    public TMP_FontAsset darkHeadingFont, darkTextFont;
    public TMP_FontAsset lightHeadingFont, lightTextFont;
    public Color darkBgColor, darkSearchBarColor, darkSearchIconColor;
    public Color lightBgColor, lightHeadingColor, lightButtonColor, lightTextColor;

    public DefaultTempleteModel selectedTemplete;
    private TemplateData templateData = new TemplateData();
    private HistoryModel historyData = new HistoryModel();
    //public PersonalBest

    private void Start()
    {
        darkHeadingFont= Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/Hoog0555/Hoog0555");
        darkTextFont = Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/K2D/K2D");
        lightHeadingFont = Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/Alexandria/Alexandria");
        lightTextFont = Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/Afacad/Afacad");
        darkBgColor = new Color32(99, 24, 24, 255);
        darkSearchBarColor = new Color32(81, 14, 14, 255);
        darkSearchIconColor = new Color32(127, 77, 77, 255);
        lightBgColor = new Color32(250, 249, 240, 255);
        lightHeadingColor = new Color32(150, 0, 0, 255);
        lightButtonColor = new Color32(218, 52, 52, 255);
        lightTextColor = new Color32(92, 59, 28, 255);
    }

    public void OnInitialize(string pProfileUsername, string pProfileID)
    {
        this.mProfileUsername = pProfileUsername;
        this.mProfileID = pProfileID;
        PreferenceManager.Instance.SetString("login_username", pProfileUsername);
        mSidebar = false;

        //LoadHistory();
        //gameTheme = LoadTheme();

    }

    public void OnResetSession()
    {
        this.mProfileUsername = null;
        this.mProfileID = null;
    }

    // Sets the current week's attendance dates
    public void AddGymVisit()
    {
        // Store gym visit with the current date
        string visitKey = "GymVisit_" + ApiDataHandler.Instance.GetCurrentWeekStartDate().ToString("yyyy-MM-dd");

        // Get the stored visit dates for the current week
        List<string> visits = PreferenceManager.Instance.GetStringList(visitKey) ?? new List<string>();

        // Get today's date in string format
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        // Add today's date if it's not already recorded
        if (!visits.Contains(today))
        {
            visits.Add(today);
            PreferenceManager.Instance.SetStringList(visitKey, visits);
        }
        UpdateStreak();
        
    }
    public bool HasMetWeeklyGoal()
    {
        // Get weekly goal
        int weeklyGoal = ApiDataHandler.Instance.GetWeeklyGoal();

        // Get the current week's attendance
        string visitKey = "GymVisit_" + ApiDataHandler.Instance.GetCurrentWeekStartDate().ToString("yyyy-MM-dd");
        List<string> visits = PreferenceManager.Instance.GetStringList(visitKey) ?? new List<string>();

        // Check if the user has met their weekly goal
        return visits.Count >= weeklyGoal;
    }
    // Gets the user's current streak
   

    // Checks and updates the streak
    public void UpdateStreak()
    {
        // Check and update the current week's start date if needed
        ApiDataHandler.Instance.CheckAndUpdateWeekStartDate();

        // Get the stored week start date and the current week start date
        DateTime lastWeekStartDate = ApiDataHandler.Instance.GetCurrentWeekStartDate();
        DateTime currentWeekStartDate = ApiDataHandler.Instance.GetStartOfCurrentWeek();

        // Check if it's a new week
        if (currentWeekStartDate > lastWeekStartDate)
        {
            // If the user met the weekly goal, increase the streak
            if (HasMetWeeklyGoal())
            {
                int currentStreak = ApiDataHandler.Instance.GetUserStreak();
                ApiDataHandler.Instance.SetUserStreak(currentStreak + 1);
            }
            else
            {
                // Reset streak if the user failed to meet the weekly goal
                ApiDataHandler.Instance.SetUserStreak(0);
            }

            // Update the date when the weekly goal was last set
            ApiDataHandler.Instance.SetCurrentWeekStartDate(currentWeekStartDate);
        }
    }



    //public void SaveTheme(Theme theme)
    //{
    //    PreferenceManager.Instance.SetInt("SelectedTheme", (int)theme);
    //    PreferenceManager.Instance.Save();
    //}
    //public Theme LoadTheme()
    //{
    //    int savedTheme = PlayerPrefs.GetInt("SelectedTheme", (int)Theme.Dark);
    //    return (Theme)savedTheme;
    //}


    //public void SaveTemplateData()
    //{
    //    string json = JsonUtility.ToJson(templateData);
    //    print(json);
    //    PreferenceManager.Instance.SetString("excerciseData", json);
    //    PreferenceManager.Instance.Save();
    //}

    //public void LoadTemplateData()
    //{
    //    if (PreferenceManager.Instance.HasKey("excerciseData"))
    //    {
    //        string json = PreferenceManager.Instance.GetString("excerciseData");
    //        templateData = JsonUtility.FromJson<TemplateData>(json);
    //        print(json);
    //    }
    //    else
    //    {
    //        templateData = new TemplateData();
    //        CreateRandomDefaultEntry();
    //    }
    //}

    //public void CreateRandomDefaultEntry()
    //{
    //    ExerciseTypeModel back1 = new ExerciseTypeModel
    //    {
    //        index = 0,
    //        name = "Deadlift (Barbell)",
    //        categoryName = "Glutes",
    //        exerciseType = ExerciseType.WeightAndReps,
    //        exerciseModel = new List<ExerciseModel>()
    //    };
    //    ExerciseTypeModel back2 = new ExerciseTypeModel
    //    {
    //        index = 0,
    //        name = "Seated narrow grip row (cable)",
    //        categoryName ="Lats",
    //        exerciseType = ExerciseType.WeightAndReps,
    //        exerciseModel = new List<ExerciseModel>()
    //    }; 
    //    ExerciseTypeModel chest = new ExerciseTypeModel
    //    {
    //        index = 0,
    //        name = "Bench Press (Barbell)",
    //        categoryName ="Chest",
    //        exerciseType = ExerciseType.WeightAndReps,
    //        exerciseModel = new List<ExerciseModel>()
    //    };
    //    ExerciseTypeModel running = new ExerciseTypeModel
    //    {
    //        index = 0,
    //        name = "Running",
    //        categoryName="Cardio",
    //        exerciseType = ExerciseType.TimeAndMiles,
    //        exerciseModel = new List<ExerciseModel>()
    //    };
    //    ExerciseTypeModel jumpRope = new ExerciseTypeModel
    //    {
    //        index = 0,
    //        name = "Jump Rope",
    //        categoryName = "Cardio",
    //        exerciseType = ExerciseType.RepsOnly,
    //        exerciseModel = new List<ExerciseModel>()
    //    };
    //    ExerciseTypeModel spiderCurls = new ExerciseTypeModel
    //    {
    //        index = 0,
    //        name = "Spider Curls",
    //        categoryName="Biceps",
    //        exerciseType = ExerciseType.WeightAndReps,
    //        exerciseModel = new List<ExerciseModel>()
    //    };
    //    ExerciseTypeModel bicepCulDumbbell = new ExerciseTypeModel
    //    {
    //        index = 0,
    //        name = "Bicep Curl (Dumbbell)",
    //        categoryName = "Biceps",
    //        exerciseType = ExerciseType.WeightAndReps,
    //        exerciseModel = new List<ExerciseModel>()
    //    };
    //    ExerciseTypeModel bicepCurlMachine = new ExerciseTypeModel
    //    {
    //        index = 0,
    //        name = "Bicep Curl (Machine)",
    //        categoryName = "Biceps",
    //        exerciseType = ExerciseType.WeightAndReps,
    //        exerciseModel = new List<ExerciseModel>()
    //    };
    //    ExerciseModel defaultExerciseModel1 = new ExerciseModel
    //    {
    //        setID = 1,
    //        previous = "-",
    //        weight = 0,
    //        rir = 0,
    //        reps = 0
    //    };
    //    back1.exerciseModel.Add(defaultExerciseModel1);
    //    back1.exerciseModel.Add(defaultExerciseModel1);
    //    back1.exerciseModel.Add(defaultExerciseModel1);
    //    back2.exerciseModel.Add(defaultExerciseModel1);
    //    chest.exerciseModel.Add(defaultExerciseModel1);
    //    running.exerciseModel.Add(defaultExerciseModel1);
    //    running.exerciseModel.Add(defaultExerciseModel1);
    //    jumpRope.exerciseModel.Add(defaultExerciseModel1);
    //    jumpRope.exerciseModel.Add(defaultExerciseModel1);
    //    jumpRope.exerciseModel.Add(defaultExerciseModel1);
    //    spiderCurls.exerciseModel.Add(defaultExerciseModel1);
    //    spiderCurls.exerciseModel.Add(defaultExerciseModel1);
    //    bicepCulDumbbell.exerciseModel.Add(defaultExerciseModel1);
    //    bicepCulDumbbell.exerciseModel.Add(defaultExerciseModel1);
    //    bicepCurlMachine.exerciseModel.Add(defaultExerciseModel1);
    //    bicepCurlMachine.exerciseModel.Add(defaultExerciseModel1);


    //    DefaultTempleteModel chestAndBack = new DefaultTempleteModel
    //    {
    //        templeteName = "Chest And Back",
    //        exerciseTemplete = new List<ExerciseTypeModel> { back1, back2, chest }
    //    };
    //    DefaultTempleteModel runingAndJumpRope = new DefaultTempleteModel
    //    {
    //        templeteName = "Runing And Jump Rope",
    //        exerciseTemplete = new List<ExerciseTypeModel> { running, jumpRope }
    //    };
    //    DefaultTempleteModel bicep = new DefaultTempleteModel
    //    {
    //        templeteName = "Biceps",
    //        exerciseTemplete = new List<ExerciseTypeModel> { bicepCulDumbbell, bicepCulDumbbell, spiderCurls }
    //    };
    //    templateData.exerciseTemplete.Clear();
    //    templateData.exerciseTemplete.Add(chestAndBack);
    //    templateData.exerciseTemplete.Add(runingAndJumpRope);
    //    templateData.exerciseTemplete.Add(bicep);

    //    SaveTemplateData();
    //}

    //public void SaveHistory()
    //{
    //    string json = JsonUtility.ToJson(historyData);
    //    PreferenceManager.Instance.SetString("historyData", json);
    //    PreferenceManager.Instance.Save();
    //}
    //public void LoadHistory()
    //{
    //    if (PreferenceManager.Instance.HasKey("historyData"))
    //    {
    //        string json = PreferenceManager.Instance.GetString("historyData");
    //        historyData = JsonUtility.FromJson<HistoryModel>(json);
    //    }
    //    else
    //    {
    //        historyData = new HistoryModel();
    //    }
    //}
}
