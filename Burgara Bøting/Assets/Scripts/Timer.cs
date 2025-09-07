using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    [SerializeField]
    float timeLeftSeconds = 180;
    bool timerRunning = false;
    [SerializeField]
    TMP_Text timerText;
    public UnityEvent eventsOnTimerEnd;

    void Start()
    {
        timerRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerRunning)
        {
            timeLeftSeconds -= Time.deltaTime;

            if (timeLeftSeconds <= 0)
            {
                timeLeftSeconds = 0;
                StopTimer();
                eventsOnTimerEnd.Invoke();
            }

            timerText.text = $"{(int)(timeLeftSeconds / 60)}:{((int)(timeLeftSeconds % 60)).ToString("D2")}";
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }
}
