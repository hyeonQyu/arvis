using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCam : MonoBehaviour
{
    /* 웹캠 디바이스에 관련된 구성요소 */
    private const string Tag = "ARVIS) ";

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private GameObject _screen;
    [SerializeField]
    private Text _text;

    WebCamTexture _webcamTexture = null;
    ScreenOrientation _screenOrientation = ScreenOrientation.Portrait;
    CameraClearFlags _cameraClearFlags;

    private void Awake()
    {
        foreach(Camera c in Camera.allCameras)
        {
            if(c != _camera)
                c.cullingMask = ~(1 << _screen.layer);
        }

        _camera.gameObject.SetActive(false);
        _screen.SetActive(false);
        _camera.farClipPlane = _camera.nearClipPlane + 1f;
        _screen.transform.localPosition = new Vector3(0, 0, _camera.farClipPlane * .5f);
       
        WebCamDevice[] devices = WebCamTexture.devices;
        if(devices.Length > 0)
        {
            _webcamTexture = new WebCamTexture(Screen.width, Screen.height);
            _screen.GetComponent<Renderer>().material.mainTexture = _webcamTexture;
        }

        _screenOrientation = Screen.orientation;
        SetOrientation(_screenOrientation);
        StartCoroutine(Orientation());
        Show();
    }

    private void SetOrientation(ScreenOrientation screenOrientation)
    {
        float h = Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * .5f) * _screen.transform.localPosition.z * .2f;

        if(_camera.orthographic)
            h = Screen.height / _camera.pixelHeight;
        
        if(ScreenOrientation.Landscape == screenOrientation)
        {
            Debug.Log(Tag + "1");
            _screen.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
            _screen.transform.localScale = new Vector3(_camera.aspect * h, 1f, h);
        }
        else if(ScreenOrientation.LandscapeLeft == screenOrientation)
        {
            Debug.Log(Tag + "2");
            _screen.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
            _screen.transform.localScale = new Vector3(_camera.aspect * h, 1f, h);
        }
        else if(ScreenOrientation.LandscapeRight == screenOrientation)
        {
            Debug.Log(Tag + "3");
            _screen.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            _screen.transform.localScale = new Vector3(_camera.aspect * h, 1f, h);
        }
        else if(ScreenOrientation.Portrait == screenOrientation)
        {
            Debug.Log(Tag + "4");
            _screen.transform.localRotation = Quaternion.Euler(180f, -90f, 90f);
            _screen.transform.localScale = new Vector3(h, 1f, _camera.aspect * h);
        }
        else if(ScreenOrientation.PortraitUpsideDown == screenOrientation)
        {
            Debug.Log(Tag + "5");
            _screen.transform.localRotation = Quaternion.Euler(0f, 90f, -90f);
            _screen.transform.localScale = new Vector3(h, 1f, _camera.aspect * h);
        }
    }

    IEnumerator Orientation()
    {
        while(true)
        {
            if(_screenOrientation != Screen.orientation)
            {
                _screenOrientation = Screen.orientation;
                SetOrientation(_screenOrientation);
            }
            yield return new WaitForSeconds(.5f);
        }
    }

    private void Show()
    {
        if(_webcamTexture == null)
            return;
        //if(Camera.main != _camera)
        //{
        //    _cameraClearFlags = Camera.main.clearFlags;
        //    Camera.main.clearFlags = CameraClearFlags.Depth;
        //}
        _camera.gameObject.SetActive(true);
        _screen.SetActive(true);
        _webcamTexture.Play();
        _text.text = _webcamTexture.width.ToString() + " " + _webcamTexture.height.ToString();
    }

    private void Update()
    {
        //_screen.GetComponent<Renderer>().material.mainTexture = HandTracker.Process(_webcamTexture);
    }
}
