using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;

public static class HandTracker
{
    // 손 인식에 사용될 프레임 이미지
    private static Mat _imgFrame;
    private static Mat _imgOrigin;
    private static Mat _imgMask;
    private static Mat _imgHand;

    //SkinDetector _skinDetector;
    //FaceDetector _faceDetector;
    //HandDetector _handDetector;
    //HandManager _handManager;

    public static Texture2D Process(WebCamTexture input)
    {
        _imgFrame = OpenCvSharp.Unity.TextureToMat(input);
        return OpenCvSharp.Unity.MatToTexture(_imgFrame);
    }
}
