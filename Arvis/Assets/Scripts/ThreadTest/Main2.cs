using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main2 : MonoBehaviour
{
    int frame;

    // Start is called before the first frame update
    void Start()
    {
        frame = 0;
    }

    // Update is called once per frame
    void Update()
    {
        frame++;

        int a = 0;
        for(int i = 0; i < 800000000; i++)
        {
            a++;
        }
        Debug.Log("No Thread" + frame);
    }
}
