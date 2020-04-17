using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class WebCam : MonoBehaviour
{
    private WebCamTexture _cam;
    private RawImage _display;
    [SerializeField]
    private Canvas _canvas;

    // 손 인식에 사용될 프레임 이미지
    private Mat _imgFrame;
    private Mat _imgOrigin;
    private Mat _imgMask;
    private Mat _imgHand;

    SkinDetector _skinDetector;
    FaceDetector _faceDetector;
    HandDetector _handDetector;
    HandManager _handManager;

    //Color[] _colors;

    private int _frame = 0;

    private void Awake()
    {
        _skinDetector = new SkinDetector();
        _faceDetector = new FaceDetector();
        _handDetector = new HandDetector();
        //_handManager = new HandManager(_object, _handObject, this.Surface);

        _imgOrigin = new Mat();
    }

    private void Start()
    {
        _display = GetComponent<RawImage>();

        // 안드로이드 폰에서 실행시키기 위한 회전
        _canvas.transform.rotation = Quaternion.Euler(0, 0, 0);

        _cam = new WebCamTexture(Screen.width, Screen.height, 60);
        _display.texture = _cam;
        _cam.Play();

        //_colors = _cam.GetPixels();

        //Client.Setup();
    }
    
    private void Update()
    {
        HandTracking();
    }

    private Texture2D ResizeWebCamTexture()
    {
        Texture2D snap = new Texture2D(_cam.width, _cam.height);
        snap.SetPixels(_cam.GetPixels());
        TextureScale.Bilinear(snap, 424, 240);
        snap.Apply();
        return snap;
    }

    private void HandTracking()
    {
        _handDetector.IsCorrectDetection = true;

        // input 영상이 imgFrame
        _imgFrame = OpenCvSharp.Unity.TextureToMat(ResizeWebCamTexture());
        _imgOrigin = _imgFrame.Clone();

        // 얼굴 제거
        _faceDetector.RemoveFaces(_imgFrame, _imgFrame);

        // 피부색으로 마스크 이미지를 검출
        _imgMask = _skinDetector.GetSkinMask(_imgFrame);

        // 손의 점들을 얻음
        _imgHand = _handDetector.GetHandLineAndPoint(_imgFrame, _imgMask);

        // 손 인식이 정확하지 않으면 프레임을 업데이트 하지 않음
        if(!_handDetector.IsCorrectDetection)
        {
            //output = OpenCvSharp.Unity.MatToTexture(_imgOrigin, output);
            //return true;
            _display.texture = OpenCvSharp.Unity.MatToTexture(_imgFrame);
            return;
        }

        // 손가락 끝점을 그림
        _handDetector.DrawFingerPointAtImg(_imgHand);

        try
        {
            // 화면상의 손가락 끝 좌표를 가상세계 좌표로 변환
            _handManager.InputPoint(_handDetector.FingerPoint, _handDetector.Center);

            // 가상 손을 움직임
            _handManager.MoveHand();

            _handDetector.MainPoint.Clear();
            _handManager.Cvt3List.Clear();

            _display.texture = OpenCvSharp.Unity.MatToTexture(_imgHand);
        }
        catch(System.Exception e)
        {
            _display.texture = OpenCvSharp.Unity.MatToTexture(_imgFrame);
        }
        return;
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