using UnityEngine;
using XRDC24.Helper;

namespace XRDC24.Demo
{
    public class DemoController: MonoBehaviour
    {
        public GameObject part1Root;
        public ModuleManagerHack moduleManagerHack;
        
        public GameObject part2Root;
        public CustomInputHandler customInputHandler;

        public FullScreenImage splashScreen;

        void Start()
        {
            moduleManagerHack.SwitchToPart2 += SwitchToPart2;
            customInputHandler.frameController.OnFinalFrame += OnFinalFrame;
            // disable part2 first
            part2Root.SetActive(false);
            customInputHandler.gameObject.SetActive(false);
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
            // disable stage1
            part1Root.SetActive(false);
            moduleManagerHack.gameObject.SetActive(false);
            // enable stage2
            part2Root.SetActive(true);
            customInputHandler.gameObject.SetActive(true);
        }
    }
}