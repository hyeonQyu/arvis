using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public class BackgroundRemover
{
    private Mat _background;
    private bool isCalibrated = false;

    public BackgroundRemover()
    {
        _background = new Mat();
    }

    public void Calibrate(Mat input)
    {
        Cv2.CvtColor(input, _background, ColorConversionCodes.BGR2GRAY);
        isCalibrated = true;
    }

    public Mat GetForeground(Mat input)
    {
        Mat foregroundMask = GetForegroundMask(input);
        Mat foreground = new Mat();
        input.CopyTo(foreground, foregroundMask);

        return foreground;
    }

    private Mat GetForegroundMask(Mat input)
    {
        Mat foregroundMask = new Mat();

        if(!isCalibrated)
        {
            foregroundMask = Mat.Zeros(input.Size(), MatType.CV_8UC1);
            return foregroundMask;
        }

        Cv2.CvtColor(input, foregroundMask, ColorConversionCodes.BGR2GRAY);
        RemoveBackground(foregroundMask, _background);

        return foregroundMask;
    }

    private void RemoveBackground(Mat input, Mat background)
    {
        int thresholdOffset = 10;

        for(int i = 0; i < input.Rows; i++)
        {
            for(int j = 0; j < input.Cols; j++)
            {
                byte framePixel = input.At<byte>(i, j);
                byte bgPixel = _background.At<byte>(i, j);

                if((framePixel >= bgPixel - thresholdOffset) && (framePixel <= bgPixel + thresholdOffset))
                    input.Set<byte>(i, j, 0);
                else
                    input.Set<byte>(i, j, 255);
            }
        }
    }
}