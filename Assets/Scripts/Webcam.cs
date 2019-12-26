using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public class Webcam:WebCamera
{
    protected override void Awake()
    {
        
    }

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        

        return true;
    }
}