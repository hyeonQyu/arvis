using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [SerializeField]
    private RawImage _display;
    private WebCam _webCam;
    private HandDetector _handDetector;

    private void Start()
    {
        _webCam = _display.GetComponent<WebCam>();
        _handDetector = _webCam.HandDetector;

        Client.Setup();
    }

    public void DetectHand()
    {
        gameObject.SetActive(false);

        // 이미지를 JPG로 변환
        Texture2D img = (Texture2D)_display.texture;
        byte[] jpg = img.EncodeToJPG();
        Debug.Log("jpg " + jpg.Length);
        
        // 클라이언트 쓰레드에게 전송할 jpg 넘김
        Client.Connect(jpg, _handDetector);
    }
}
