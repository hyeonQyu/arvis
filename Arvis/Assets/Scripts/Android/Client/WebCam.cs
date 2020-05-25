using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using OpenCvSharp;
using System.IO;

public class WebCam : MonoBehaviour
{
    private static WebCamTexture _cam;
    private RawImage _display;
    [SerializeField, Header("Virtual Camera")]
    private Camera _virtualCamera;
    [SerializeField, Header("Virtual World(Render texture)")]
    private RenderTexture _vWorld;
    [SerializeField, Header("Virtual Display")]
    private RawImage _vDisplay;

    // 가상 손의 손가락
    [SerializeField, Header("Hand")]
    private GameObject _hand;

    // Resize할 크기
    private const int _width = 16 * 15;
    private const int _height = 9 * 15;

    // 손 인식에 사용될 프레임 이미지
    private Mat _imgFrame;
    private Mat _imgMask;
    private Mat _imgHand;

    private SkinDetector _skinDetector;
    private HandDetector _handDetector;
    private HandManager _handManager;

    private int _frame = 0;
    public static bool IsAndroid { get; private set; }

    private void Start()
    {
    #if UNITY_EDITOR    // for PC
        IsAndroid = false;
    #elif UNITY_ANDROID // for Android
        IsAndroid = true;
    #endif

        _display = GetComponent<RawImage>();
        _display.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);

        _vDisplay.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        _vDisplay.texture = _vWorld;

        if(!IsAndroid)
        {
            _display.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        _vWorld.width = Screen.width;
        _vWorld.height = Screen.height;

        // for Updating camera field of view
        _virtualCamera.enabled = false;
        _virtualCamera.enabled = true;

        // 원본 화면 = _cam
        _cam = new WebCamTexture(Screen.width, Screen.height, 60);

        //_display.texture = _cam;
        _cam.Play();

        _skinDetector = new SkinDetector();
        _handDetector = new HandDetector();

        // no resize : _cam.width, _cam.height
        // resize : _width, _height
        _handManager = new HandManager(_hand, _display, _width, _height);

        Client.Setup();
    }

    private void Update()
    {
        _frame++;
        if (_frame < 120)
            return;

        _imgFrame = OpenCvSharp.Unity.TextureToMat(_cam);

        SendJpgInClientThread();

        Texture2D texture = new Texture2D(_width, _height);
        Cv2.Resize(_imgFrame, _imgFrame, new Size(_width, _height));

        _handDetector.IsCorrectDetection = true;

        // 피부색으로 마스크 이미지를 검출
        _imgMask = _skinDetector.GetSkinMask(_imgFrame, _skinDetector.IsExtractedSkinColor);

        // 손의 점들을 얻음
        _imgHand = _handDetector.GetHandLineAndPoint(_imgFrame, _imgMask);

        // 손 인식이 정확하지 않으면 프레임을 업데이트 하지 않음
        // if(!_handDetector.IsCorrectDetection)
        // {
        //     texture = OpenCvSharp.Unity.MatToTexture(_imgHand, texture);
        //     return;
        // }

        // 손가락 끝점을 그림
        _handDetector.DrawFingerPointAtImg(_imgHand);

        // 화면상의 손가락 끝 좌표를 가상세계 좌표로 변환
        _handManager.InputPoint(_handDetector.FingerPoint, _handDetector.Center);

        // 가상 손을 움직임
        _handManager.MoveHand(_handDetector.Radius);

        _handDetector.MainPoint.Clear();
        _handManager.Cvt3List.Clear();

        texture = OpenCvSharp.Unity.MatToTexture(_imgMask, texture);
        _display.texture = texture;
    }

    private void SendJpgInClientThread()
    {
        if(!Client.IsThreadRun && (!_handDetector.IsInitialized || _frame % 600 == 0))
        {
            _skinDetector.ImgOrigin = _imgFrame.Clone();

            Texture2D img = new Texture2D(_cam.width, _cam.height);
            img.SetPixels32(_cam.GetPixels32());

            byte[] jpg = img.EncodeToJPG();
            Debug.Log("메인 쓰레드 jpg 크기: " + jpg.Length);
            Client.Connect(jpg, _handDetector, _skinDetector);
        }
    }

    private void OnApplicationQuit()
    {
        if(Client.IsConnected)
            Client.Close();
    }
}
