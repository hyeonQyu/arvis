using System.Collections;
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
    private List<Vector3> _preCvt3List;
    private Vector3[] _startVec3;
    private List<float> _angleList;
    private float _pWidth;
    private float _pHeight;
    private const float _speed = 10.0f;
    private Vector3 zAxis;
    private bool isStart;
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
        _preCvt3List = new List<Vector3>();
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
        isStart = true;

        _startVec3 = new Vector3[_hand.transform.childCount + 1];
    }

    public void InputPoint(List<Point> pointList, Point center)
    {
        // Clear List for new one
        if(_cvt3List.Count != 0)
        {
            _cvt3List.Clear();
            _angleList.Clear();
        }

        // _cvt3List : Center (Index : 0)
        _cvt3List.Add(Point2Vector3(center));

        // _cvt3List : Finger points(Index : 1 ~ 5)
        for (int i = 0; i < pointList.Count; i++)
        {
            _cvt3List.Add(Point2Vector3(pointList[i]));
        }

        // Sort Fingers' Point
        SortFingerPoints();
    }

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

    private void SortFingerPoints()
    {
        // Center
        Vector3 center = new Vector3(_cvt3List[0].x, _cvt3List[0].y, _cvt3List[0].z);

        // Finger
        Vector3 finger;
        for (int i = 1; i < _cvt3List.Count; i++)
        {
            finger = new Vector3(_cvt3List[i].x, _cvt3List[i].y, _cvt3List[i].z);

            float angle = Mathf.Atan2(finger.y - center.y, finger.x - center.x) * AngleConstant;
      
             //중복된 거 KEY 있으면 remove
            if (_angleArray.ContainsKey((angle - 90 + 360) % 360))
            {
                _angleArray.Remove((angle - 90 + 360) % 360);
            }
            
            // 각도에 해당하는 vector3 넣기
            _angleArray.Add((angle - 90 + 360) % 360, finger);
        }
        
        // KEY(각도) 오름차순 정렬
        _angleList = _angleArray.Keys.ToList();
        _angleList.Sort();

        // KEY(각도) 값에 따른 vector3 값 넣기
        for (int i = 1; i < _angleList.Count + 1; i++)
        {
            _cvt3List[i] = _angleArray[_angleList[i-1]];
        }
        _angleArray.Clear();
    }

    public void MoveHand(float radius)
    {
        // Only choose one
        MoveSmooth(radius);
        //MoveHard(radius);
    }

    private void MoveHard(float radius)
    {
        // Round up radius and for Z axis Standard
        radius = Mathf.Round(radius*0.1f) * 10 - 70;

        // Z Axis
        _hand.transform.localPosition = new Vector3(0, 0, _screen.transform.position.z);
        //_hand.transform.localPosition = new Vector3(0, 0, _screen.transform.position.z + radius);
        
        // X, Y Axis
        for(int i=0; i<_cvt3List.Count; i++)
        {
            _handObjects[i].transform.localPosition = _cvt3List[i];
        }
        
        // Fingers' direction follow Center
        //RotateFingers();
    }

    private void MoveSmooth(float radius)
    {
        // Move smooth(From the second)
        if(!isStart)
        {   // Clamp Vector3 by radius
            _cvt3List[0] = _preCvt3List[0] + Vector3.ClampMagnitude(_cvt3List[0] - _preCvt3List[0], radius * 2.5f);
            for(int i=1; i<_cvt3List.Count; i++)
            {
                _cvt3List[i] = _preCvt3List[i] + Vector3.ClampMagnitude(_cvt3List[i] - _preCvt3List[i], radius * 1.5f);
            }

            // Round up radius and for Z axis Standard
            radius = Mathf.Round(radius*0.1f) * 10 - 70;

            // Z Axis for Move smooth
            zAxis = new Vector3(0, 0, _screen.transform.position.z);    // (0, 0, 100)
            //zAxis = new Vector3(0, 0, _screen.transform.position.z + radius); // (radius - 70) 수정

            // Start MoveSmooth Coroutine on the WebCam
            _screen.gameObject.GetComponent<WebCam>().StartMoveSmooth(MoveSmooth());
        }   // Start(Only first time)
        else
        {   // Round up radius and for Z axis Standard
            radius = Mathf.Round(radius*0.1f) * 10 - 70;
            
            // Z Axis
            _hand.transform.localPosition = new Vector3(0, 0, _screen.transform.position.z);
            //_hand.transform.localPosition = new Vector3(0, 0, _screen.transform.position.z + radius);
            
            // X, Y Axis
            for(int i=0; i<_cvt3List.Count; i++)
            {
                _handObjects[i].transform.localPosition = _cvt3List[i];
            }

            // MoveSmooth for next time
            isStart = false;

            // For Clamp(X, Y Axis)
            for(int i=0; i<_cvt3List.Count; i++)
            {
                _preCvt3List.Add(new Vector3(_cvt3List[i].x, _cvt3List[i].y, 0));
            }   // (Z Axis)
            _preCvt3List.Add(new Vector3(0, 0, _hand.transform.localPosition.z));
            
            // Fingers' direction follow Center
            RotateFingers();
        }

    }
    private void RotateFingers()
    {
        Debug.Log("cv3Listcount = " + _cvt3List.Count + "   handobjectsCount = " + _handObjects.Length + "    angleListCount = "+_angleList.Count);
        for(int i = 1; i < _cvt3List.Count; i++)
        {
            Quaternion rotation = _handObjects[i].transform.rotation;
            _handObjects[i].transform.rotation = Quaternion.Euler(rotation.x, rotation.y, _angleList[i-1]);   // angle값 변경
        }
    }

    private IEnumerator MoveSmooth()
    {
        float rate = 0.0f;
        for(int i=0; i<_preCvt3List.Count; i++)
        {
            _startVec3[i] = _preCvt3List[i];
        }

        // 가속도 수정, 탈출 조건 수정
        while(true)
        {
            //Debug.Log("Move smooth");
            // Start Lerp and Move
            rate += Time.deltaTime * _speed;    // 0.1초만에 도달하게(_speed = 10.0f)
            Debug.Log("rate = "+rate);
            // Lerp and Move Z Axis
            _hand.transform.localPosition = new Vector3(0, 0, Mathf.Lerp(_startVec3.Last().z, zAxis.z, rate));

            // Lerp and Move X, Y Axis
            for(int i=0; i<_cvt3List.Count; i++)
            {
                _handObjects[i].transform.localPosition = new Vector3(
                        Mathf.Lerp(_startVec3[i].x, _cvt3List[i].x, rate), 
                    Mathf.Lerp(_startVec3[i].y, _cvt3List[i].y, rate),
                0);
            }

            // Fingers' direction follow Center
            RotateFingers();
            
            // For Next Clamp (X, Y Axis)
            _preCvt3List.Clear();
            for(int i=0; i< _handObjects.Length; i++)
            {
                _preCvt3List.Add(new Vector3(_handObjects[i].transform.localPosition.x, _handObjects[i].transform.localPosition.y, 0));
            }   // (Z Axis)
            _preCvt3List.Add(new Vector3(0, 0, _hand.transform.localPosition.z));

            // Finish MoveSmooth
            if(rate>=1){break;}

            Debug.Log("Time.deltaTime / _speed" + Time.deltaTime/_speed);
            yield return new WaitForSeconds(Time.deltaTime / _speed);   // 1초당 걸리는 시간 / speed(10.0f)
        }
        Debug.Log("Finish MoveSmooth Coroutine");
        // Wait for next frame(already finished lerp)
        yield return null;
    }
}