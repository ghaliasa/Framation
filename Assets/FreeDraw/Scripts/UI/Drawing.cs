using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using OpenCvSharp;
using Framation;
using AnotherFileBrowser.Windows;

public class Drawing : View
{
    [SerializeField] public Button _GoToHomeButton;
    [SerializeField] public Button _FinishButton;
    [SerializeField] public Button _DrawSkeletonButton;
    [SerializeField] public Button _DrawSkeletonTwoButton ;
    [SerializeField] public Button _ControlMaxDotButton ;
    [SerializeField] public Button _DeleteDotButton ;
    [SerializeField] public Button _BackToFramesUIButton ;
    [SerializeField] public Button _ClearBoardButton ;
    [SerializeField] public Button _ChoseImageButton ;
    [SerializeField] public GameObject _color_panel;
    [SerializeField] public GameObject _video_panel;
    [SerializeField] public GameObject _video_panel_2;
    [SerializeField] public GameObject _DrawSkeleton;
    [SerializeField] public GameObject _DrawSkeletonTwo;
    [SerializeField] public GameObject _Finish;
    [SerializeField] public GameObject _drawAnotherViewButton;
    [SerializeField] public GameObject _showButton;
    [SerializeField] public GameObject _text;
    [SerializeField] public GameObject _ControlMaxDot ;
    [SerializeField] public GameObject _DeleteDot ;
    [SerializeField] public GameObject _BackToFramesUI ;
    [SerializeField] public GameObject _ClearBoard ;
    [SerializeField] public GameObject _ChoseImage ;

    public static bool drawSkeltonMode; 
    public static bool drawSkelton2Mode; 
    public static bool deleteDotMode; 
    public static bool controlMaxDotMode; 
    public static bool moveToDrawingBoard; 
    public static bool finishMode; 
    public static bool vanishMode; 
    public static bool spriteChangedMode; 

    private void Update() {
        if(moveToDrawingBoard){
            _BackToFramesUI.SetActive(true);
            _color_panel.SetActive(true);
            _ClearBoard.SetActive(true);
            _DeleteDot.SetActive(false);
            _video_panel.SetActive(false);
            _video_panel_2.SetActive(false);
            _DrawSkeleton.SetActive(false);
            _DrawSkeletonTwo.SetActive(false);
            _Finish.SetActive(false);
            _drawAnotherViewButton.SetActive(false);
            _showButton.SetActive(false);
            _text.SetActive(false);
            _ControlMaxDot.SetActive(false);
            moveToDrawingBoard = false;
        }
    }
    
    public override void Initialize()
    {
        drawSkeltonMode    = false;
        drawSkelton2Mode   = false;
        moveToDrawingBoard = false;
        vanishMode         = false;
        spriteChangedMode  = false;
        
        _ClearBoardButton.onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("GoToBack");
            Drawable.drawable.ResetCanvas();
        });

        _GoToHomeButton.onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("GoToBack");
            ViewManager.Show<Home>();
        });

        _DrawSkeletonButton.onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("DrawAndFinish");
            _DeleteDot.SetActive(true);
            _video_panel.SetActive(true);
            _DrawSkeletonTwo.SetActive(true);
            _color_panel.SetActive(false);
            _ChoseImage.SetActive(false);
            _ClearBoard.SetActive(false);
            _video_panel_2.SetActive(false);
            _DrawSkeleton.SetActive(false);
            _ControlMaxDot.SetActive(false);
            _BackToFramesUI.SetActive(false);
            drawSkeltonMode = true;
        });

        _DrawSkeletonTwoButton.onClick.AddListener(()=>{
             Audio_Manager.Instance.PlaySound("DrawAndFinish");
             _video_panel_2.SetActive(true);
             _Finish.SetActive(true);
             _ControlMaxDot.SetActive(true);
             _text.SetActive(true);
             _ChoseImage.SetActive(false);
             _color_panel.SetActive(false);
             _ClearBoard.SetActive(false);
             _video_panel.SetActive(false);
             _DrawSkeletonTwo.SetActive(false);
             _BackToFramesUI.SetActive(false);
             _DeleteDot.SetActive(false);
             drawSkelton2Mode = true;
        });

        _FinishButton.onClick.AddListener(()=> {
            Audio_Manager.Instance.PlaySound("DrawAndFinish");
            _drawAnotherViewButton.SetActive(true);
            _showButton.SetActive(true);
            _color_panel.SetActive(false);
            _ClearBoard.SetActive(false);
            _ChoseImage.SetActive(false);
            _video_panel.SetActive(false);
            _video_panel_2.SetActive(false);
            _Finish.SetActive(false);
            _DrawSkeletonTwo.SetActive(false);
            _BackToFramesUI.SetActive(false);
            _ControlMaxDot.SetActive(false);
            _DeleteDot.SetActive(false);
            _text.SetActive(false);
            finishMode = true;
        });

        _drawAnotherViewButton.GetComponent<Button>().onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("GoToBack");
            _color_panel.SetActive(true);
            _ClearBoard.SetActive(true);
            _ChoseImage.SetActive(true);
            _DrawSkeleton.SetActive(true);
            _video_panel.SetActive(false);
            _video_panel_2.SetActive(false);
            _DeleteDot.SetActive(false);
            _DrawSkeletonTwo.SetActive(false);
            _Finish.SetActive(false);
            _drawAnotherViewButton.SetActive(false);
            _showButton.SetActive(false);
            _BackToFramesUI.SetActive(false);
            _text.SetActive(false);
            _ControlMaxDot.SetActive(false);     
            PenTool.penTool.doCopySkeleton1 = true;
            Drawable.drawable.drawable_texture.LoadImage(File.ReadAllBytes(Home.imagesPath+"\\" + ( PenTool.frameId - 1 ) + ".png"));
            Drawable.drawable.drawable_texture.Apply();
            Drawable.drawable.drawable_sprite  = Sprite.Create(
                Drawable.drawable.drawable_texture,
                new UnityEngine.Rect(0, 0,
                Drawable.drawable.drawable_texture.width,
                Drawable.drawable.drawable_texture.height),
                Vector2.zero
            );
            Drawable.drawable.drawable_texture = Drawable.drawable.drawable_sprite.texture;
            PenTool.frameId = PenTool.frameId - 1  ;
            vanishMode    = true;       
        });

        _ControlMaxDotButton.GetComponent<Button>().onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("DrawAndFinish");
            controlMaxDotMode = true;
        });

        _BackToFramesUIButton.GetComponent<Button>().onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("DrawAndFinish");
            System.IO.File.WriteAllBytes(Scroll.spritePath, Drawable.drawable.drawable_texture.EncodeToPNG()); // save modified frame 
            Drawable.drawable.go.SetActive(true);
            Drawable.drawable.ResetCanvas();
            ViewManager.Show<Frames>();
        });

        _DeleteDotButton.GetComponent<Button>().onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("DrawAndFinish");
            deleteDotMode = true;
        });

        _showButton.GetComponent<Button>().onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("GoToBack");
            ViewManager.Show<Frames>();
        });

        _ChoseImageButton.GetComponent<Button>().onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("GoToBack");
            var bp = new BrowserProperties();
            bp.filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            bp.filterIndex = 0;

            new FileBrowser().OpenFileBrowser(bp, path =>
            {
                
                Texture2D texture = new Texture2D(20,20);
                texture.LoadImage(File.ReadAllBytes(path));
                Mat outputTexture = new Mat();
                Cv2.Resize(
                    OpenCvSharp.Unity.TextureToMat(texture),
                    outputTexture,
                    new Size(1300, 925)
                );
                Drawable.drawable.drawable_texture.LoadImage(OpenCvSharp.Unity.MatToTexture(outputTexture).EncodeToPNG());
                Drawable.drawable.drawable_texture.Apply();
                Drawable.drawable.drawable_sprite  = Sprite.Create(
                    Drawable.drawable.drawable_texture,
                    new UnityEngine.Rect(0, 0,
                    Drawable.drawable.drawable_texture.width,
                    Drawable.drawable.drawable_texture.height),
                    Vector2.zero
                );
                Drawable.drawable.drawable_texture = Drawable.drawable.drawable_sprite.texture;
                
            });


        });
    }
}