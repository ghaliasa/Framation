using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Splash : MonoBehaviour
{
    float time,second;
    [SerializeField] public Image FillImage;

    void Start()
    {
        second = 5;
        Invoke("LoadGame" , 5f);
    }
    void Update()
    {
        if(time < 5)
        {
            time += Time.deltaTime;
            FillImage.fillAmount = time / second;
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
    
}