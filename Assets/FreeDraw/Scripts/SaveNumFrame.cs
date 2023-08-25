using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveNumFrame : MonoBehaviour
{
    [SerializeField] TMP_InputField input;

    public void SaveData(){
        if(input.text == ""){
            input.text = "3";  // #Show Error message
        }
        PenTool.frameNum = int.Parse(input.text);
    }
}