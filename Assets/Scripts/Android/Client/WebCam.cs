using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using OpenCvSharp;

public class WebCam : MonoBehaviour
{
    private static WebCamTexture _cam;
    private RawImage _display;
    [SerializeField]
    private Canvas _canvas;
    [SerializeField]
    private Text _text;

    // 서버로 전송할 데이터
    private byte[] _data;

    //private System.Diagnostics.Stopwatch _stopwatch;

    private int _width = 16 * 15;
    private int _height = 9 * 15;

    // 손 인식에 사용될 프레임 이미지
    private Mat _imgFrame;
    private Mat _imgOrigin;
    private Mat _imgMask;
    private Mat _imgHand;

    SkinDetector _skinDetector;
    HandDetector _handDetector;

    private void Start()
    {
        _display = GetComponent<RawImage>();

#if UNITY_EDITOR
        _canvas.transform.rotation = Quaternion.Euler(0, 0, 0);
#elif UNITY_ANDROID
        _canvas.transform.rotation = Quaternion.Euler(0, 0, 90);
#endif

        _cam = new WebCamTexture(Screen.width, Screen.height, 60);
        //_display.texture = _cam;
        _cam.Play();
        //     _text.text = _cam.height.ToString();

        //Client.Setup();

        //byte[] width = BitConverter.GetBytes(_cam.width);
        //byte[] height = BitConverter.GetBytes(_cam.height);
        //byte[] size = new byte[width.Length + height.Length];
        //Array.Copy(width, 0, size, 0, width.Length);
        //Array.Copy(height, 0, size, width.Length, height.Length);

        //// 서버에 이미지 사이즈를 알림(디코딩을 위함)
        //Client.Send(size);

        //_stopwatch = new System.Diagnostics.Stopwatch();

        _skinDetector = new SkinDetector();
        _handDetector = new HandDetector();
    }
    
    private void Update()
    {
        _imgFrame = OpenCvSharp.Unity.TextureToMat(_cam);
        //Cv2.Resize(_imgFrame, _imgFrame, new Size(240, 135));

        Texture2D texture = new Texture2D(_width, _height);



        _handDetector.IsCorrectDetection = true;

        Cv2.Resize(_imgFrame, _imgFrame, new Size(_width, _height));
        _imgOrigin = _imgFrame.Clone();

        // 피부색으로 마스크 이미지를 검출
        _imgMask = _skinDetector.GetSkinMask(_imgFrame);

        // 손의 점들을 얻음
        _imgHand = _handDetector.GetHandLineAndPoint(_imgFrame, _imgMask);

        //// 손 인식이 정확하지 않으면 프레임을 업데이트 하지 않음
        //if(!_handDetector.IsCorrectDetection)
        //{
        //    texture = OpenCvSharp.Unity.MatToTexture(_imgHand, texture);
        //    return;
        //}

        // 손가락 끝점을 그림
        _handDetector.DrawFingerPointAtImg(_imgHand);

        _handDetector.MainPoint.Clear();


        texture = OpenCvSharp.Unity.MatToTexture(_imgHand, texture);
        _display.texture = texture;
        Debug.Log("hingjae");
        //Texture2D texture = new Texture2D(_cam.width, _cam.height);
        //texture.SetPixels32(_cam.GetPixels32(), 0);
        //texture.Apply();
        //_data = Coder.Encode(_cam);
        //Debug.Log(_data.Length);
        //byte[] bytes = new byte[1024];
        //Array.Copy(_data, 0, bytes, 0, 1024);

        //Client.Send(bytes);


        //// input 영상이 imgFrame
        //_imgFrame = OpenCvSharp.Unity.TextureToMat(_cam);
        //byte[] bytes = new byte[1024];

        //var pt = _imgFrame.At<Vec3b>(0, 0);
        ////pt.Item0 = (byte)(255 - pt.Item0);
        ////pt.Item1 = (byte)(255 - pt.Item1);
        ////pt.Item2 = (byte)(255 - pt.Item2);
        //Debug.Log(_cam.width + " " + _cam.height);
        //Debug.Log("0 " + pt.Item0 + " " + pt.Item1 + " " + pt.Item2);
        //Color32 color = _cam.GetPixel(0, _height - 1);
        //Debug.Log("1 " + color.b + " " + color.g + " " + color.r);
        //Color32[] colors = _cam.GetPixels32();
        //Debug.Log("2 " + colors[517440].b + " " + colors[517440].g + " " + colors[517440].r);
        
        ////Color32 color2 = _cam.GetPixel(959, _height - 1);
        ////Debug.Log("2 " + color2.b + " " + color2.g + " " + color2.r);
        ////Color32 color3 = _cam.GetPixel(959, _height - 1);
        ////Debug.Log("3 " + color3.b + " " + color3.g + " " + color3.r);

        ////Client.Send(_imgFrame.ToBytes());

        ////_display.texture = OpenCvSharp.Unity.MatToTexture(_imgFrame);
        ////_imgFrame.Release();
    }

    //private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    //{
    //    Texture2D result = new Texture2D(_cam.width, _cam.height, TextureFormat.RGB24, true);
    //    Color[] rpixels = result.GetPixels(0);
    //    float incX = (1.0f / (float)targetWidth);
    //    float incY = (1.0f / (float)targetHeight);
    //    for(int px = 0; px < rpixels.Length; px++)
    //    {
    //        rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
    //    }
    //    result.SetPixels(rpixels, 0);
    //    result.Apply();
    //    return result;
    //}

    //private Texture2D ResizeTexture()
    //{
    //    Texture2D snap = new Texture2D(_cam.width, _cam.height, TextureFormat.RGB24, false);
    //    snap.SetPixels(_cam.GetPixels());
    //    TextureScale.Bilinear(snap, 424, 240);

    //    snap.Apply();
    //    return snap;
    //}

    //private byte[] TextureToBytes(Texture2D texture)
    //{
    //    return texture.EncodeToJPG();
    //}

    //private void OnApplicationQuit()
    //{
    //    Client.Close();
    //}
}