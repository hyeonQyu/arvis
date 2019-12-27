using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    
    // Start is called before the first frame update
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Finger")
        {
            gameObject.GetComponent<Animator>().SetBool("check", true);
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("check", false);
        }
    }
}
