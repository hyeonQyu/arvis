using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public class Webcam2 : WebCamera
{
    private int _rangeCount = 0;

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

    private Mat[] _imgFrames;

    private int _frameCount;
    private int _frameIndex;

    // 녹화할 프레임의 수
    private const int _recordFrameCount = 50;

    // 손 인식을 위한 학습된 모델
    //private static CascadeClassifier _cascade;

    // 얼굴 인식을 위한 학습된 모델
    private CascadeClassifier _faceCascadeClassifer;

    protected override void Awake()
    {
        InitializeHsv();

        _faceCascadeClassifer = new CascadeClassifier(Application.dataPath + "/Resources/haarcascade_frontalface_alt.xml");

        //// 녹화를 위한 초기화
        //_imgFrames = new Mat[_recordFrameCount + 1];
        //_frameCount = 0;
        //_frameIndex = 0;

        base.Awake();
        this.forceFrontalCamera = true;
    }

    // Our sketch generation function
    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        //// 일정 프레임까지 녹화한 후 재생
        //if(_frameCount > _recordFrameCount)
        //{
        //    return PlayRecordedFrame(ref output);
        //}

        // input 영상이 imgFrame
        Mat imgFrame = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);
        Mat imgMask = new Mat();

        // 얼굴 제거
        RemoveFaces(imgFrame, imgFrame);

        // 피부색 영역만 검출
        imgMask = GetSkinMask(imgFrame);

        // 손의 점을 얻음
        Mat imgHand = GetHandLineAndPoint(imgFrame, imgMask);

        ////Convert image to grayscale
        //Mat imgGray = new Mat();
        //Cv2.CvtColor(imgFrame, imgGray, ColorConversionCodes.BGR2GRAY);

        //// Clean up image using Gaussian Blur
        //Mat imgGrayBlur = new Mat();
        //Cv2.GaussianBlur(imgGray, imgGrayBlur, new Size(5, 5), 0);

        ////Extract edges
        //Mat cannyEdges = new Mat();
        //Cv2.Canny(imgGrayBlur, cannyEdges, 10.0, 70.0);

        ////Do an invert binarize the image
        //Mat mask = new Mat();
        //Cv2.Threshold(cannyEdges, mask, 70.0, 255.0, ThresholdTypes.BinaryInv);

        double radius;
        // 손의 중심을 찾음
        Point center = GetHandCenter(imgMask, out radius);
        Debug.Log(center + " " + radius);
        // 손의 중심에 원을 그림
        Cv2.Circle(imgHand, center, (int)(radius * 1.3), new Scalar(0, 255, 0), 2);

        // 영상 녹화
        //_imgFrames[_frameCount++] = imgMask;
        //Debug.Log(_frameCount);

        // 결과 출력
        output = OpenCvSharp.Unity.MatToTexture(imgHand, output);

        //_backgroundRemover.Calibrate(imgFrame);
        return true;
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
        if(_rangeCount == 2)
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

    private void RemoveFaces(Mat input, Mat output)
    {
        OpenCvSharp.Rect[] faces;
        Mat frameGray = new Mat();

        Cv2.CvtColor(input, frameGray, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(frameGray, frameGray);

        faces = _faceCascadeClassifer.DetectMultiScale(frameGray, 1.1, 2, HaarDetectionType.ScaleImage, new Size(120, 120));

        for(int i = 0; i < faces.Length; i++)
        {
            Cv2.Rectangle(output, new Point(faces[i].X, faces[i].Y), new Point(faces[i].X + faces[i].Width, faces[i].Y + faces[i].Height), new Scalar(0, 0, 0), -1);
        }
    }

    //private Mat DetectHand(Mat img)
    //{
    //    Mat imgCopy = new Mat();
    //    img.CopyTo(imgCopy);

    //    Mat imgGray = new Mat();
    //    Mat imgEdge = new Mat();

    //    Cv2.CvtColor(imgCopy, imgGray, ColorConversionCodes.BGR2GRAY);

    //    //Cv2.GaussianBlur(imgGray, imgGray, new Size(5, 5), 0);

    //    Cv2.EqualizeHist(imgGray, imgGray);
    //    OpenCvSharp.Rect[] hands = _cascade.DetectMultiScale(imgCopy, 1.1, 4, HaarDetectionType.ScaleImage, new Size(30, 30));
    //    for(int i = 0; i < hands.Length; i++)
    //    {
    //        Cv2.Rectangle(imgCopy, hands[i], new Scalar(255, 0, 0), 2);
    //    }

    //    return imgGray;
    //}

    private Mat GetHandLineAndPoint(Mat img, Mat imgMask)
    {
        // 원본영상 & 마스크이미지 -> 피부색 영역 검출
        Mat imgSkin = new Mat();
        Cv2.BitwiseAnd(img, img, imgSkin, imgMask);

        // 피부색 추출 -> GrayScale
        Mat imgGray = new Mat();
        Cv2.CvtColor(imgSkin, imgGray, ColorConversionCodes.BGR2GRAY);
        Cv2.BitwiseNot(imgGray, imgGray);  // 색상반전

        // GrayScale -> Canny
        Mat imgCanny = new Mat();
        Cv2.Canny(imgGray, imgCanny, 100, 200);
        //bitwise_not(img_canny, img_canny);  // 색상반전


        // 특정 윤곽선 검출하기
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(imgCanny, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
        Point[][] hull = new Point[contours.Length][];
        int[][] convexHullIdx = new int[contours.Length][];
        Vec4i[][] defects = new Vec4i[contours.Length][];

        Mat imgHand = Mat.Zeros(imgGray.Size(), MatType.CV_8UC3);
        Cv2.BitwiseNot(imgHand, imgHand);

        // Version_1
        int largestArea = 0;
        int largestContourIndex = 0;
        double a;

        for(int i = 0; i < contours.Length; i++) // iterate through each contour.
        {
            hull[i] = Cv2.ConvexHull(contours[i]);
            convexHullIdx[i] = Cv2.ConvexHullIndices(contours[i]);
            defects[i] = Cv2.ConvexityDefects(contours[i], convexHullIdx[i]);
            a = Cv2.ContourArea(contours[i], false);  //  Find the area of contour
            if(a > largestArea)
            {
                largestArea = (int)a;
                largestContourIndex = i;                //Store the index of largest contour
            }

        }

        Debug.Log("largeIndex : " + largestContourIndex);

        if(largestArea > 1)
        {
            Cv2.DrawContours(imgHand, contours, largestContourIndex, new Scalar(0, 255, 0));
            Cv2.DrawContours(imgHand, hull, largestContourIndex, new Scalar(0, 0, 255));
            Debug.Log(defects[largestContourIndex].Length);
            // Draw defect
            for(int i = 0; i < defects[largestContourIndex].Length; i++)
            {
                Point start, end, far;
                int d = defects[largestContourIndex][i].Item3;
                /*// 가까이 있는 점들이 배열에 Linear하게 들어있을줄 알고 짰던 코드 주석 해놨
                if (d < 200)
                {
                    start = contours[largestContourIndex][defects[largestContourIndex][i].Item0];
                    int index = 1;
                    while( (i+index) < defects[largestContourIndex].Length && defects[largestContourIndex][i + index].Item3 < 200)
                    {
                        d += defects[largestContourIndex][i + index].Item3;
                        index++;
                    }

                    if (index == 1)
                        continue;
                    else
                    {
                        end = contours[largestContourIndex][defects[largestContourIndex][i+index-1].Item1];
                        far = contours[largestContourIndex][defects[largestContourIndex][(i+index)/2].Item2];
                        i += index - 1;
                    }

                }*/
                //else
                //{
                start = contours[largestContourIndex][defects[largestContourIndex][i].Item0];
                end = contours[largestContourIndex][defects[largestContourIndex][i].Item1];
                far = contours[largestContourIndex][defects[largestContourIndex][i].Item2];
                string log = i + "   " + far;
                //Debug.Log(log);
                //}
                if(d > 1)
                {
                    Scalar scalar = Scalar.RandomColor();
                    //Cv2.Line(imgHand, start, far, scalar, 2, LineTypes.AntiAlias);
                    //Cv2.Line(imgHand, end, far, scalar, 2, LineTypes.AntiAlias);
                    Cv2.Circle(imgHand, far, 5, scalar, -1, LineTypes.AntiAlias);
                }
            }
        }
        Debug.Log("---------------");

        return imgHand;
    }

    private bool PlayRecordedFrame(ref Texture2D output)
    {
        if(_frameIndex == _recordFrameCount)
            _frameIndex = 0;

        // 녹화한 영상 재생
        output = OpenCvSharp.Unity.MatToTexture(_imgFrames[_frameIndex++], output);
        return true;
    }

    private void InitializeHsv()
    {
        _rgbColor = new Mat(1, 1, MatType.CV_8UC3, _skin);
        _rgbColor2 = new Mat(1, 1, MatType.CV_8UC3, _table);
        _hsvColor = new Mat();

        Cv2.CvtColor(_rgbColor, _hsvColor, ColorConversionCodes.BGR2HSV);

        _hue = (int)_hsvColor.At<Vec3b>(0, 0)[0];
        _saturation = (int)_hsvColor.At<Vec3b>(0, 0)[1];
        _value = (int)_hsvColor.At<Vec3b>(0, 0)[2];

        _lowHue = _hue - 7;
        _highHue = _hue + 7;

        if(_lowHue < 10)
        {
            _rangeCount = 2;

            _highHue1 = 180;
            _lowHue1 = _lowHue + 180;
            _highHue2 = _highHue;
            _lowHue2 = 0;
        }
        else if(_highHue > 170)
        {
            _rangeCount = 2;

            _highHue1 = _lowHue;
            _lowHue1 = 180;
            _highHue2 = _highHue - 180;
            _lowHue2 = 0;
        }
        else
        {
            _rangeCount = 1;

            _lowHue1 = _lowHue;
            _highHue1 = _highHue;
        }
    }
}
