using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderCrew.SeeThroughShader
{
    public class CanvasToggle : MonoBehaviour
    {

        public GameObject myCanvas;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (myCanvas.activeSelf)
                {
                    myCanvas.SetActive(false);
                }
                else
                {
                    myCanvas.SetActive(true);
                }
            }
        }
    }
}