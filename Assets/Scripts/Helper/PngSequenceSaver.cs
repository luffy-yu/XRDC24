using System;
using System.IO;
using UnityEngine;

namespace XRDC24.Helper
{
    public class PngSequenceSaver : MonoBehaviour
    {
        [Header("Camera")] private Camera camera;

        public bool enableAlpha = false;

        public RenderTexture renderTexture;

        public int getFrameEveryMS = 100;
        private float lastUpdateTime = 0.0f;

        // timestamp for last frame
        private long lastFrameTimestampMS = 0;

        [HideInInspector] public bool collecting = false;

        [Space(30)] [Header("Naming")] public string datasetFolder = "Images";

        private string directory;

        private void Awake()
        {
            directory = GetImageDirectory();

            camera = gameObject.GetComponent<Camera>();
            // change it to renderTexture when starting
            camera.targetTexture = renderTexture;
            
            // Set the camera's clear flags to SolidColor
            camera.clearFlags = CameraClearFlags.SolidColor;

            // Set the background color to transparent (supports alpha if enableAlpha is true)
            camera.backgroundColor = enableAlpha ? new Color(0, 0, 0, 0) : Color.black;
        }

        #region Image capturing

        // refer to: https://gist.github.com/krzys-h/76c518be0516fb1e94c7efbdcd028830
        // fix the issue: SRGB to Linear color space (image is very dark compared)
        private Texture2D TakeSnapshot()
        {
            var mWidth = camera.targetTexture.width;
            var mHeight = camera.targetTexture.height;

            RenderTexture rt = new RenderTexture(mWidth, mHeight, 24, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            RenderTexture oldRT = camera.targetTexture;
            camera.targetTexture = rt;
            camera.Render();
            camera.targetTexture = oldRT;

            RenderTexture.active = rt;
            // TextureFormat.RGBA32 to enable alpha
            var format = TextureFormat.RGB24;

            if (enableAlpha)
            {
                format = TextureFormat.RGBA32;
            }

            Texture2D tex = new Texture2D(rt.width, rt.height, format, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            DestroyImmediate(rt);

            return tex;
        }

        internal string GetImageDirectory()
        {
            var dir = Directory.CreateDirectory(Path.Combine(Application.dataPath, $"../{datasetFolder}")).FullName;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }

        public FileInfo SavePNG(byte[] bytes)
        {
            var filename = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff") + ".png";
            string filepath = Path.Combine(directory, filename);

            File.WriteAllBytes(filepath, bytes);

            return new FileInfo(filepath);
        }


        #endregion

        private long GetTimeStamp()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                collecting = !collecting;
                Debug.LogWarning($"Collecting status: {collecting}");
            }

            if (collecting)
            {
                var timestamp = GetTimeStamp();
                if (timestamp - lastFrameTimestampMS > getFrameEveryMS)
                {
                    lastFrameTimestampMS = timestamp;
                    TakePhoto();
                }
            }
        }

        public void TakePhoto()
        {
            Texture2D image = TakeSnapshot();
            var bytes = image.EncodeToPNG();
            // destroy to avoid memory leak
            DestroyImmediate(image);

            var fileInfo = SavePNG(bytes);
            print($"Save image to: {fileInfo.FullName}");
        }
    }
}