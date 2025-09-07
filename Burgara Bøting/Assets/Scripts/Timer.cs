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
    public UnityEvent eventsOnStart;
    public UnityEvent eventsOnTimerEnd;
    

    void Start()
    {
        timerRunning = true;
        // Do NOT auto-start. Wait for server "start" command (or local StartTimer()).
        //timerRunning = false;
        // Draw initial text
        if (timerText != null)
            timerText.text = $"{(int)(timeLeftSeconds / 60)}:{((int)(timeLeftSeconds % 60)).ToString("D2")}";
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
        eventsOnStart.Invoke();
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

   // Let the server set the round length
    public void ResetTimer(float seconds)
    {
        timeLeftSeconds = Mathf.Max(0, seconds);
        if (timerText != null)
            timerText.text = $"{(int)(timeLeftSeconds / 60)}:{((int)(timeLeftSeconds % 60)).ToString("D2")}";
    }

    public float GetTimeLeft() => timeLeftSeconds;

}
