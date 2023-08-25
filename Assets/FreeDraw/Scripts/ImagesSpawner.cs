using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ImagesSpawner : MonoBehaviour
{
    public Sprite[] sprites;
    public Text header;

    void SetupSprites() {
        sprites = Resources.LoadAll("Sprites", typeof(Sprite)).Cast<Sprite>().ToArray();

    }

    void SetupText(){

    }

    private void start(){
        SetupSprites();
        SetupText();
        foreach (var item in sprites)
        {
            SpawnImage(item);
        }
    }    

    void SpawnImage(Sprite sprites)
    {

    }
}
