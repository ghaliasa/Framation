using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;

public class Home: View
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _secondButton;
    [SerializeField] private Button _thirdButton;
    [SerializeField] private GameObject _color_panel;
    [SerializeField] private GameObject _video_panel;
    [SerializeField] private GameObject _video_panel_2;
    [SerializeField] private GameObject _DrawSkeleton;
    [SerializeField] private GameObject _DrawSkeletonTwo;
    [SerializeField] private GameObject _Finish;
    [SerializeField] private GameObject _drawAnotherViewButton;
    [SerializeField] private GameObject _showButton;
    [SerializeField] private GameObject _text;
    [SerializeField] private GameObject _ControlMaxDot ;
    [SerializeField] private GameObject _DeleteDot ;
    [SerializeField] private GameObject _ClearBoard ;
    [SerializeField] private GameObject _ChoseImage ;

    public static string whiteBoardPath;
    public static string sketchVideoPath;
    public static string imagesPath;
    public static string renderTexturePath;

    public override void Initialize()
    { 
        string basePath  = Application.dataPath + "\\Resources";
        string fileName  = "whiteBoard.png";
        string videoName = "sketch.mp4";
        string renderTextureName = "sketchTexture.renderTexture";
        whiteBoardPath     = Path.Combine(basePath, fileName);
        sketchVideoPath    = Path.Combine(Application.dataPath, videoName);
        renderTexturePath  = Path.Combine(basePath, renderTextureName);
        imagesPath = Application.dataPath + "\\images";
        _startButton.onClick.AddListener(()=> {
            Audio_Manager.Instance.PlaySound("Draw");
            
            if (!Directory.Exists(imagesPath)){
                Directory.CreateDirectory(imagesPath);
            }else{
                Directory.Delete(imagesPath, true);
                Directory.CreateDirectory(imagesPath);
            }
            if (!File.Exists(sketchVideoPath)){
                CreateEmptyVideo(sketchVideoPath,10,10,0);
            }
            if (!File.Exists(renderTexturePath)){
                CreateFile(renderTexturePath);
            }
            if (!File.Exists(whiteBoardPath)){
                int width = 1300;
                int height = 925;

                Texture2D whiteImage = new Texture2D(width, height);

                Color[] pixels = new Color[width * height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.white;
                }

                whiteImage.SetPixels(pixels);
                whiteImage.Apply();
                
                byte[] imageBytes = whiteImage.EncodeToPNG();
                File.WriteAllBytes(whiteBoardPath, imageBytes);
            }
            ViewManager.Show<Drawing>();
            _color_panel.SetActive(true);
            _ClearBoard.SetActive(true);
            _DrawSkeleton.SetActive(true);
            _ChoseImage.SetActive(true);
            _DeleteDot.SetActive(false);
            _video_panel.SetActive(false);
            _video_panel_2.SetActive(false);
            _DrawSkeletonTwo.SetActive(false);
            _Finish.SetActive(false);
            _drawAnotherViewButton.SetActive(false);
            _showButton.SetActive(false);
            _text.SetActive(false);
            _ControlMaxDot.SetActive(false);

            PenTool.frameId = 0 ;
        });

        _secondButton.onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("Options");
            ViewManager.Show<Options>();
        });
        
        _thirdButton.onClick.AddListener(()=> {
            Audio_Manager.Instance.PlaySound("About");
            ViewManager.Show<About>();
        }); 
    }

    public void CreateEmptyVideo(string filePath, int width, int height, int durationInSeconds)
    {
        string ffmpegPath = @"ffmpeg";
        string command = $"-f lavfi -i color=c=black:s={width}x{height}:d={durationInSeconds} -c:v vp9 -t {durationInSeconds} \"{filePath}\"";

        ProcessStartInfo processInfo = new ProcessStartInfo(ffmpegPath)
        {
            Arguments = command,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        Process process = new Process
        {
            StartInfo = processInfo
        };

        process.Start();
        process.WaitForExit();
    }

    public void CreateFile(string filePath)
    {
        // Create the file
        FileStream fileStream = File.Create(filePath);
        fileStream.Close();

        // Optional: Rename the file with the desired extension
        string newFilePath = Path.ChangeExtension(filePath, ".renderTexture");
        File.Move(filePath, newFilePath);
    }
}