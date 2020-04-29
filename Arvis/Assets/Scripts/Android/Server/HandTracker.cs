using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using OpenCvSharp.Demo;

public class HandTracker : MonoBehaviour
{
    //private SkinDetector _skinDetector;
    //private HandDetector _handDetector;

    //public static int Width;
    //public static int Height;

    [SerializeField]
    private RawImage _display;
    [SerializeField]
    public Text[] _text;
    [SerializeField]
    public Text _queueNull;
    public Text _running;

    private void Start()
    {
        //_skinDetector = new SkinDetector();
        //_handDetector = new HandDetector();
        Debug.Log("Start");
        Server.Open();
    }

    private void Update()
    {
        _running.text = "Run";
        if(Server.ImgQueue.Count == 0)
        {
            //_queueNull.text = "0";
            return;
        }
        _queueNull.text = "1";

        byte[] data = Server.ImgQueue.Dequeue();
        Texture2D texture = new Texture2D(_display.texture.width, _display.texture.height);
        Debug.Log("JPG");
        for(int i = 0; i < 10; i++)
        {
            _text[i].text = data[i].ToString();
        }
        texture.LoadImage(data);

        _display.texture = texture;
        //_text.text = data.Length.ToString();
        //Mat imgFrame = Coder.Decode(data, Width, Height);

        //Texture2D texture = OpenCvSharp.Unity.MatToTexture(imgFrame);
        //_display.texture = texture;

        //Process(imgFrame);
    }

    //public void Process(Mat imgFrame)
    //{
    //    _handDetector.IsCorrectDetection = true;

    //    Mat imgOrigin = imgFrame.Clone();

    //    // 피부색으로 마스크 이미지를 검출
    //    Mat imgMask = _skinDetector.GetSkinMask(imgFrame);

    //    // 손의 점들을 얻음
    //    Mat imgHand = _handDetector.GetHandLineAndPoint(imgFrame, imgMask);

    //    // 손 인식이 정확하지 않으면 안드로이드 기기에 결과를 전송하지 않음
    //    if(!_handDetector.IsCorrectDetection)
    //    {
    //        return;
    //    }

    //    // 손가락 끝점을 그림
    //    _handDetector.DrawFingerPointAtImg(imgHand);
    //}

    private void OnApplicationQuit()
    {
        Server.Close();
    }
}
