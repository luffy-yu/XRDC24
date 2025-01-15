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

            moduleManagerHack.SwitchToPart2 += SwitchToPart2;
            customInputHandler.frameController.OnFinalFrame += OnFinalFrame;
            // disable part2 first
            part2Root.SetActive(false);
            customInputHandler.MyTurn = false;
            
            dissolveController.inDebugMode = false;
        }

        private void OnFinalFrame()
        {
            // show splash screen, disable all interactions
            part2Root.SetActive(false);
            customInputHandler.gameObject.SetActive(false);
            // enable splash
            splashScreen.ShowEnd();
        }

        private void SwitchToPart2()
        {
            part1Camera.enabled = false;
            part2Camera.enabled = true;

            customInputHandler.MyTurn = true;
            
            // disolve
            dissolveController.Dissolve();

            // disable stage1
            part1Root.SetActive(false);
            moduleManagerHack.gameObject.SetActive(false);
            // enable stage2
            part2Root.SetActive(true);
        }
    }
}