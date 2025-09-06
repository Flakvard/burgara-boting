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
        SetInputFieldText(inputText);
    }

    // Example of setting text programmatically
    public void SetInputFieldText(string newText)
    {
        myInputField.text = newText;
    }

    public void LoadTheScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
