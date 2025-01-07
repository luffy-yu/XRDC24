using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixieDustBurst : MonoBehaviour {

    [SerializeField] Transform target;

    Vector2 targetPos;

    public ParticleSystem fairyDust;

    // Use this for initialization
    void Start () {

        targetPos = transform.position;
    }

   

    void Update()
    {
     
        targetPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.position = targetPos;

        if (Input.GetMouseButtonDown(0))
        {

            fairyDust.Play();

        }

    }

    
		
	
}
