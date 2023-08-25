using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;
using System.Diagnostics;

public class VideoPlayerController : MonoBehaviour
{
    public RawImage videoScreen;
    public RenderTexture renderTexture;

    private VideoPlayer videoPlayer;

    void OnEnable()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Home.sketchVideoPath;

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        Texture2D texture = new Texture2D(1300, 925);
        texture.LoadImage(File.ReadAllBytes(Home.renderTexturePath));
        Graphics.CopyTexture(texture, renderTexture);
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.targetMaterialRenderer = videoScreen.GetComponent<Renderer>();
        videoPlayer.targetMaterialProperty = "_MainTex";//
        videoPlayer.isLooping = true;
        videoPlayer.Prepare();
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.K)){
            videoPlayer.Play();
        }
        if(Input.GetKeyDown(KeyCode.S)){
            videoPlayer.Pause();
        }
    }

    public void PlayVideo()
    {
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Video playback has ended, perform any necessary actions here
    }
}