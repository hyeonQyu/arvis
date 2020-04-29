using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    int i = 0;
    int speed = 10;

    // Update is called once per frame
    void Update()
    {
        Vector3 moving = new Vector3(0, 0, 0);

        if(i < 60)
        {
            moving = (Vector3.left * speed * Time.deltaTime);
        }
        else if(i < 120)
        {
            moving = (Vector3.right * speed * Time.deltaTime);
        }
        else
        {
            i = 0;
        }
        transform.Translate(moving);
        //transform.Rotate(moving);
        i++;
    }
}
