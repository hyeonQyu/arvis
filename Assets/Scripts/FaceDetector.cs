using UnityEngine;
using OpenCvSharp;

public class FaceDetector
{
    // 얼굴 인식을 위한 학습된 모델
    private CascadeClassifier _faceCascadeClassifer;

    public FaceDetector()
    {
        _faceCascadeClassifer = new CascadeClassifier(Application.dataPath + "/Resources/haarcascade_frontalface_alt.xml");
    }

    // 프레임 이미지에서 얼굴을 제거
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
}
