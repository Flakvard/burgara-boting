using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadEndScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadTheScene(string scene)
    {
        // Make sure we save before loading
        SceneManager.LoadScene(scene);
    }
}
