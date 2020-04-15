using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine.Video;

public class FT_Webcam : MonoBehaviour
{
    // 손 인식에 사용될 프레임 이미지
    private Mat _imgFrame;
    private Mat _imgOrigin;
    private Mat _imgMask;
    private Mat _imgHand;

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

        _frameSource = Cv2.CreateFrameSource_Video("Assets/Resources/test2.mp4");

    }

    private void Update()
    {
        HandDetect();
    }

    public void HandDetect()
    {

        _imgFrame = new Mat();
        _frameSource.NextFrame(_imgFrame);
        _handDetector.IsCorrectDetection = true;

        _stopWatch.Start();
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
            return;
        }

        // 손가락 끝점을 그림
        _handDetector.DrawFingerPointAtImg(_imgHand);

        // 스탑워치를 중지하고 소요시간을 리스트에 추가
        _stopWatch.Stop();
        _taskTimeList.Add(_stopWatch.ElapsedMilliseconds.ToString());

        // 5의 배수로 리스트에 찰때 마다 디버그로 출력
        if (_taskTimeList.Count % 5 == 0)
        {
            string timeList = "";
            for (int i = 0; i < _taskTimeList.Count; i++)
            {
                timeList += _taskTimeList[i] + " ";
            }
            UnityEngine.Debug.Log(timeList);
            UnityEngine.Debug.Log("Failed Frame Counts = " + _failFrameCount.Count);
        }

        _handDetector.MainPoint.Clear();

        return;
    }
}