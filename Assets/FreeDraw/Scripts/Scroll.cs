using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Framation;

public class Scroll : MonoBehaviour
{
    [SerializeField] private Transform scrollContent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject _GoToHome;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button previousPageButton;
    
    public static byte[] spriteToChange;
    public static string spritePath;
    public static bool spriteIsChanged;
    private static string folderPath  ;
    private static string[] imagePaths;
    private int currentPage = 0;
    private int imagesPerPage = 10;

    public void Start()
    {
        folderPath = Application.dataPath + "\\images";
        buttonPrefab = Instantiate(scrollContent.GetChild(0).gameObject);
        scrollContent.GetChild(0).gameObject.SetActive(false);

        nextPageButton.onClick.AddListener(NextPage);
        previousPageButton.onClick.AddListener(PreviousPage);

    }

    public void OnEnable()
    {
        LoadImages();
        UpdateButtons();
    }

    
    public static void LoadImages()
    {
        folderPath = Application.dataPath + "\\images";
        imagePaths = Directory.GetFiles(folderPath, "*.png");
        imagePaths = imagePaths.OrderBy(path => int.Parse(Path.GetFileNameWithoutExtension(path))).ToArray();
    }

    private void UpdateButtons()
    {
        ClearAllChildren(scrollContent);

        int startIndex = currentPage * imagesPerPage;
        int endIndex = Mathf.Min(startIndex + imagesPerPage, imagePaths.Length);

        for (int i = startIndex; i < endIndex; i++)
        {
            string imagePath = imagePaths[i];
            Texture2D imageTexture = LoadImageFromDisk(imagePath);

            GameObject button = Instantiate(buttonPrefab, scrollContent);
            button.GetComponentInChildren<Text>().text = "frame " + int.Parse(Path.GetFileNameWithoutExtension(imagePath));
            button.GetComponentInChildren<Image>().sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), Vector2.zero);
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                ViewManager.Show<Drawing>();
                Drawing.moveToDrawingBoard = true;
                _GoToHome.SetActive(false);
                Drawable.drawable.go.SetActive(false);
                Drawable.drawable.isDrawing = true;
                Drawable.drawable.changeTexture = true;
                spriteToChange = imageTexture.EncodeToPNG();
                spritePath = imagePath;
                spriteIsChanged = true;
            });
        }

        nextPageButton.interactable     = currentPage < (imagePaths.Length - 1) / imagesPerPage;
        previousPageButton.interactable = currentPage > 0;
    }

    private void NextPage()
    {
        currentPage++;
        UpdateButtons();
    }

    private void PreviousPage()
    {
        currentPage--;
        UpdateButtons();
    }

    void ClearAllChildren(Transform parent)
    {
        int childCount = parent.childCount;
        for (int i = childCount - 1; i >= 1; i--)
        {
            Transform child = parent.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private Texture2D LoadImageFromDisk(string imagePath)
    {
        byte[] imageData = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }
}