using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [SerializeField]
    private RawImage _display;
    private WebCam _webCam;

    private void Start()
    {
        _webCam = _display.GetComponent<WebCam>();

        Client.Setup();
    }

    public void DetectHand()
    {
        gameObject.SetActive(false);
        Client.Connect();

        SendImageToServer();
        // 손 인식이 성공했는지 여부를 체크
        _webCam.HandDetector.IsInitialized = Client.Receive(_webCam.SkinDetector.HandBoundary);

        Client.Close();
    }

    private void SendImageToServer()
    {
        Texture2D img = (Texture2D)_display.texture;
        byte[] jpg = img.EncodeToJPG();
        Debug.Log("jpg " + jpg.Length);

        // jpg 전송
        Client.Send(BitConverter.GetBytes(jpg.Length));
        Client.Send(jpg);
    }
}
