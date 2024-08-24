using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static EditWorkoutTemplete;

public class ExerciseItem : MonoBehaviour, IPointerClickHandler, ItemController
{
    [SerializeField]
    private Text exerciseNameText;
    [SerializeField]
    private Text categoryNameText;
    [SerializeField]
    private Image exerciseImage;

    void Start()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void onInit(Dictionary<string, object> data)
    {
        if (data.TryGetValue("data", out object exerciseDataObj) && exerciseDataObj is ExerciseDataItem exerciseData)
        {
            exerciseNameText.text = exerciseData.exerciseName;
            categoryNameText.text = exerciseData.category;
        }
    }

}
