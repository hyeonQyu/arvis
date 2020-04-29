using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using OpenCvSharp;

public class WebCam : MonoBehaviour
{
    private static WebCamTexture _cam;
    private RawImage _display;
    [SerializeField]
    private Canvas _canvas;

    // 움직일(터치할) 오브젝트
    [SerializeField, Header("Object to Move")]
    private GameObject _object;
    // 가상 손의 손가락
    [SerializeField, Header("Finger & Center")]
    private GameObject[] _handObject;

    // 서버로 전송할 데이터
    private byte[] _data;

    // Resize할 크기
    private const int _width = 16 * 15;
    private const int _height = 9 * 15;

    // 손 인식에 사용될 프레임 이미지
    private Mat _imgFrame;
    private Mat _imgMask;
    private Mat _imgHand;

    SkinDetector _skinDetector;
    HandDetector _handDetector;
    HandManager _handManager;

    private void Start()
    {
        Debug.Log(Math.Atan2(23, 10) * 180 / Math.PI);
        Debug.Log(Math.Atan2(23, -10) * 180 / Math.PI);
        Debug.Log(Math.Atan2(-23, -10) * 180 / Math.PI);
        Debug.Log(Math.Atan2(-23, 10) * 180 / Math.PI);
        _display = GetComponent<RawImage>();

#if UNITY_EDITOR
        _canvas.transform.rotation = Quaternion.Euler(0, 0, 0);
#elif UNITY_ANDROID
        _canvas.transform.rotation = Quaternion.Euler(0, 0, 270);
#endif
        // 원본 화면 = _cam
        _cam = new WebCamTexture(Screen.width, Screen.height, 60);
        //_display.texture = _cam;
        _cam.Play();

        _skinDetector = new SkinDetector();
        _handDetector = new HandDetector();
        _handManager = new HandManager(_object, _handObject, _canvas);

        //Client.Setup();
    }

    private void Update()
    {
        _imgFrame = OpenCvSharp.Unity.TextureToMat(_cam);

        Texture2D texture = new Texture2D(_width, _height);
        Cv2.Resize(_imgFrame, _imgFrame, new Size(_width, _height));

        _handDetector.IsCorrectDetection = true;

        // 피부색으로 마스크 이미지를 검출
        _imgMask = _skinDetector.GetSkinMask(_imgFrame);

        // 손의 점들을 얻음
        _imgHand = _handDetector.GetHandLineAndPoint(_imgFrame, _imgMask);

        //// 손 인식이 정확하지 않으면 프레임을 업데이트 하지 않음
        //if(!_handDetector.IsCorrectDetection)
        //{
        //    texture = OpenCvSharp.Unity.MatToTexture(_imgHand, texture);
        //    return;
        //}

        // 손가락 끝점을 그림
        _handDetector.DrawFingerPointAtImg(_imgHand);

        // 화면상의 손가락 끝 좌표를 가상세계 좌표로 변환
        _handManager.InputPoint(_handDetector.FingerPoint, _handDetector.Center);

        // 가상 손을 움직임
        _handManager.MoveHand();

        _handDetector.MainPoint.Clear();
        _handManager.Cvt3List.Clear();

        texture = OpenCvSharp.Unity.MatToTexture(_imgHand, texture);
        _display.texture = texture;
    }

    //private void OnApplicationQuit()
    //{
    //    Client.Close();
    //}
}