using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcam : MonoBehaviour
{
    private RawImage _background;
    private WebCamTexture _webCamTexture;

    // Start is called before the first frame update
    void Start()
    {
        _background = this.GetComponent<RawImage>();
        _webCamTexture = new WebCamTexture(Screen.width, Screen.height);
        _background.texture = _webCamTexture;
        _background.material.mainTexture = _webCamTexture;
        _webCamTexture.Play();
    }
}
