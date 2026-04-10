using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoToNextLevel : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName; // Set this in Inspector

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video Finished!");
        SceneManager.LoadScene(nextSceneName);
    }
}