using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using SFB;
public class Video : View
{
    
    [SerializeField] private Button _GoToHomeButton;
    [SerializeField] private Button _BackToFramesButton;
    [SerializeField] private Button _SaveVideoButton;

    public static string frameRate = "24";
    public static string quality   = "854x480";

    public override void Initialize()
    {
        _GoToHomeButton.onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("GoToBack");
            Drawing.vanishMode = true;
            ViewManager.Show<Home>();
        });

        _BackToFramesButton.onClick.AddListener(()=>{
             Audio_Manager.Instance.PlaySound("GoToBack");
             ViewManager.Show<Frames>();
        });

        _SaveVideoButton.onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("GoToBack");
            string[] selectedPath = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);

            if (!string.IsNullOrEmpty(selectedPath[0]))
            {
                string ffmpegPath = @"ffmpeg"; // Replace with your FFmpeg executable path
                ProcessStartInfo startInfo       = new ProcessStartInfo();
                startInfo.FileName               = ffmpegPath;
                startInfo.Arguments = $"-y -framerate {frameRate} -i \"{Home.imagesPath}\\%d.png\" -s {quality} \"{selectedPath[0]}\\framation.mp4\"";
                startInfo.UseShellExecute        = false;
                startInfo.CreateNoWindow         = true;
                startInfo.RedirectStandardOutput = true;
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
                print("Video created successfully! in >  " + selectedPath[0]);
            }    
        });
    }
}