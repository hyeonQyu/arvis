using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using System;
using System.Linq;

public class HandManager
{
    private const float AngleConstant = 180 / Mathf.PI;     // 1도

    // 가상 손의 손가락
    private GameObject _hand;
    private GameObject[] _handObjects;
    private RawImage _screen;
    private List<Vector3> _cvt3List;
    private float _pWidth;
    private float _pHeight;
    private bool isNew;
    private Dictionary<float, Vector3> _angleArray;

    public List<Vector3> Cvt3List
    {
        get
        {
            return _cvt3List;
        }
    }

    public HandManager(GameObject hand, RawImage screen, int width, int height)
    {
        _cvt3List = new List<Vector3>();
        _screen = screen;
        _pWidth = (float)width;
        _pHeight = (float)height;
        _hand = hand;
        _handObjects = new GameObject[_hand.transform.childCount];
        _angleArray = new Dictionary<float, Vector3>();

        for (int i = 0; i < _handObjects.Length; i++)
        {
            _handObjects[i] = _hand.transform.GetChild(i).gameObject;
        }
        isNew = false;
    }

    public void MoveHand(double radius)
    {



        // Lerp 써야할 곳


        // 가상 손 Z Axis 움직임
        Debug.Log("Debug = " + radius);
        // _hand.transform.position = new Vector3(0, 0, _screen.transform.position.z + (float)(radius - 70));

        // 가상 손 X, Y Axis 움직임
        for (int i = 0; i < _handObjects.Length; i++)
        {
            _handObjects[i].transform.localPosition = _cvt3List[i];
        }
    }

    private void SortFingerPoints()
    {
        Vector3 center = _cvt3List[0];
        float centerX = center.x;
        float centerY = center.y;
        //_angleArray 클리어
        _angleArray.Clear();

        for (int i = 1; i < _cvt3List.Count; i++)
        {
            Vector3 finger = _cvt3List[i];
            float x = finger.x;
            float y = finger.y;

            float angle = Mathf.Atan2(y - centerY, x - centerX) * AngleConstant;

            //중복된 거 KEY 있으면 remove
            if (_angleArray.ContainsKey((angle - 90 + 360) % 360))
            {
                _angleArray.Remove((angle - 90 + 360) % 360);
            }
            //각도에 해당하는 vector3 넣기
            _angleArray.Add((angle - 90 + 360) % 360, finger);
            //Quaternion rotation = _handObjects[i].transform.rotation;
            //_handObjects[i].transform.rotation = Quaternion.Euler(rotation.x, rotation.y, angle - 90);
        }

        //KEY(각도) 오름차순 정렬
        var list = _angleArray.Keys.ToList();
        list.Sort();

        //KEY(각도) 값에 따른 vector3 값 넣기
        for (int i = 1; i < list.Count; i++)
        {
            _cvt3List[i] = _angleArray[list[i - 1]];
            Debug.Log(list[i - 1] + " " + _cvt3List[i]);
        }

        //_angleArray.Clear();
    }

    // 프레임 이미지의 손가락 끝 좌표들을 유니티 가상공간의 좌표로 변환
    private Vector3 Point2Vector3(Point point)
    {
        Vector3 cvt3 = new Vector3(_screen.transform.position.x, _screen.transform.position.y, 0);

        if (WebCam.IsAndroid)
        {
            cvt3.x += (point.X / _pWidth - 0.5f) * _screen.rectTransform.sizeDelta.x * _screen.transform.lossyScale.x;
        }
        else
        {
            cvt3.x += (0.5f - point.X / _pWidth) * _screen.rectTransform.sizeDelta.x * _screen.transform.lossyScale.x;
        }

        cvt3.y += (0.5f - point.Y / _pHeight) * _screen.rectTransform.sizeDelta.y * _screen.transform.lossyScale.y;

        return cvt3;
    }

    public void InputPoint(List<Point> pointList, Point center)
    {
        isNew = true;
        // Center
        _cvt3List.Add(Point2Vector3(center));

        // The others
        for (int i = 0; i < pointList.Count; i++)
        {
            _cvt3List.Add(Point2Vector3(pointList[i]));
        }
        SortFingerPoints();
        // Sort Finger Point
    }
    // IEnumerator LerpCoroutine()
    // {
    //     Debug.Log("Start Lerp Coroutine");

    //     while(isNew)
    //     {



    //     }
    //     // 새로운 좌표 들어오기 전에 끝났을 때
    //     isNew = false;

    //     yield return null;      // 다음 Frame
    // }
}
