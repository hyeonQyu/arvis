using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;

public struct DistanceAndIndex
{
    public double Distance;
    public int Index;

    public DistanceAndIndex(double distance, int index)
    {
        this.Distance = distance;
        this.Index = index;
    }
}

public class HandDetector
{
    // 같은 그룹의 점들을 결정할 거리 임계값
    private int _neighborhoodDistanceThreshold;

    // 그룹화 되어 간결해진 꼭짓점
    private List<Point> _mainPoint;
    public List<Point> MainPoint
    {
        get
        {
            return _mainPoint;
        }
    }

    // 손의 중심점과 손의 크기
    private Point _center;
    public Point Center
    {
        get
        {
            return _center;
        }
    }
    private double _radius;

    // 손가락 끝 점
    private List<Point> _fingerPoint;
    public List<Point> FingerPoint
    {
        get
        {
            return _fingerPoint;
        }
    }

    // 인식이 제대로 이루어졌는지 확인
    private bool _isCorrectDetection;
    public bool IsCorrectDetection
    {
        set
        {
            _isCorrectDetection = value;
        }
        get
        {
            return _isCorrectDetection;
        }
    }

    public HandDetector()
    {
        _mainPoint = new List<Point>();
    }

    public Mat GetHandLineAndPoint(Mat img, Mat imgMask)
    {
        // 손의 임시 중앙
        _center = GetHandCenter(imgMask);

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
            //        //Cv2.Line(imgHand, start, far, scalar, 2, LineTypes.AntiAlias);
            //        //Cv2.Line(imgHand, end, far, scalar, 2, LineTypes.AntiAlias);
            //        //Cv2.Circle(imgHand, end, 5, scalar, -1, LineTypes.AntiAlias);
            //        Debug.Log(i + " " + end);
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
                //Cv2.Circle(imgHand, point, 5, new Scalar(0, 255, 0), -1, LineTypes.AntiAlias);
            }
        }

        return imgHand;
    }

    public void DrawFingerPointAtImg(Mat img)
    {
        int fingerNum = 5;
        _fingerPoint = GetFingerPoint(fingerNum);

        // 임시로 점을 찍어 출력
        for(int i = 0; i < fingerNum; i++)
        {
            Cv2.Circle(img, _fingerPoint[i], 5, new Scalar(255, 0, 0), -1, LineTypes.AntiAlias);
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

    // 가까운 점들을 그룹화 하는 함수
    private List<List<Point>> GroupPoint(Point[] contours, Vec4i[] defect)
    {
        _neighborhoodDistanceThreshold = (int)(_radius / 2 * 0.8);

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

            newPoints[i].Add(contours[defect[i].Item1]);
            groupedIndex.Add(i);

            for(int j = i + 1; j < defect.Length; j++)
            {
                if(groupedIndex.Contains(j))
                    continue;

                if(_neighborhoodDistanceThreshold > Math.Abs(contours[defect[i].Item1].X - contours[defect[j].Item1].X) &&
                         _neighborhoodDistanceThreshold > Math.Abs(contours[defect[i].Item1].Y - contours[defect[j].Item1].Y))
                {
                    newPoints[i].Add(contours[defect[j].Item1]);
                    groupedIndex.Add(j);
                }

            }
        }

        return newPoints;
    }

    // 손가락 끝점을 얻음
    private List<Point> GetFingerPoint(int fingerNum = 5)
    {
        // 손가락 좌표를 저장하여 반환할 변수 선언
        List<Point> fingerPoint = new List<Point>(fingerNum);
        //DistanceAndIndex 구조체 변수 선언
        List<DistanceAndIndex> distanceAndIndex = new List<DistanceAndIndex>(_mainPoint.Count);

        for(int i = 0; i < _mainPoint.Count; i++)
        {
            // center와의 거리값과 _maintPoint에서의 인덱스저장(Sorting 되고 난후 엔덱스를 찾기위해)
            distanceAndIndex.Add(new DistanceAndIndex(Math.Sqrt(Math.Pow(_center.X - _mainPoint[i].X, 2) + Math.Pow(_center.Y - _mainPoint[i].Y, 2)), i));
        }

        // 내림차순으로 정렬
        distanceAndIndex.Sort((DistanceAndIndex a, DistanceAndIndex b) => -a.Distance.CompareTo(b.Distance));

        for(int i = 0; i < fingerNum; i++)
        {
            // 가장 멀리있는 Point를  fingerNum 수 만큼 뽑아냄
            fingerPoint.Add(_mainPoint[distanceAndIndex[i].Index]);
        }

        return fingerPoint;
    }

    // 인식이 제대로 이루어졌는지 평가
    private void EvaluateDetection(Point prevCenter)
    {
        //Debug.Log("이전 중앙: " + prevCenter);
        //Debug.Log("현재 중앙: " + _center);
        //Debug.Log("반지름: " + _radius);

        double maxDistance = _radius * 3 / 2;
        if(_radius < 30 || _radius > 190)
        {
            //Debug.Log("반지름이 너무 작거나 큼!----------------------------------------------------------------------------------");
            _isCorrectDetection = false;
            return;
        }
        else if(maxDistance < Math.Abs(_center.X - prevCenter.X) || maxDistance < Math.Abs(_center.Y - prevCenter.Y))
        {
            //Debug.Log("중앙이 많이 차이남!---------------------------------------------------------------------------------------");
            _isCorrectDetection = false;
            return;
        }

        //Debug.Log("잘됨!-----------------------------------------------------------------------------------------------------");
        _isCorrectDetection = true;
    }
}
