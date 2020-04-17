using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.UI;

public class FT_Webcam : MonoBehaviour
{
    // 손 인식에 사용될 프레임 이미지
    private Mat _imgInput;
    private Mat _imgFrame;
    private Mat _imgOrigin;
    private Mat _imgMask;
    private Mat _imgHand;

    [SerializeField]
    private RawImage _rawImage;

    // 영상을 input 값으로
    private FrameSource _frameSource;

    private Stopwatch _stopWatch;

    // 수행시간을 저장할 리스ㅌ
    private List<string> _taskTimeList;
    private List<int> _failFrameCount;

    SkinDetector _skinDetector;
    FaceDetector _faceDetector;
    HandDetector _handDetector;
    HandManager _handManager;

    private void Start()
    {
        _skinDetector = new SkinDetector();
        _faceDetector = new FaceDetector();
        _handDetector = new HandDetector();

        _stopWatch = new Stopwatch();

        _taskTimeList = new List<string>();
        _failFrameCount = new List<int>();

        _imgOrigin = new Mat();

        // 영상으로부터 이미지를 입력 받을 때
        //_frameSource = Cv2.CreateFrameSource_Video(Application.dataPath +"/Resources/test2.mp4");

        // 카메라로부터 이미지를 입력 받을 때
        _frameSource = Cv2.CreateFrameSource_Camera(0); 

    }

    private void Update()
    {
        HandDetect();
    }

    public void HandDetect()
    {
        _imgInput = new Mat();
        _imgFrame = new Mat();

        Texture2D texture2D;

        _frameSource.NextFrame(_imgInput);

        _handDetector.IsCorrectDetection = true;

        // 스탑워치 시작
        _stopWatch.Start();


                     //************************* _imgInput 해상도를 변경하여 _imgFrame 생성
        Size size = new Size(720, 480);
        Cv2.Resize(_imgInput, _imgFrame, size);

        // or

                    // 기존해상도 사용  **************************
        //_imgFrame = _imgInput;
                    // ***********************


        //UnityEngine.Debug.Log("Width = "+ _imgFrame.Width+ "Height = " + _imgFrame.Height);


        // input 영상이 imgFrame
        _imgOrigin = _imgFrame.Clone();

        // 얼굴 제거
        _faceDetector.RemoveFaces(_imgFrame, _imgFrame);

        // 피부색으로 마스크 이미지를 검출
        _imgMask = _skinDetector.GetSkinMask(_imgFrame);

        // 손의 점들을 얻음
        _imgHand = _handDetector.GetHandLineAndPoint(_imgFrame, _imgMask);

        // 손 인식이 정확하지 않으면 프레임을 업데이트 하지 않음
        if (!_handDetector.IsCorrectDetection)
        {
            _stopWatch.Stop();
            _failFrameCount.Add(0);
            texture2D = OpenCvSharp.Unity.MatToTexture(_imgFrame);

            _rawImage.GetComponent<RawImage>().texture = texture2D;
            //UnityEngine.Debug.Log("Failed Frame Counts = " + _failFrameCount.Count);
            _imgFrame.Release();
            _imgInput.Release();
            _imgMask.Release();
            _imgHand.Release();
            return;
        }

        // 손가락 끝점을 그림
        _handDetector.DrawFingerPointAtImg(_imgHand);

        // 스탑워치를 중지하고 소요시간을 리스트에 추가
        _stopWatch.Stop();
        _taskTimeList.Add(_stopWatch.ElapsedMilliseconds.ToString());
        _stopWatch.Reset();

        // 5의 배수로 리스트에 찰때 마다 디버그로 출력
        if (_taskTimeList.Count % 5 == 0)
        {
            string timeList = "";
            for (int i = 0; i < _taskTimeList.Count; i++)
            {
                timeList += _taskTimeList[i] + " ";
            }
            //UnityEngine.Debug.Log(timeList);
            //UnityEngine.Debug.Log("Failed Frame Counts = " + _failFrameCount.Count);
        }

        _handDetector.MainPoint.Clear();

        texture2D =  OpenCvSharp.Unity.MatToTexture(_imgFrame);

        _rawImage.GetComponent<RawImage>().texture = texture2D;

        //Cv2.ImShow("image", _imgFrame);
        _imgFrame.Release();
        _imgInput.Release();
        _imgMask.Release();
        _imgHand.Release();
        return;
    }

}