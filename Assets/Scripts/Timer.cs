using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    // Start is called before the first frame update
    public float timeStart = 0;
    public TMP_Text timer;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timeStart += Time.deltaTime;
        DisplayTime(timeStart);
    }

    void DisplayTime(float timeToDisplay)
    {
    timeToDisplay += 1;

    float minutes = Mathf.FloorToInt(timeToDisplay / 60);
    float seconds = Mathf.FloorToInt(timeToDisplay % 60);

    timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
}

}
