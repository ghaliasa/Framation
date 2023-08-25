using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayback : MonoBehaviour
{
   public VideoClip[] videoClips;    // Array of VideoClips to play
   private VideoPlayer videoplayer;  // Reference to the VideoPlayer component
   private int videoClipIndex;       // Index of the currently selected video clip
   private void Awake()
   {
        videoplayer = GetComponent<VideoPlayer>();  // Get a reference to the VideoPlayer component attached to the same GameObject
   }
  
   void Start()
   {

        videoplayer.clip = videoClips[0];  // Set the initial video clip to play
   }
   public void playNext()
   {
        videoClipIndex++;  // Increment the index to move to the next video clip

        if (videoClipIndex >= videoClips.Length)
        {
            videoClipIndex = videoClipIndex % videoClips.Length;  // If the index exceeds the array length, wrap around to the beginning
        }

        videoplayer.clip = videoClips[videoClipIndex];  // Set the current video clip to play
   }

   public void playPrevious()
   {
        videoClipIndex--;  // Decrement the index to move to the previous video clip

        if (videoClipIndex < 0)
        {
            videoClipIndex = videoClips.Length - 1;  // If the index becomes negative, wrap around to the last video clip
        }

        videoplayer.clip = videoClips[videoClipIndex];  // Set the current video clip to play
   }

   public void playPause()
   {
        if (videoplayer.isPlaying)
        {
          //   print("pause pause pause");
            videoplayer.Pause();  // If the video is playing, pause it
        }
        else
        {
            videoplayer.Play();  // If the video is paused, play it
        }
   }
}
