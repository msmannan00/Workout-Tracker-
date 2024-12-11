using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class UserNameController : MonoBehaviour, PageController
{
    public TMP_InputField userNameInput;
    public TextMeshProUGUI messageText;
    public Button nextButton;

    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        nextButton.onClick.AddListener(Next);
    }
    public void Next()
    {
        string stringWithoutSpaces = userNameInput.text.Replace(" ", "");
        int lengthWithoutSpaces = stringWithoutSpaces.Length;
        if (lengthWithoutSpaces < 4)
        {
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Username: minimum 4 characters", 2);
        }
        else
            CheckAndStoreUsername(userNameInput.text,userSessionManager.Instance.mProfileID);
    }
    public void CheckAndStoreUsername(string username,string userID)
    {
        StartCoroutine(CheckUsernameExists(username,userID));
    }

    private IEnumerator CheckUsernameExists(string username,string userID)
    {
        // Reference to the 'usernames' node in Firebase Realtime Database
        var usernameRef = FirebaseDatabase.DefaultInstance.GetReference("usernames");

        // Check if the username exists in the database
        var checkUserTask = usernameRef.Child(username).GetValueAsync();

        yield return new WaitUntil(() => checkUserTask.IsCompleted);

        if (checkUserTask.Exception != null)
        {
            Debug.LogError("Error checking username: " + checkUserTask.Exception);
            yield break;
        }

        // If the username doesn't exist, it's unique
        if (!checkUserTask.Result.Exists)
        {
            // Store the username in the database
            StoreUsername(username,userID);
        }
        else
        {
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Username is already taken", 2);
            Debug.Log("Username is already taken.");
            // Inform the user that the username is already taken
        }
    }

    // Function to store the username in the Firebase Database
    private void StoreUsername(string username,string userID)
    {
        FirebaseDatabase.DefaultInstance.GetReference("users")
            .Child(userID)  // Save under the user's UID
            .Child("username").SetValueAsync(username)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Username saved successfully under user UID.");
                }
                else
                {
                    GlobalAnimator.Instance.ShowTextMessage(messageText, "Failed to store username", 2);
                    Debug.LogError("Error saving username under UID: " + task.Exception);
                    return;
                }
            });

        // Save the username under the 'usernames/{username}' path to ensure uniqueness
        FirebaseDatabase.DefaultInstance.GetReference("usernames")
            .SetValueAsync(username)  // Use `true` or any value to mark it as taken
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    ApiDataHandler.Instance.userName=username;
                    Dictionary<string, object> mData = new Dictionary<string, object> { { "data", true } };
                    StateManager.Instance.OpenStaticScreen("profile", gameObject, "weeklyGoalScreen", mData);
                    PreferenceManager.Instance.SetBool("FirstTimePlanInitialized_" /*+ userSessionManager.Instance.mProfileUsername*/, false);


                    Debug.Log("Username saved successfully under usernames path.");
                }
                else
                {
                    Debug.LogError("Error saving username under usernames path: " + task.Exception);
                }
            });











        //var usernameRef = FirebaseDatabase.DefaultInstance.GetReference("usernames");
        //usernameRef.Child(username).SetValueAsync(true).ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsCompleted)
        //    {
        //        Dictionary<string, object> mData = new Dictionary<string, object> { { "data", true } };
        //        StateManager.Instance.OpenStaticScreen("profile", gameObject, "weeklyGoalScreen", mData);
        //        PreferenceManager.Instance.SetBool("FirstTimePlanInitialized_" /*+ userSessionManager.Instance.mProfileUsername*/, false);
        //        Debug.Log("Username stored successfully!");
        //    }
        //    else
        //    {
        //        GlobalAnimator.Instance.ShowTextMessage(messageText, "Failed to store username", 2);
        //        Debug.LogError("Failed to store username: " + task.Exception);
        //    }
        //});
    }
}
