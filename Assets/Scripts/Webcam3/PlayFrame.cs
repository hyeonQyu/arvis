using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public class PlayFrame
{
    int i;
    public Mat[] ImgFrames;

    public PlayFrame()
    {
        ImgFrames = new Mat[150];
    }
}
