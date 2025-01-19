using System;
using System.Collections;
using UnityEngine;
using XRDC24.Environment;
using XRDC24.Helper;

namespace XRDC24.Demo
{
    public class DemoController : MonoBehaviour
    {
        public GameObject part1Root;
        public ModuleManagerHack moduleManagerHack;

        public GameObject part2Root;
        public CustomInputHandler customInputHandler;

        public FullScreenImage splashScreen;

        public Camera part1Camera;
        public Camera part2Camera;

        public RoomDissolveController dissolveController;

        public HTMLTextOnGUI htmlTextOnGUI;

        private int p1CameraCullingMask;
        private int p2CameraCullingMask;

        void Start()
        {
            // only use part2 camera
            p1CameraCullingMask = part1Camera.cullingMask;
            p2CameraCullingMask = part2Camera.cullingMask;

            // enable p1 camera;
            part1Camera.enabled = true;
            // disable p2 camera
            part2Camera.enabled = false;

            part2Camera.gameObject.GetComponent<AudioListener>().enabled = false;

            moduleManagerHack.SwitchToPart2 += SwitchToPart2;
            customInputHandler.frameController.OnFinalFrame += OnFinalFrame;

            // disable part2 first
            part2Root.SetActive(false);
            customInputHandler.MyTurn = false;

            dissolveController.inDebugMode = false;

            // disable help text first
            htmlTextOnGUI.gameObject.SetActive(false);

            // disable action first
            moduleManagerHack.nextActionAvailable = false;

            splashScreen.OnStartVideoEnded += OnStartVideoEnded;
            splashScreen.OnEndVideoEnded += OnEndVideoEnded;
        }

        private void OnStartVideoEnded()
        {
            print("starting video ended");
            // enable help text
            htmlTextOnGUI.gameObject.SetActive(true);
            // enable action
            moduleManagerHack.nextActionAvailable = true;
        }

        private void OnEndVideoEnded()
        {
            // disable splash
            splashScreen.DisableSplash();
            // go to start
            BackToStart();
        }

        private void OnFinalFrame()
        {
            // show splash screen
            splashScreen.ShowEnd();

            // disable interaction
            part2Root.SetActive(false);
            customInputHandler.MyTurn = false;

            // disable help text
            htmlTextOnGUI.gameObject.SetActive(false);
        }

        void BackToStart()
        {
            part2Camera.enabled = false;
            part1Camera.enabled = true;

            // revert disolve
            dissolveController.Revert();
            // show part1
            part1Root.SetActive(true);
            // reset
            moduleManagerHack.BackToStart();

            // show help text
            htmlTextOnGUI.gameObject.SetActive(true);
        }

        private void SwitchToPart2()
        {
            part1Camera.enabled = false;
            part2Camera.enabled = true;

            customInputHandler.MyTurn = true;

            part1Camera.gameObject.GetComponent<AudioListener>().enabled = false;
            part2Camera.gameObject.GetComponent<AudioListener>().enabled = true;

            // disolve
            dissolveController.Dissolve();

            // clear portals
            moduleManagerHack.ClearPortals();

            // disable stage1
            part1Root.SetActive(false);
            moduleManagerHack.gameObject.SetActive(false);
            // enable stage2
            part2Root.SetActive(true);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SwitchToPart2();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                OnFinalFrame();
            }
#endif
        }
    }
}