using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class PlayCutscene : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, "cutscene.mp4");
        videoPlayer.Play();
    }
}
