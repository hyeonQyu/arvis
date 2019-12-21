using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public class Webcam3 : WebCamera
{
    private Mat _frame, _frameOut, _handMask, _foreground, _fingerCountDebug;

    protected override void Awake()
    {
        base.Awake();
        this.forceFrontalCamera = true;
    }

    // Our sketch generation function
    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        // input 영상이 imgFrame
        Mat imgFrame = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);
        
        output = OpenCvSharp.Unity.MatToTexture(imgFrame, output);
        return true;
    }
}
