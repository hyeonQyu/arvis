using OpenCvSharp;
using UnityEngine.XR;

public class HandBoundary
{
    public int Left { private set; get; }
    public int Right { private set; get; }
    public int Top { private set; get; }
    public int Bottom { private set; get; }

    public void SetBoundary(int[] datas)
    {
        Left = datas[0];
        Right = datas[1];
        Top = datas[2];
        Bottom = datas[3];
    }
}

public class SkinDetector
{
    private int _rangeCount;

    private Scalar _skin;
    private Scalar _table;

    private Mat _rgbColor;
    private Mat _rgbColor2;
    private Mat _hsvColor;

    private int _hue;
    private int _saturation;
    private int _value;

    private int _lowHue;
    private int _highHue;

    private int _lowHue1;
    private int _lowHue2;
    private int _highHue1;
    private int _highHue2;

    public HandBoundary HandBoundary { get; set; }

    public SkinDetector()
    {
        _skin = new Scalar(95, 127, 166);
        _table = new Scalar(176, 211, 238);

        HandBoundary = new HandBoundary();

        InitializeHsv();
    }

    // HSV를 위한 기본 값 세팅
    private void InitializeHsv()
    {
        _rgbColor = new Mat(1, 1, MatType.CV_8UC3, _skin);
        _rgbColor2 = new Mat(1, 1, MatType.CV_8UC3, _table);
        _hsvColor = new Mat();

        Cv2.CvtColor(_rgbColor, _hsvColor, ColorConversionCodes.BGR2HSV);

        _hue = (int)_hsvColor.At<Vec3b>(0, 0)[0];
        _saturation = (int)_hsvColor.At<Vec3b>(0, 0)[1];
        _value = (int)_hsvColor.At<Vec3b>(0, 0)[2];

        _lowHue = _hue - 10;
        _highHue = _hue + 10;

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

    // 피부색을 검출하여 마스크 이미지를 만듦
    public Mat GetSkinMask(Mat img, int minCr = 128, int maxCr = 170, int minCb = 73, int maxCb = 158)
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
        Cv2.InRange(imgHsv, new Scalar(_lowHue1, 50, 50), new Scalar(_highHue1, 255, 255), imgMask1);
        if(_rangeCount == 2)
        {
            Cv2.InRange(imgHsv, new Scalar(_lowHue2, 50, 50), new Scalar(_highHue2, 255, 255), imgMask2);
            imgMask1 |= imgMask2;
        }

        //morphological opening 작은 점들을 제거
        Cv2.Erode(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
        Cv2.Dilate(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));

        //morphological closing 영역의 구멍 메우기
        Cv2.Dilate(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
        Cv2.Erode(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));

        imgMask2.Release();

        return imgMask1;
    }
}
