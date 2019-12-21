using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public class Webcam2 : WebCamera
{
    private int range_count = 0;

    private Scalar _skin = new Scalar(95, 127, 166);
    private Scalar _table = new Scalar(176, 211, 238);

    private Mat _rgbColor;
    private Mat _rgbColor2;
    private Mat _hsvColor;

    private int _hue;
    private int _saturation;
    private int _value;

    private int _lowHue;
    private int _highHue;

    private int _lowHue1, _lowHue2, _highHue1, _highHue2;

    protected override void Awake()
    {
        _rgbColor = new Mat(1, 1, MatType.CV_8UC3, _skin);
        _rgbColor2 = new Mat(1, 1, MatType.CV_8UC3, _table);
        _hsvColor = new Mat();

        Cv2.CvtColor(_rgbColor, _hsvColor, ColorConversionCodes.BGR2HSV);

        _hue = (int) _hsvColor.At<Vec3b>(0, 0)[0];
        _saturation = (int) _hsvColor.At<Vec3b>(0, 0)[1];
        _value = (int) _hsvColor.At<Vec3b>(0, 0)[2];

        _lowHue = _hue - 7;
        _highHue = _hue + 7;

        if(_lowHue < 10)
        {
            range_count = 2;

            _highHue1 = 180;
            _lowHue1 = _lowHue + 180;
            _highHue2 = _highHue;
            _lowHue2 = 0;
        }
        else if(_highHue > 170)
        {
            range_count = 2;

            _highHue1 = _lowHue;
            _lowHue1 = 180;
            _highHue2 = _highHue - 180;
            _lowHue2 = 0;
        }
        else
        {
            range_count = 1;

            _lowHue1 = _lowHue;
            _highHue1 = _highHue;
        }

        base.Awake();
        this.forceFrontalCamera = true;
    }

    // Our sketch generation function
    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        // input 영상이 imgFrame
        Mat imgFrame = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);

        // 피부색 영역만 검출
        Mat imgMask = GetSkinMask(imgFrame);

        /* 윤곽선을 그리고 그 위에서 중앙을 찾으면 중지의 상단(손의 가장 윗부분)을 찾음
        // 윤곽선 그림
        Mat imgCanny = new Mat();
        Cv2.Canny(imgMask, imgCanny, 10.0, 70.0);  */

        double radius;
        // 손의 중심을 찾음
        Point center = GetHandCenter(imgMask, out radius);
        Debug.Log(center + " " + radius);
        // 손의 중심에 원을 그림
        Cv2.Circle(imgFrame, center, 5, new Scalar(0, 255, 0), 2);

        ///조명 괜찮은 곳에서 다시 확인
        //Point[][] contour;
        //HierarchyIndex[] hierarchy;
        //Cv2.FindContours(imgMask, out contour, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
        //Cv2.DrawContours(imgMask, contour, 0, Scalar.Yellow, 2, LineTypes.AntiAlias, hierarchy);
        //Point[][] hull = new Point[contour.Length][];
        //hull[0] = Cv2.ConvexHull(contour[0]);
        //Cv2.DrawContours(imgFrame, hull, 0, Scalar.Red, 2, LineTypes.AntiAlias);

        output = OpenCvSharp.Unity.MatToTexture(imgFrame, output);
        return true;

        ////////////////////////////////////////////////////////////////////////// 1

        ////Convert image to grayscale
        //Mat imgGray = new Mat();
        //Cv2.CvtColor(skin, imgGray, ColorConversionCodes.BGR2GRAY);

        //// Clean up image using Gaussian Blur
        //Mat imgGrayBlur = new Mat();
        //Cv2.GaussianBlur(imgGray, imgGrayBlur, new Size(5, 5), 0);

        ////Extract edges
        //Mat cannyEdges = new Mat();
        //Cv2.Canny(imgGrayBlur, cannyEdges, 10.0, 70.0);

        ////Do an invert binarize the image
        //Mat mask = new Mat();
        //Cv2.Threshold(cannyEdges, mask, 70.0, 255.0, ThresholdTypes.BinaryInv);

        // result, passing output texture as parameter allows to re-use it's buffer
        // should output texture be null a new texture will be created
    }

    private Mat GetSkinMask(Mat img, int minCr = 128, int maxCr = 170, int minCb = 73, int maxCb = 158)
    {
        // 블러 처리
        Mat imgBlur = new Mat();
        Cv2.GaussianBlur(img, imgBlur, new Size(5, 5), 0);

        // HSV로 변환
        Mat imgHsv = new Mat(imgBlur.Size(), MatType.CV_8UC3);
        Cv2.CvtColor(imgBlur, imgHsv, ColorConversionCodes.BGR2HSV);

        //지정한 HSV 범위를 이용하여 영상을 이진화
        Mat imgMask1, imgMask2;
        imgMask1 = new Mat();
        imgMask2 = new Mat();
        Cv2.InRange(imgHsv, new Scalar(_lowHue1, 50, 80), new Scalar(_highHue1, 255, 255), imgMask1);
        if(range_count == 2)
        {
            Cv2.InRange(imgHsv, new Scalar(_lowHue2, 50, 80), new Scalar(_highHue2, 255, 255), imgMask2);
            imgMask1 |= imgMask2;
        }


        //morphological opening 작은 점들을 제거
        Cv2.Erode(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
        Cv2.Dilate(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));


        //morphological closing 영역의 구멍 메우기
        Cv2.Dilate(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
        Cv2.Erode(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));

        return imgMask1;
    }

    private Point GetHandCenter(Mat img, out double radius)
    {
        // 거리 변환 행렬을 저장할 변수
        Mat dstMatrix = new Mat();
        Cv2.DistanceTransform(img, dstMatrix, DistanceTypes.L2, DistanceMaskSize.Mask5);

        // 거리 변환 행렬에서 값(거리)이 가장 큰 픽셀의 좌표와 값을 얻어옴
        int[] maxIdx = new int[2];
        double null1;
        int null2;
        Cv2.MinMaxIdx(dstMatrix, out null1, out radius, out null2, out maxIdx[0], img);

        return new Point(maxIdx[1], maxIdx[0]);
    }
}
