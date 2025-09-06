using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadScene : MonoBehaviour
{
    public TMP_InputField myInputField;

    void Start()
    {
        // Subscribe to the OnEndEdit event
        myInputField.onEndEdit.AddListener(ProcessInput);
    }


    void ProcessInput(string inputText)
    {
        Debug.Log("User entered: " + inputText);

        // Save input globally
        PlayerStats.Name = inputText;
    }

    public void LoadTheScene(string scene)
    {
        // Make sure we save before loading
        PlayerStats.Name = myInputField.text;
        SceneManager.LoadScene(scene);
    }
}