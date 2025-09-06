using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void LoadTheScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
