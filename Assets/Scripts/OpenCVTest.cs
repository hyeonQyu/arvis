using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;


public class OpenCVTest:MonoBehaviour
{
    public Button btnFlipImage;
    public Image imageBlack;
    public Text txt;

    [DllImport("opencv410")]
    private static extern void FlipImage(ref Color32[] rawImage, int width, int height);

    // Start is called before the first frame update
    void Start()
    {
        btnFlipImage.onClick.AddListener(CallFlipImage);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CallFlipImage()
    {
        var blackImage = imageBlack.sprite.texture.GetPixels32();
        FlipImage(ref blackImage, 800, 800);
        imageBlack.sprite.texture.SetPixels32(blackImage);
        imageBlack.sprite.texture.Apply();
    }
}