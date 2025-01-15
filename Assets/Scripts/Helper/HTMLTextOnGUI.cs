using UnityEngine;

namespace XRDC24.Helper
{
    public class HTMLTextOnGUI : MonoBehaviour
    {
        private string htmlText =
            "<b>Bold Text</b>\n<i>Italic Text</i>\n<color=red>Red Text</color>\n<size=24>Large Text</size>";

        private GUIStyle richTextStyle;

        void Start()
        {
            richTextStyle = new GUIStyle
            {
                richText = true,
                fontSize = 16, 
                normal = { textColor = Color.white }
            };
        }

        void OnGUI()
        {
            Rect textRect = new Rect(10, 10, 400, 200);
            
            GUI.Label(textRect, htmlText, richTextStyle);
        }
    }
}