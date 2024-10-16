using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishWorkoutPopup : MonoBehaviour,IPrefabInitializer
{
    public Button saveButton, discardButton;
    private GameObject workoutScreen;
    private DefaultTempleteModel modifiedModel;
    Action<object> callback;
    public void InitPrefab(Action<List<object>> onFinish, List<object> data)
    {
        workoutScreen = (GameObject)data[0];
        modifiedModel = (DefaultTempleteModel)data[1];
        this.callback = (Action<object>)data[2];
    }
    private void Start()
    {
        saveButton.onClick.AddListener(Save);
        discardButton.onClick.AddListener(Discard);
    }
    void Discard()
    {
        PopupController.Instance.ClosePopup("FinishWorkoutPopup");
        StateManager.Instance.HandleBackAction(workoutScreen);
        StateManager.Instance.OpenFooter(null, null, false);
    }
    void Save()
    {
        int index = GetIndexByTempleteName(modifiedModel.templeteName);
        ApiDataHandler.Instance.RemoveItemFromTempleteData(index);
        ApiDataHandler.Instance.InsertItemToTemplateData(index, modifiedModel);//.exerciseTemplete.Insert(index, modifiedModel);
        ApiDataHandler.Instance.SaveTemplateData();
        PopupController.Instance.ClosePopup("FinishWorkoutPopup");
        StateManager.Instance.HandleBackAction(workoutScreen);
        StateManager.Instance.OpenFooter(null, null, false);
        this.callback.Invoke(null);
    }

    public int GetIndexByTempleteName(string name)
    {
        return ApiDataHandler.Instance.getTemplateData().exerciseTemplete.FindIndex(t => t.templeteName == name);
    }
}
