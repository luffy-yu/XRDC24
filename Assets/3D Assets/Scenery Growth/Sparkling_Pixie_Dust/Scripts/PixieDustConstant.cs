using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixieDustConstant : MonoBehaviour {

    [SerializeField] Transform target;

    Vector2 targetPos;

    public ParticleSystem fairyDust;

    private bool dustActive = false;
    private bool dustChange = false;

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

            if (dustActive == false)
            {
                fairyDust.Play();
                dustChange = true;
                dustActive = true;
                print("start");
            }

            if (dustActive == true)
            {

                if (dustChange == false)
                {
                    fairyDust.Stop();
                    dustActive = true;
                    dustActive = false;
                    print("stop");
                }
            }

            dustChange = false;

        }

    }

    
		
	
}
