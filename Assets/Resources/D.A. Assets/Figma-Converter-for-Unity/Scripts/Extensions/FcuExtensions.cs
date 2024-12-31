using DA_Assets.FCU.Drawers;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DA_Assets.FCU.Extensions
{
    public static class FcuExtensions
    {
        public static bool IsScrollContent(this string objectName)
        {
            if (objectName.IsEmpty())
                return false;

            objectName = objectName.ToLower();
            objectName = Regex.Replace(objectName, "[^a-z]", "");
            return objectName == "content";
        }

        public static bool IsScrollViewport(this string objectName)
        {
            if (objectName.IsEmpty())
                return false;

            objectName = objectName.ToLower();
            objectName = Regex.Replace(objectName, "[^a-z]", "");
            return objectName == "viewport";
        }

        public static bool IsInputTextArea(this string objectName)
        {
            if (objectName.IsEmpty())
                return false;

            objectName = objectName.ToLower();
            objectName = Regex.Replace(objectName, "[^a-z]", "");
            return objectName == "textarea";
        }

        public static bool IsCheckmark(this string objectName)
        {
            if (objectName.IsEmpty())
                return false;

            objectName = objectName.ToLower();
            objectName = Regex.Replace(objectName, "[^a-z]", "");
            return objectName == "checkmark";
        }

        public static bool IsJsonNetExists(this FigmaConverterUnity fcu)
        {
#pragma warning disable CS0162
#if JSONNET_EXISTS
            return true;
#endif
            return false;
#pragma warning restore CS0162
        }

        public static IEnumerator ReEnableRectTransform(this FigmaConverterUnity fcu)
        {
            fcu.gameObject.SetActive(false);
            yield return WaitFor.Delay01();
            fcu.gameObject.SetActive(true);
        }

        public static bool TryParseSpriteName(this string spriteName, out float scale, out System.Numerics.BigInteger hash)
        {
            try
            {
                if (spriteName.IsEmpty())
                {
                    throw new Exception($"Sprite name is empty.");
                }

                char delimiter = ' ';

                string withoutEx = Path.GetFileNameWithoutExtension(spriteName);
                List<string> nameParts = withoutEx.Split(delimiter).ToList();

                if (nameParts.Count < 2)
                {
                    throw new Exception($"nameParts.Count < 2: {spriteName}");
                }

                string _hash = nameParts[nameParts.Count() - 1];
                string _scale = nameParts[nameParts.Count() - 2].Replace("x", "");

                bool scaleParsed = _scale.TryParseWithDot(out scale);
                bool hashParsed = System.Numerics.BigInteger.TryParse(_hash, out hash);

                if (scaleParsed == false)
                {
                    throw new Exception($"Cant parse scale from name: {spriteName}");
                }

                if (hashParsed == false)
                {
                    throw new Exception($"Cant parse hash from name: {spriteName}");
                }

                return true;
            }
            catch (Exception ex)
            {
                DALogger.LogException(ex);
                scale = 1;
                hash = -1;
                return false;
            }
        }

        public static bool TryParseWithDot(this string str, out float value) =>
            float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        public static string ToDotString(this float value) =>
            value.ToString(CultureInfo.InvariantCulture);

        public static IEnumerator WriteLog(this DARequest request, string text, string add = null)
        {
            FileInfo[] fileInfos = new DirectoryInfo(FcuConfig.LogPath).GetFiles($"*.*");

            if (fileInfos.Length >= FcuConfig.Instance.LogFilesLimit)
            {
                foreach (FileInfo file in fileInfos)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {

                    }
                }
            }

            string logFileName = $"{DateTime.Now.ToString(FcuConfig.Instance.DateTimeFormat1)}_{add}{FcuConfig.Instance.WebLogFileName}";
            string logFilePath = Path.Combine(FcuConfig.LogPath, logFileName);

            string result;

            JFResult jfr = DAFormatter.Format<string>(text);

            if (jfr.IsValid)
            {
                result = jfr.Json;
            }
            else
            {
                result = text;
            }

            result = $"{request.Query}\n{result}";

            File.WriteAllText(logFilePath, result);

            yield return null;
        }

        public static bool IsProjectEmpty(this SelectableFObject sf)
        {
            if (sf == null)
                return true;

            if (sf.Id.IsEmpty())
                return true;

            return false;
        }

        public static string GetImageFormat(this ImageFormat imageFormat) =>
            imageFormat.ToString().ToLower();

        public static bool IsNova(this FigmaConverterUnity fcu) => fcu.Settings.MainSettings.UIFramework == UIFramework.NOVA;
        public static bool IsUITK(this FigmaConverterUnity fcu) => fcu.Settings.MainSettings.UIFramework == UIFramework.UITK;
        public static bool IsUGUI(this FigmaConverterUnity fcu) => fcu.Settings.MainSettings.UIFramework == UIFramework.UGUI;

        public static bool IsDebug(this FigmaConverterUnity fcu) => FcuConfig.Instance.DebugSettings.DebugMode;
    }
}