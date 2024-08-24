using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkoutSet : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    // Start is called before the first frame update
    void Awake()
    {
        numberText.text = (transform.parent.childCount-1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}