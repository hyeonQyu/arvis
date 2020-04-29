using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;
        Input.gyro.updateInterval = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        GyroRotate();
    }

    private void GyroRotate()
    {
        Quaternion transquat = Quaternion.identity;
        transquat.w = Input.gyro.attitude.w;
        transquat.x = -Input.gyro.attitude.x;
        transquat.y = -Input.gyro.attitude.y;
        transquat.z = Input.gyro.attitude.z;

        transform.rotation = Quaternion.Euler(90, 0, 180) * transquat;
    }
}
