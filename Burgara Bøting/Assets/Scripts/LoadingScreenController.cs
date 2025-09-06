using UnityEngine;

public class LoadingScreenController : MonoBehaviour
{
    public AudioClip LoadingMusic;

    void Start() {
        SoundManager.Instance.PlayMusic(LoadingMusic);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
