using System;
using UnityEngine;
using Random = System.Random;

namespace XRDC24.Bubble
{

    public class RigidbodyFallSimulator : MonoBehaviour
    {
        public float fallSpeed = 0.01f;
        public float movementSpeed = 2f;

        private bool falling = false;
        
        // stablize
        [Tooltip("Change horizontal movement every shuffle frames")]
        public int shuffle = 20;

        private int count = 0;

        private float xOffset;
        private float zOffset;

        private Random random = new Random();

        void Start()
        {
            count = 0;
            falling = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartFalling();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                StopFalling();
            }

            if (falling)
            {
                if (count % shuffle == 0)
                {
                    var rand = random.Next(2) == 1;
                    var flag = random.Next(2) == 1;

                    var x = 0f;
                    var z = 0f;

                    var sign = flag ? -1 : 1;

                    if (rand)
                    {
                        xOffset = (float)random.NextDouble() * movementSpeed * Time.deltaTime * sign;
                        zOffset = 0f;
                    }
                    else
                    {
                        zOffset = (float)random.NextDouble() * movementSpeed * Time.deltaTime * sign;
                        zOffset = 0f;
                    }
                    // reset
                    count = 0;
                }

                Vector3 currentPosition = transform.position;
                Vector3 newPosition = new Vector3(currentPosition.x + xOffset,
                    currentPosition.y - fallSpeed * Time.deltaTime, currentPosition.z + zOffset);

                transform.position = newPosition;
                
                count += 1;
            }
        }

        void DisableFloatingAnimation()
        {
            var go = transform.parent.gameObject;
            go.GetComponent<BubbleFloatingAnimation>().enabled = false;
        }

        public void StartFalling()
        {
            // disable parent bubble floating animation
            // DisableFloatingAnimation();
            falling = true;
        }

        public void StopFalling()
        {
            falling = false;
        }
    }
}