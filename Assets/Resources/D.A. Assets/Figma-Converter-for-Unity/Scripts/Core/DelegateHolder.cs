using DA_Assets.Shared;
using System;
using UnityEngine;

namespace DA_Assets.FCU
{
    public delegate void ShowDifferenceChecker(PreImportInput data, Action<PreImportOutput> callback);
    public delegate bool GetGameViewSize(out Vector2 size);
    public delegate void Void();
    // Add by Liuchuan Yu to fix `UpdateScrollContent` not working
    public delegate void UpdateScrollContentDelegate();

    [Serializable]
    public class DelegateHolder : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public ShowDifferenceChecker ShowDifferenceChecker { get; set; }
        public GetGameViewSize GetGameViewSize { get; set; }
        public Void UpdateScrollContent { get; set; }
        public Func<Vector2, bool> SetGameViewSize { get; set; }
    }
}