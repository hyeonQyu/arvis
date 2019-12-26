using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public struct DistanceAndIndex
{
    public double distance;
    public int index;

    public DistanceAndIndex(double distance, int index)
    {
        this.distance = distance;
        this.index = index;
    }
}

public class Webcam2:WebCamera
{
    [Header("Object to Move")]
    public GameObject _object;

    [Header("Finger & Center")]
    public GameObject[] _handObject;

    List<Vector3> _cvt3List = new List<Vector3>(); // Converted Vector3 List 

    private int _rangeCount = 0;

    private Scalar _skin = new Scalar(95, 127, 166);
    private Scalar _table = new Scalar(176, 211, 238);

    private Mat _rgbColor;
    private Mat _rgbColor2;
    private Mat _hsvColor;

    private int _hue;
    private int _saturation;
    private int _value;

    private int _lowHue, _highHue;
    private int _lowHue1, _lowHue2, _highHue1, _highHue2;

    private Mat[] _imgFrames;

    private int _frameCount;
    private int _frameIndex;

    // 얼굴 인식을 위한 학습된 모델
    private CascadeClassifier _faceCascadeClassifer;

    // 같은 그룹의 점들을 결정할 거리 임계값
    private int _neighborhoodDistanceThreadhold;

    // 그룹화 되어 간결해진 꼭짓점
    private List<Point> _mainPoint;

    // 녹화할 프레임의 수
    private const int _recordFrameCount = 150;

    private OpenCvSharp.Rect _boundingRect;

    // 손의 중심점과 손의 크기
    private Point _center;
    private double _radius;

    private bool _isCorrectDetection;

    protected override void Awake()
    {
        InitializeHsv();

        _faceCascadeClassifer = new CascadeClassifier(Application.dataPath + "/Resources/haarcascade_frontalface_alt.xml");

        _mainPoint = new List<Point>();

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
        // input 영상이 imgFrame
        Mat imgFrame = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);
        Mat imgMask = new Mat();

        // 얼굴 제거
        RemoveFaces(imgFrame, imgFrame);

        // 피부색 영역만 검출
        imgMask = GetSkinMask(imgFrame);

        // 손의 임시 중앙을 찾음
        _center = GetHandCenter(imgMask);

        // 손의 점을 얻음
        Mat imgHand = GetHandLineAndPoint(imgFrame, imgMask);

        // 손가락 꼭짓점을 찾음
        int fingerNum = 5;
        // fingerPoint에 손가락 끝의 좌표값 저장
        List<Point> fingerPoint = new List<Point>(fingerNum);
        fingerPoint = GetFingerPoint(fingerNum);

        // 가상 손 좌표와 맵핑
        InputPoint(fingerPoint);

        // 가상 손을 움직임
        for(int i = 0; i < _handObject.Length; i++)
        {
            _handObject[i].GetComponent<HandSkeleton>().MoveTransform(_cvt3List[i]);
        }

        // 임시로 점을 찍어 출력
        for(int i = 0; i < fingerNum; i++)
        {
            Cv2.Circle(imgHand, fingerPoint[i], 5, new Scalar(255, 0, 0), -1, LineTypes.AntiAlias);
        }

        _mainPoint.Clear();
        _cvt3List.Clear();

        if(!_isCorrectDetection)
            return false;

        output = OpenCvSharp.Unity.MatToTexture(imgHand, output);
        Debug.Log("반지름: " + _radius);
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

        return imgMask1;
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

    private Point GetHandCenter(Mat img)
    {
        // 거리 변환 행렬을 저장할 변수
        Mat dstMatrix = new Mat();
        Cv2.DistanceTransform(img, dstMatrix, DistanceTypes.L2, DistanceMaskSize.Mask5);

        // 거리 변환 행렬에서 값(거리)이 가장 큰 픽셀의 좌표와 값을 얻어옴
        int[] maxIdx = new int[2];
        double null1;
        int null2;
        Cv2.MinMaxIdx(dstMatrix, out null1, out _radius, out null2, out maxIdx[0], img);

        return new Point(maxIdx[1], maxIdx[0]);
    }

    private Mat GetHandLineAndPoint(Mat img, Mat imgMask1)
    {
        // 원본영상 & 마스크이미지 -> 피부색 영역 검출
        Mat imgSkin = new Mat();
        Cv2.BitwiseAnd(img, img, imgSkin, imgMask1);

        // 피부색 추출 -> GrayScale
        Mat imgGray = new Mat();
        Cv2.CvtColor(imgSkin, imgGray, ColorConversionCodes.BGR2GRAY);
        Cv2.BitwiseNot(imgGray, imgGray);  // 색상반전

        // GrayScale -> Canny
        Mat imgCanny = new Mat();
        Cv2.Canny(imgGray, imgCanny, 100, 200);
        //bitwise_not(imgCanny, imgCanny);  // 색상반전

        // 윤곽선 검출을 위한 변수
        Point[][] contours;
        HierarchyIndex[] hierarchy;

        // 윤곽선 검출하기
        Cv2.FindContours(imgCanny, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

        // 꼭짓점 검출을 위한 변수
        Point[][] hull = new Point[contours.Length][];
        int[][] convexHullIdx = new int[contours.Length][];
        Vec4i[][] defects = new Vec4i[contours.Length][];

        Mat imgHand = Mat.Zeros(imgGray.Size(), MatType.CV_8UC3);
        Mat imgFillHand = Mat.Zeros(imgGray.Size(), MatType.CV_8UC1);
        Cv2.BitwiseNot(imgHand, imgHand);

        // Version_1
        int largestArea = 0;
        int largestContourIndex = 0;
        double a;

        // 가장 큰 contour를 찾고 convexHull, defect 초기화
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

        // defects의 점들을 그룹화 하여 newPoints에 저장
        List<List<Point>> newPoints;
        newPoints = GroupPoint(contours[largestContourIndex], defects[largestContourIndex]);

        if(largestArea > 1)
        {
            Cv2.DrawContours(imgHand, contours, largestContourIndex, new Scalar(0, 255, 0));
            Cv2.DrawContours(imgFillHand, contours, largestContourIndex, new Scalar(255, 0, 0));
            Cv2.FloodFill(imgFillHand, _center, new Scalar(255, 0, 0));
            Cv2.DrawContours(imgHand, hull, largestContourIndex, new Scalar(0, 0, 255));

            Point prevCenter = _center;
            // 손 중앙 갱신
            _center = GetHandCenter(imgFillHand);
            Cv2.Circle(imgHand, _center, /*(int)radius*/5, new Scalar(0, 0, 255));

            // 인식이 부정확하지 않은지 평가
            EvaluateDetection(prevCenter); 

            //// Draw defect  기존의 점과 선을 그리던 함수 -> 나중에 주석처리
            //for(int i = 0; i < defects[largestContourIndex].Length; i++)
            //{
            //    Point start, end, far;
            //    int d = defects[largestContourIndex][i].Item3;

            //    start = contours[largestContourIndex][defects[largestContourIndex][i].Item0];
            //    end = contours[largestContourIndex][defects[largestContourIndex][i].Item1];
            //    far = contours[largestContourIndex][defects[largestContourIndex][i].Item2];

            //    if(d > 1)
            //    {
            //        Scalar scalar = Scalar.RandomColor();
            //        Cv2.Line(imgHand, start, far, scalar, 2, LineTypes.AntiAlias);
            //        Cv2.Line(imgHand, end, far, scalar, 2, LineTypes.AntiAlias);
            //        Cv2.Circle(imgHand, far, 5, scalar, -1, LineTypes.AntiAlias);
            //        Debug.Log(i + " " + far);
            //    }
            //}

            // 새롭게 중요 꼭짓점을 그리는 코드
            for(int i = 0; i < newPoints.Count; i++)
            {
                Point point = new Point(0, 0);
                for(int j = 0; j < newPoints[i].Count; j++)
                {
                    point.X += newPoints[i][j].X;
                    point.Y += newPoints[i][j].Y;
                }
                if(newPoints[i].Count == 0)
                    continue;
                // 평균값으로 꼭짓점 찍기
                point.X = point.X / newPoints[i].Count;
                point.Y = point.Y / newPoints[i].Count;
                _mainPoint.Add(point);
                //Cv2.Circle(imgHand, point, 5, new Scalar(255, 0, 0), -1, LineTypes.AntiAlias);
            }
        }

        return imgHand;
    }

    // 가까운 점들을 그룹화 하는 함수
    private List<List<Point>> GroupPoint(Point[] contours, Vec4i[] defect)
    {
        _neighborhoodDistanceThreadhold = (int) (_radius / 2 * 0.8);

        // 그룹들을 저장할 List
        List<List<Point>> newPoints = new List<List<Point>>();
        for(int i = 0; i < defect.Length; i++)
        {
            newPoints.Add(new List<Point>());
        }

        // 어떠한 그룹에 속한 Index를 저장하여 검사하지 않음
        List<int> groupedIndex = new List<int>();

        for(int i = 0; i < defect.Length - 1; i++)
        {
            // 이미 어떠한 그룹에 속해 있다면 검사하지 않고 넘김
            if(groupedIndex.Contains(i))
                continue;

            newPoints[i].Add(contours[defect[i].Item2]);
            groupedIndex.Add(i);

            for(int j = i + 1; j < defect.Length; j++)
            {
                if(groupedIndex.Contains(j))
                    continue;

                if(_neighborhoodDistanceThreadhold > Math.Abs(contours[defect[i].Item2].X - contours[defect[j].Item2].X) &&
                         _neighborhoodDistanceThreadhold > Math.Abs(contours[defect[i].Item2].Y - contours[defect[j].Item2].Y))
                {
                    newPoints[i].Add(contours[defect[j].Item2]);
                    groupedIndex.Add(j);
                }

            }
        }

        return newPoints;
    }

    private List<Point> GetFingerPoint(int fingerNum = 5)
    {
        // 손가락 좌표를 저장하여 반환할 변수 선언
        List<Point> fingerPoint = new List<Point>(fingerNum);
        //DistanceAndIndex 구조체 변수 선언
        List<DistanceAndIndex> distanceAndIndex = new List<DistanceAndIndex>(_mainPoint.Count);

        for(int i = 0; i < _mainPoint.Count; i++)
        {
            // center와의 거리값과 _maintPoint에서의 인덱스저장(Sorting 되고 난후 엔덱스를 찾기위해)
            distanceAndIndex.Add(new DistanceAndIndex(Math.Exp(_center.X - _mainPoint[i].X) + Math.Exp(_center.Y - _mainPoint[i].Y), i));
        }

        // 내림차순으로 정렬
        distanceAndIndex.Sort((DistanceAndIndex a, DistanceAndIndex b) => -a.distance.CompareTo(b.distance));

        for(int i = 0; i < fingerNum; i++)
        {
            // 가장 멀리있는 Point를  fingerNum 수 만큼 뽑아냄
            fingerPoint.Add(_mainPoint[distanceAndIndex[i].index]);
        }

        return fingerPoint;
    }

    private void EvaluateDetection(Point prevCenter)
    {
        double maxDistance = _radius * 3 / 2;
        if(_radius < 50 || _radius > 170)
            _isCorrectDetection = false;
        else if(maxDistance < Math.Abs(_center.X - prevCenter.X) || maxDistance < Math.Abs(_center.Y - prevCenter.Y))
        {
            _isCorrectDetection = false;
        }

        _isCorrectDetection = true;
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

    private Vector3 Point2Vector3(Point _point)
    {
        Vector3 cvt3 = new Vector3(0, 0, _object.transform.position.z);
        cvt3.x = (_point.X - gameObject.GetComponent<RectTransform>().sizeDelta.x / 2) * gameObject.GetComponent<Transform>().transform.lossyScale.x;
        cvt3.y = (gameObject.GetComponent<RectTransform>().sizeDelta.y / 2 - _point.Y) * gameObject.GetComponent<Transform>().transform.lossyScale.y;
        return cvt3;
    }

    private void InputPoint(List<Point> pointList)
    {
        _cvt3List.Add(Point2Vector3(_center));
        for(int i = 0; i < pointList.Count; i++)
        {
            _cvt3List.Add(Point2Vector3(pointList[i]));
        }
    }
}
