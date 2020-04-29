using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField, Header("Rope")]
    private GameObject _rope;

    [SerializeField, Header("Particle System")]
    private ParticleSystem particle;

    private float _blWeight = 0.0f;
    private bool _blCheck = false;


    private void Update()
    {
        if (transform.position.x <= 66.0f && transform.position.x >= 62.0f && transform.position.y <= 34.0f && transform.position.y >= 27.0f)
        {
            Debug.Log("Goal!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            if (!_blCheck)
            {
                particle.Play();
                _blCheck = true;
                InvokeRepeating("IncreaseWeight", 0f, 0.01f);
            }
        }
    }

    private void IncreaseWeight()
    {
        if (_blWeight >= 100.0f)
        {
            _blCheck = false;
            _blWeight = 0.0f;
            CancelInvoke("IncreaseWeight");
        }
        _rope.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, _blWeight++);
    }

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
