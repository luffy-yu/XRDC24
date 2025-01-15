using System;
using UnityEngine;

namespace XRDC24.Helper
{
    public class HTMLTextOnGUI : MonoBehaviour
    {
        private string htmlText =
            "<size=50><color=white><b>Float Mind</b></color></size>\n\n" +
            "<size=25><color=white>By Zia, Jennifer, Tingting, Muki, and Luffy</color></size>\n" +
            "<size=25><color=black>-------------------------------------------------------</color></size>\n" +
            "<color=white>WASD: Move Camera</color>\n" +
            "<color=white>Mouse: Rotate Camera</color>\n" +
            "<color=white>ESC: Lock Rotation</color>\n" +
            "<color=white>Space: Reset Camera</color>\n" +
            "<color=white>(N)ext, (P)revious: Frame Control</color>\n" +
            "<size=25><color=black>-------------------------------------------------------</color></size>\n" +
            "<size=15><i><color=yellow>Design Review Purpose Only,</color></i></size>\n" +
            "<size=15><i><color=yellow>Fully Functional Product Exclusive to Headsets</color><i></size>\n" +
            "";

        private GUIStyle richTextStyle;

        private bool showing = false;

        private string defaultText = "<b>Press Tab for Help</b>";

        void Start()
        {
            richTextStyle = new GUIStyle
            {
                richText = true,
                fontSize = 20,
                normal = { textColor = Color.white }
            };
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            if (showing)
            {
                GUILayout.Label(htmlText, richTextStyle);
            }
            else
            {
                GUILayout.Label(defaultText, richTextStyle);
            }

            GUILayout.EndVertical();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                showing = true;
            }

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                showing = false;
            }
        }
    }
}