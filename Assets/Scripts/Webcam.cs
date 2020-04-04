using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class Webcam:WebCamera
{
    // 손 인식에 사용될 프레임 이미지
    private Mat _imgFrame;
    private Mat _imgOrigin;
    private Mat _imgMask;
    private Mat _imgHand;

    SkinDetector _skinDetector;
    FaceDetector _faceDetector;
    HandDetector _handDetector;
    HandManager _handManager;

    // 움직일(터치할) 오브젝트
    [SerializeField, Header("Object to Move")]
    private GameObject _object;
    // 가상 손의 손가락
    [SerializeField, Header("Finger & Center")]
    private GameObject[] _handObject;

    protected override void Awake()
    {
        _skinDetector = new SkinDetector();
        _faceDetector = new FaceDetector();
        _handDetector = new HandDetector();
        _handManager = new HandManager(_object, _handObject, this.Surface);

        _imgOrigin = new Mat();

        base.Awake();
        this.forceFrontalCamera = true;
    }

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        _handDetector.IsCorrectDetection = true;

        // input 영상이 imgFrame
        _imgFrame = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);
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
            output = OpenCvSharp.Unity.MatToTexture(_imgOrigin, output);
            return true;
        }

        // 손가락 끝점을 그림
        _handDetector.DrawFingerPointAtImg(_imgHand);

        // 화면상의 손가락 끝 좌표를 가상세계 좌표로 변환
        _handManager.InputPoint(_handDetector.FingerPoint, _handDetector.Center);

        // 가상 손을 움직임
        _handManager.MoveHand();

        _handDetector.MainPoint.Clear();
        _handManager.Cvt3List.Clear();

        output = OpenCvSharp.Unity.MatToTexture(_imgHand, output);
        return true;
    }
}