using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;

public class SkinDetector
{
    private int _hLowThreshold;
    private int _hHighThreshold;
    private int _sLowThreshold;
    private int _sHighThreshold;
    private int _vLowThreshold;
    private int _vHighThreshold;

    bool _isCalibrated;

    OpenCvSharp.Rect _skinColorSampleRectangle1;
    OpenCvSharp.Rect _skinColorSampleRectangle2;

    public SkinDetector()
    {
        _hLowThreshold = 0;
        _hHighThreshold = 0;
        _sLowThreshold = 0;
        _sHighThreshold = 0;
        _vLowThreshold = 0;
        _vHighThreshold = 0;

        _isCalibrated = false;
    }

    public void DrawSkinColorSampler(Mat input)
    {
        int frameWidth = input.Size().Width;
        int frameHeight = input.Size().Height;

        int rectangleSize = 20;
        Scalar rectangleColor = new Scalar(255, 0, 255);

        _skinColorSampleRectangle1 = new OpenCvSharp.Rect(frameWidth / 5, frameHeight / 2, rectangleSize, rectangleSize);
        _skinColorSampleRectangle2 = new OpenCvSharp.Rect(frameWidth / 5, frameHeight / 3, rectangleSize, rectangleSize);

        Cv2.Rectangle(input, _skinColorSampleRectangle1, rectangleColor);
        Cv2.Rectangle(input, _skinColorSampleRectangle2, rectangleColor);
    }

    public void Calibrate(Mat input)
    {
        Mat hsvInput = new Mat();
        Cv2.CvtColor(input, hsvInput, ColorConversionCodes.BGR2HSV);

        Mat sample1 = new Mat(hsvInput, _skinColorSampleRectangle1);
        Mat sample2 = new Mat(hsvInput, _skinColorSampleRectangle2);

        CalculateThresholds(sample1, sample2);

        _isCalibrated = true;
    }

    public Mat GetSkinMask(Mat input)
    {
        Mat skinMask = new Mat();

        if(!_isCalibrated)
        {
            skinMask = Mat.Zeros(input.Size(), MatType.CV_8UC1);
            return skinMask;
        }

        Mat hsvInput = new Mat();
        Cv2.CvtColor(input, hsvInput, ColorConversionCodes.BGR2HSV);

        Cv2.InRange(hsvInput, new Scalar(_hLowThreshold, _sLowThreshold, _vLowThreshold),
                                new Scalar(_hHighThreshold, _sHighThreshold, _vHighThreshold), skinMask);

        Size structuralElementSize = new Size(3, 3);
        PerformOpening(skinMask, MorphShapes.Ellipse, structuralElementSize);
        Cv2.Dilate(skinMask, skinMask, new Mat(), new Point(-1, -1), 3);

        return skinMask;
    }

    private void CalculateThresholds(Mat sample1, Mat sample2)
    {
        int offsetLowThreshold = 80;
        int offsetHighThreshold = 30;

        Scalar hsvMeansSample1 = Cv2.Mean(sample1);
        Scalar hsvMeansSample2 = Cv2.Mean(sample2);

        _hLowThreshold = (int)Math.Min(hsvMeansSample1[0], hsvMeansSample2[0]) - offsetLowThreshold;
        _hHighThreshold = (int)Math.Max(hsvMeansSample1[0], hsvMeansSample2[0]) + offsetHighThreshold;

        _sLowThreshold = (int)Math.Min(hsvMeansSample1[1], hsvMeansSample2[1]) - offsetLowThreshold;
        _sHighThreshold = (int)Math.Max(hsvMeansSample1[1], hsvMeansSample2[1]) + offsetHighThreshold;

        //_vLowThreshold = (int)Math.Min(hsvMeansSample1[2], hsvMeansSample2[2]) - offsetLowThreshold;
        //_vHighThreshold = (int)Math.Max(hsvMeansSample1[2], hsvMeansSample2[2]) + offsetHighThreshold;
        _vLowThreshold = 0;
        _vHighThreshold = 255;
    }

    private void PerformOpening(Mat binaryImage, MorphShapes kernelShape, Size kernelSize)
    {
        Mat structuringElement = Cv2.GetStructuringElement(kernelShape, kernelSize);
        Cv2.MorphologyEx(binaryImage, binaryImage, MorphTypes.Open, structuringElement);
    }
}
