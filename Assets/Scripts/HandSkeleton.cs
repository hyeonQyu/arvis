using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSkeleton:MonoBehaviour
{
    //[Header("UpTarget & DownTarget")]
    //public Transform upTarget;
    //public Transform downTarget;

    //[Header("Check Target")]
    //[SerializeField]
    //private bool isUpTarget = false;
    //[SerializeField]
    //private bool isDownTarget = false;

    //// Update is called once per frame
    ////void Update()
    ////{
    ////    if(isUpTarget)
    ////    {
    ////        transform.up = upTarget.position - transform.position; // 방향 변경
    ////    }
    ////    if(isUpTarget && isDownTarget)
    ////    {
    ////        transform.position = (upTarget.transform.position + downTarget.transform.position) / 2; // 위치 변경
    ////    }
    ////}

    public void MoveTransform(Vector3 mvt3)
    {
        gameObject.transform.position = mvt3;
    }
}