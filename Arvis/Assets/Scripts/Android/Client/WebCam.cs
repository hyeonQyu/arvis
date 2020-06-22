using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using OpenCvSharp;
using System.IO;
using System.Net.Sockets;

public class WebCam : MonoBehaviour
{
    public Material[] _sortTest; // Remove this later(for the sorting test)
    private static WebCamTexture _cam;
    private RawImage _display;
    [SerializeField, Header("Virtual Camera")]
    private Camera _virtualCamera;
    [SerializeField, Header("Virtual World(Render texture)")]
    private RenderTexture _vWorld;
    [SerializeField, Header("Virtual Display")]
    private RawImage _vDisplay;

    [SerializeField]
    private GameObject[] _bounds;

    // 가상 손
    [SerializeField, Header("Hand")]
    private GameObject _hand;

    // Resize할 크기
    public static int Width = 16 * 15;
    public static int Height = 9 * 15;

    // 손 인식에 사용될 프레임 이미지
    private Mat _imgFrame;
    private Mat _imgMask;
    private Mat _imgHand;

    private SkinDetector _skinDetector;
    private HandDetector _handDetector;
    private HandManager _handManager;

    private IEnumerator _moveSmooth;
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

        _bounds[0].transform.localPosition = new Vector3(0, Screen.height / 2, 0);
        _bounds[1].transform.localPosition = new Vector3(0, -Screen.height / 2 + 10, 0);
        _bounds[2].transform.localPosition = new Vector3(-Screen.width / 2, 0, 0);
        _bounds[3].transform.localPosition = new Vector3(Screen.width / 2, 0, 0);

        // for Updating camera field of view
        _virtualCamera.enabled = false;
        _virtualCamera.enabled = true;

        // 원본 화면 = _cam
        _cam = new WebCamTexture(Screen.width, Screen.height, 60);

        _display.texture = _cam;
        _cam.Play();

        _skinDetector = new SkinDetector();
        _handDetector = new HandDetector();

        // no resize : _cam.width, _cam.height
        // resize : Width, Height
        _handManager = new HandManager(_hand, _display, Width, Height);
        
        Client.Setup();

        //// Remove this later(for the sorting test)
        //for(int i=1; i<_hand.transform.childCount; i++)
        //{
        //    _hand.transform.GetChild(i).GetComponent<MeshRenderer>().material = _sortTest[i];
        //}
    }

    private void Update()
    {
        _frame++;
        if (_frame < 120)
            return;

        _imgFrame = OpenCvSharp.Unity.TextureToMat(_cam);

        SendJpgInClientThread();

        Texture2D texture = new Texture2D(Width, Height);
        Cv2.Resize(_imgFrame, _imgFrame, new Size(Width, Height));

        _handDetector.IsCorrectDetection = true;

        if(_skinDetector.IsReceivedSkinColor)
        {
            _skinDetector.SetSkinColor();
            _skinDetector.IsReceivedSkinColor = false;
        }

        // 피부색으로 마스크 이미지를 검출
        _imgMask = _skinDetector.GetSkinMask(_imgFrame, _skinDetector.IsExtractedSkinColor);

        // 손의 점들을 얻음
        _imgHand = _handDetector.GetHandLineAndPoint(_imgFrame, _imgMask);

        //// 손 인식이 정확하지 않으면 프레임을 업데이트 하지 않음
        //if(!_handDetector.IsCorrectDetection)
        //{
        //    texture = OpenCvSharp.Unity.MatToTexture(_imgHand, texture);
        //    _display.texture = texture;
        //    return;
        //}

        // 손가락 끝점을 그림
        //_handDetector.DrawFingerPointAtImg(_imgHand);

        // 화면상의 손가락 끝 좌표를 가상세계 좌표로 변환
        _handManager.InputPoint(_handDetector.MainPoint, _handDetector.Center);
        
        // Stop MoveSmooth Coroutine
        //StopMoveSmooth();
        
        // 가상 손을 움직임
        _handManager.MoveHand((float)_handDetector.Radius);

        _handDetector.MainPoint.Clear();

        //texture = OpenCvSharp.Unity.MatToTexture(_imgHand, texture);
        //_display.texture = texture;

        // _imgHand.Release();
        // _imgMask.Release();
        // _imgFrame.Release();
    }

    private void SendJpgInClientThread()
    {
        if(!Client.IsThreadRun && (!_handDetector.IsInitialized || _frame % 1000 == 0))
        {
            _skinDetector.ImgOrigin = _imgFrame.Clone();

            Texture2D img = new Texture2D(_cam.width, _cam.height);
            img.SetPixels32(_cam.GetPixels32());

            byte[] jpg = img.EncodeToJPG();
            Debug.Log("메인 쓰레드 jpg 크기: " + jpg.Length);
            Client.Connect(jpg, _handDetector, _skinDetector);

            // DestroyImmediate(img);
        }
    }

    private void OnApplicationQuit()
    {
        if(Client.IsConnected)
            Client.Close();
    }

    public void StartMoveSmooth(IEnumerator coroutine)
    {
        //Debug.Log("Start MoveSmooth Coroutine");
        // Start Coroutine
        _moveSmooth = coroutine;
        StartCoroutine(_moveSmooth);
    }

    public void StopMoveSmooth()
    {
        if(_moveSmooth != null)
        {
            //Debug.Log("Stop MoveSmooth Coroutine");
            // Stop Coroutine
            StopCoroutine(_moveSmooth);
        }
    }
}
