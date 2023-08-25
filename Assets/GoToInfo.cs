using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToInfo : MonoBehaviour
{
    public void GoToOption()
    {
        Audio_Manager.Instance.PlaySound("About");
        ViewManager.Show<About>();
    }
}
