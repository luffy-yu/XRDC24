using UnityEngine;

namespace XRDC24.Helper
{
    public class DisableErrorLogging: MonoBehaviour
    {
        void Awake()
        {
            Application.logMessageReceived += SuppressLogs;
        }

        private void SuppressLogs(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                return;
            }
            
            Debug.Log(condition);
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= SuppressLogs;
        }
    }
}