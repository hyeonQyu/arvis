using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class HandTracker : MonoBehaviour
{
    // 손 인식에 사용될 프레임 이미지
    private Mat _imgFrame;
    private Mat _imgOrigin;
    private Mat _imgMask;
    private Mat _imgHand;

    //SkinDetector _skinDetector;
    //FaceDetector _faceDetector;
    //HandDetector _handDetector;
    //HandManager _handManager;

    private void Start()
    {
        Server.StartServer();
    }

    private void Update()
    {

    }

    public Texture2D Process(WebCamTexture input)
    {
        _imgFrame = OpenCvSharp.Unity.TextureToMat(input);
        return OpenCvSharp.Unity.MatToTexture(_imgFrame);
    }
}
