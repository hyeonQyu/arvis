using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public class Webcam3 : WebCamera
{
    private Mat _frame, _frameOut, _imgHandMask, _imgForeground, _fingerCountDebug;

    private BackgroundRemover _backgroundRemover;
    private SkinDetector _skinDetector;

    private Mat[] _imgFrames;
    PlayFrame _playFrame;

    private int _frameCount;
    private int _frameIndex;

    protected override void Awake()
    {
        _backgroundRemover = new BackgroundRemover();
        _skinDetector = new SkinDetector();

        _frameCount = 0;
        _frameIndex = 0;

        _imgFrames = new Mat[150];
        _playFrame = new PlayFrame();

        base.Awake();
        this.forceFrontalCamera = true;
    }

    // Our sketch generation function
    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        _frameCount++;
        Debug.Log(_frameCount);
        if(_frameCount > 10)
        {
            if(_frameIndex == 10)
                _frameIndex = 0;

            // 녹화한 영상 재생
            output = OpenCvSharp.Unity.MatToTexture(_imgFrames[_frameIndex++]);
            return true;
        }

        // input 영상이 imgFrame
        Mat imgFrame = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);

        //_skinDetector.DrawSkinColorSampler(imgFrame);
        _imgForeground = _backgroundRemover.GetForeground(imgFrame);
        //_imgHandMask = _skinDetector.GetSkinMask(imgFrame);

        // 영상 녹화
        _imgFrames[_frameCount] = _imgForeground;

        //_skinDetector.Calibrate(imgFrame);
        _backgroundRemover.Calibrate(imgFrame);
        return false;
    }
}
