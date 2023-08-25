
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : View
{
    [SerializeField] private Button _GoToHomeButton;

    public override void Initialize()
    {
        _GoToHomeButton.onClick.AddListener(()=>{
            Audio_Manager.Instance.PlaySound("GoToBack");
            ViewManager.ShowLast();
        });
    }
}