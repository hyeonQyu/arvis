using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCam : MonoBehaviour
{
    private WebCamTexture _cam;
    private RawImage _display;
    private Texture2D _screen;
    [SerializeField]
    private Canvas _canvas;

    private int _frame = 0;

    private void Start()
    {
        _display = GetComponent<RawImage>();

        // 안드로이드 폰에서 실행시키기 위한 회전
        _canvas.transform.rotation = Quaternion.Euler(0, 0, 90);

        _cam = new WebCamTexture(Screen.width, Screen.height, 60);
        _display.texture = _cam;
        _cam.Play();

        Client.Setup();
    }
    
    private void Update()
    {
        if(_frame % 5 == 0)
            Client.Send();
        else if(_frame % 5 == 3)
            Client.Receive();
        _frame++;
        //_frame++;

        //if(_frame % 30 != 0)
        //    return;

        //_screen = new Texture2D(_cam.width, _cam.height);
        //_screen.SetPixels(_cam.GetPixels());
        //_screen.Apply();
        //_screen = ScaleTexture()
    }

    private Texture ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / targetWidth);
        float incY = (1.0f / targetHeight);
        for(int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }
}