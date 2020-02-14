using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public class Task
{
    private Mat _imgFrame;
    public Mat ImgFrame
    {
        get
        {
            return _imgFrame;
        }
    }
    private Mat _imgOrigin;
    public Mat ImgOrigin
    {
        get
        {
            return _imgOrigin;
        }
    }
    private Mat _imgOther;
    public Mat ImgOther
    {
        get
        {
            return _imgOther;
        }
    }
    private bool _isSuccessDetect;
    public bool IsSuccessDetect
    {
        get
        {
            return _isSuccessDetect;
        }
    }
    private HandDetector _handDetector;
    public HandDetector HandDetector
    {
        set
        {
            _handDetector.Center = value.Center;
            _handDetector.MainPoint = value.MainPoint;
            _handDetector.IsCorrectDetection = value.IsCorrectDetection;
        }
        get
        {
            return _handDetector;
        }
    }

    public Task(Mat imgFrame, Mat imgOrigin, Mat imgOther = null)
    {
        _imgFrame = imgFrame;
        _imgOrigin = imgOrigin;
        _imgOther = imgOther;
        _handDetector = new HandDetector();
    }
}
