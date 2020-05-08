using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public class HandManager
{
    private const float AngleConstant = 180 / Mathf.PI;

    // 움직일 물체
    private GameObject _object;

    // 가상 손의 손가락
    private GameObject[] _handObject;

    private Canvas _screen;

    private List<Vector3> _cvt3List;
    public List<Vector3> Cvt3List
    {
        get
        {
            return _cvt3List;
        }
    }

    public HandManager(GameObject movingObject, GameObject[] handObject, Canvas screen)
    {
        _cvt3List = new List<Vector3>();
        _object = movingObject;
        _handObject = handObject;
        _screen = screen;
    }

    public void MoveHand()
    {
        // 가상 손을 움직임
        for(int i = 0; i < _handObject.Length; i++)
        {
            _handObject[i].transform.position = _cvt3List[i] * 12;
        }

        RotateFingers();
    }

    private void RotateFingers()
    {
        Vector3 center = _handObject[0].transform.position;
        float centerX = center.x;
        float centerY = center.y;

        for(int i = 1; i < _handObject.Length; i++)
        {
            Vector3 finger = _handObject[i].transform.position;
            float x = finger.x;
            float y = finger.y;

            float angle = Mathf.Atan2(y - centerY, x - centerX) * AngleConstant;
            Quaternion rotation = _handObject[i].transform.rotation;
            _handObject[i].transform.rotation = Quaternion.Euler(rotation.x, rotation.y, angle - 90);
        }
    }

    // 프레임 이미지의 손가락 끝 좌표들을 유니티 가상공간의 좌표로 변환
    private Vector3 Point2Vector3(Point point)
    {
        Vector3 cvt3 = new Vector3(0, 0, _object.transform.position.z);
        cvt3.x = (point.X - _screen.GetComponent<RectTransform>().sizeDelta.x / 2) * _screen.GetComponent<Transform>().transform.lossyScale.x;
        cvt3.y = (_screen.GetComponent<RectTransform>().sizeDelta.y / 2 - point.Y) * _screen.GetComponent<Transform>().transform.lossyScale.y;
        return cvt3;
    }

    public void InputPoint(List<Point> pointList, Point center)
    {
        _cvt3List.Add(Point2Vector3(center));
        for(int i = 0; i < pointList.Count; i++)
        {
            _cvt3List.Add(Point2Vector3(pointList[i]));
        }
    }
}
