using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveNumFrameRate : MonoBehaviour
{
    [SerializeField] TMP_InputField input;

    public void SaveData(){
        if(input.text == ""){
            input.text = "24";
        }
        Video.frameRate = input.text;
    }
}