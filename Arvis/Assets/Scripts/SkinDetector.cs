using OpenCvSharp;
using UnityEngine;
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
    private const int DefaultSkinColor = 0;
    private const int ExtractedSkinColor = 1;

    public bool IsExtractedSkinColor { get; private set; }

    private int _rangeCount;

    private int[] _lowHue1;
    private int[] _lowHue2;
    private int[] _highHue1;
    private int[] _highHue2;

    public HandBoundary HandBoundary { get; set; }
    public Mat ImgOrigin { private get; set; }
    private Mat _imgHandSection;

    public SkinDetector()
    {
        HandBoundary = new HandBoundary();

        _lowHue1 = new int[2];
        _lowHue2 = new int[2];
        _highHue1 = new int[2];
        _highHue2 = new int[2];

        InitializeHsv();
    }

    // HSV를 위한 기본 값 세팅
    private void InitializeHsv(int r = 166, int g = 127, int b = 95, bool isExtractedSkinColor = false)
    {
        Scalar skin = new Scalar(b, g, r);

        Mat rgbColor = new Mat(1, 1, MatType.CV_8UC3, skin);
        Mat hsvColor = new Mat();

        Cv2.CvtColor(rgbColor, hsvColor, ColorConversionCodes.BGR2HSV);

        int hue = (int)hsvColor.At<Vec3b>(0, 0)[0];
        int saturation = (int)hsvColor.At<Vec3b>(0, 0)[1];
        int value = (int)hsvColor.At<Vec3b>(0, 0)[2];

        int lowHue = hue - 10;
        int highHue = hue + 10;

        int colorIndex = DefaultSkinColor;
        if(isExtractedSkinColor)
            colorIndex = ExtractedSkinColor;

        if(lowHue < 0)
        {
            _rangeCount = 2;

            _highHue1[colorIndex] = 180;
            _lowHue1[colorIndex] = lowHue + 180;
            _highHue2[colorIndex] = highHue;
            _lowHue2[colorIndex] = 0;
        }
        else if(highHue > 180)
        {
            _rangeCount = 2;

            _highHue1[colorIndex] = lowHue;
            _lowHue1[colorIndex] = 180;
            _highHue2[colorIndex] = highHue - 180;
            _lowHue2[colorIndex] = 0;  
        }
        else
        {
            _rangeCount = 1;

            _lowHue1[colorIndex] = lowHue;
            _highHue1[colorIndex] = highHue;
        }
    }

    // 피부색을 검출하여 마스크 이미지를 만듦
    public Mat GetSkinMask(Mat img, bool isExtractedSkinColor = false, int minCr = 128, int maxCr = 170, int minCb = 73, int maxCb = 158)
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

        int colorIndex = DefaultSkinColor;
        if(isExtractedSkinColor)
            colorIndex = ExtractedSkinColor;

        Cv2.InRange(imgHsv, new Scalar(_lowHue1[colorIndex], 50, 50), new Scalar(_highHue1[colorIndex], 255, 255), imgMask1);

        if(_rangeCount == 2)
        {
            Cv2.InRange(imgHsv, new Scalar(_lowHue2[colorIndex], 50, 50), new Scalar(_highHue2[colorIndex], 255, 255), imgMask2);
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

    public void SetSkinColor()
    {
        // 원본 이미지에서 손이 있는 영역 이미지만 추출
        GetHandSection();

        // HSV를 기본값으로 설정
        InitializeHsv();

        // 마스크를 통해 피부색 영역만 검출
        Mat imgMaskHandSection = GetSkinMask(_imgHandSection);
        Mat imgSkin = new Mat();
        Cv2.BitwiseAnd(_imgHandSection, _imgHandSection, imgSkin, imgMaskHandSection);

        int r, g, b;
        GetHandColor(imgSkin, out r, out g, out b);

        // 추출한 피부색으로 HSV 설정
        InitializeHsv(r, g, b, true);
        IsExtractedSkinColor = true;
    }

    private unsafe void GetHandSection()
    {
        int width, height;
        width = HandBoundary.Right - HandBoundary.Left + 1;
        height = HandBoundary.Bottom - HandBoundary.Top + 1;

        _imgHandSection = new Mat(height, width, MatType.CV_8UC3);

        byte* destPtr = _imgHandSection.DataPointer;
        byte* srcPtr = ImgOrigin.DataPointer;

        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                long destIndex = (i * _imgHandSection.Step()) + (_imgHandSection.ElemSize() * j);
                long srcIndex = (HandBoundary.Top + i) * ImgOrigin.Step() + ImgOrigin.ElemSize() * (HandBoundary.Left + j);
                destPtr[destIndex + 0] = srcPtr[srcIndex + 0];
                destPtr[destIndex + 1] = srcPtr[srcIndex + 1];
                destPtr[destIndex + 2] = srcPtr[srcIndex + 2];
            }
        }
    }

    private unsafe void GetHandColor(Mat img, out int r, out int g, out int b)
    {
        r = g = b = 0;
        int count = 0;

        byte* ptr = img.DataPointer;
        int cols = img.Cols;
        int rows = img.Rows;

        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < cols; j++)
            {
                long index = i * img.Step() + j * img.ElemSize();
                
                int bTmp = ptr[index + 0];
                int gTmp = ptr[index + 1];
                int rTmp = ptr[index + 2];

                // 피부 영역이 아님
                if(rTmp + gTmp + bTmp == 0)
                    continue;

                r += rTmp;
                g += gTmp;
                b += bTmp;
                count++;
            }
        }

        // 피부색의 RGB 평균
        r /= count;
        g /= count;
        b /= count;
    }
}
