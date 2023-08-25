using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropDownHandler : MonoBehaviour
{
    public TextMeshProUGUI Text;
    private string selectedValue;

    public void DropDown(int index)
    {
        switch (index)
        {
            case 0:
                selectedValue = "426x240";
                Text.gameObject.SetActive(true);
                break;
            case 1:
                selectedValue = "640x360";
                Text.gameObject.SetActive(false);
                break;
            case 2:
                selectedValue = "854x480";
                Text.gameObject.SetActive(false);
                break;
            case 3:
                selectedValue = "1280x720";
                Text.gameObject.SetActive(false);
                break;
        }
        
        SaveSelectedValue();
    }

    private void SaveSelectedValue()
    {
        PlayerPrefs.SetString("SelectedValue", selectedValue);
        Video.quality = selectedValue;
    }
}