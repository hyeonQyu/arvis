using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;

public class FaceDetector
{
    private string _faceClassifierFileName = Application.dataPath + "/Resources/haarcascade_frontalface_alt.xml";
    private CascadeClassifier _faceCascadeClassifer;

    // Start is called before the first frame update
    public FaceDetector()
    {
        _faceCascadeClassifer = new CascadeClassifier(_faceClassifierFileName);
    }

    public void RemoveFaces(Mat input, Mat output)
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
    /*
    public OpenCvSharp.Rect GetFaceRect(Mat input)
    {
        OpenCvSharp.Rect[] faceRectangles = new OpenCvSharp.Rect[1000];
        Mat inputGray = new Mat();

        Cv2.CvtColor(input, inputGray, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(inputGray, inputGray);

        faceCascadeClassifer.DetectMultiScale(inputGray, 1.1, 2, 0 | HaarDetectionType.ScaleImage, new Size(120, 120));

        if (faceRectangles.Length > 0)
            return faceRectangles[0];
        else
            return new OpenCvSharp.Rect(0, 0, 1, 1);
    }
    */
}