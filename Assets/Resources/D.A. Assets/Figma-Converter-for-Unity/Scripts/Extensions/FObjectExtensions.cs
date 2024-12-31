using DA_Assets.FCU.Model;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace DA_Assets.FCU.Extensions
{
    public static class FObjectExtensions
    {
        public static void SetData(this FObject fobject, SyncHelper syncHelper, FigmaConverterUnity fcu)
        {
            fobject.Data.FigmaConverterUnity = fcu;
            fobject.Data.GameObject = syncHelper.gameObject;
            syncHelper.Data = fobject.Data;
        }

        public static void ExecuteWithTemporaryParent(this FObject fobject, Transform tempChildsParent, Func<FObject, GameObject> targetSelector, Action action)
        {
            GameObject target = targetSelector(fobject);
            List<Transform> children = new List<Transform>();
            List<int> siblingIndices = new List<int>();

            foreach (Transform child in target.transform)
            {
                children.Add(child);
                siblingIndices.Add(child.GetSiblingIndex());
            }

            foreach (Transform child in children)
            {
                child.SetParent(tempChildsParent);
            }

            action.Invoke();

            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetParent(target.transform);
                children[i].SetSiblingIndex(siblingIndices[i]);
            }
        }

        public static bool IsSprite(this SyncData data)
        {
            return data.FcuImageType == FcuImageType.Downloadable || data.FcuImageType == FcuImageType.Generative;
        }

        public static bool IsSprite(this FObject fobject)
        {
            return fobject.Data.FcuImageType == FcuImageType.Downloadable || fobject.Data.FcuImageType == FcuImageType.Generative;
        }

        public static bool IsCircle(this FObject fobject)
        {
            if (fobject.Type != NodeType.ELLIPSE)
                return false;

            if (fobject.Size.x.Round(1) == fobject.Size.y.Round(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsRectangle(this FObject fobject)
        {
            if (fobject.Type == NodeType.RECTANGLE)
                return true;

            if (fobject.Type != NodeType.FRAME)
                return false;

            if (fobject.Type == NodeType.LINE && fobject.IsSupportedLine())
                return false;

            if (!fobject.Children.IsEmpty())
                return false;

            return true;
        }

        public static void SetFlagToAllChilds(this FObject parent, Action<FObject> action)
        {
            if (parent.IsDefault() || parent.Children.IsEmpty())
                return;

            foreach (FObject child in parent.Children)
            {
                action(child);
                SetFlagToAllChilds(child, action);
            }
        }

        public static List<GradientAlphaKey> ToGradientAlphaKeys(this Paint gradient)
        {
            List<GradientAlphaKey> gradientColorKeys = new List<GradientAlphaKey>();

            if (gradient.GradientStops.IsEmpty())
            {
                return gradientColorKeys;
            }

            foreach (GradientStop gradientStop in gradient.GradientStops)
            {
                gradientColorKeys.Add(new GradientAlphaKey
                {
                    alpha = gradientStop.Color.a,
                    time = gradientStop.Position
                });
            }

            return gradientColorKeys;
        }

        public static List<GradientColorKey> ToGradientColorKeys(this Paint gradient)
        {
            List<GradientColorKey> gradientColorKeys = new List<GradientColorKey>();

            if (gradient.GradientStops.IsEmpty())
            {
                return gradientColorKeys;
            }

            foreach (GradientStop gradientStop in gradient.GradientStops)
            {
                gradientColorKeys.Add(new GradientColorKey
                {
                    color = gradientStop.Color,
                    time = gradientStop.Position
                });
            }

            return gradientColorKeys;
        }

        public static string GetText(this FObject fobject)
        {
            return fobject.Characters.Replace("\\r", " ").Replace("\\n", Environment.NewLine);
        }

        public static bool IsInsideDownloadable(this FObject fobject, FigmaConverterUnity fcu, out FObject downloadableFObject)
        {
            downloadableFObject = default;

            bool get = fcu.CurrentProject.TryGetByIndex(fobject.Data.ParentIndex, out FObject parent);

            if (get == false)
                return false;

            if (fcu.ImageTypeSetter.DownloadableIds.Contains(parent.Id))
            {
                downloadableFObject = fobject;
                return true;
            }

            return IsInsideDownloadable(parent, fcu, out downloadableFObject);
        }

        public static bool IsSupportedLine(this FObject fobject)
        {
            if (fobject.StrokeCap == StrokeCap.SQUARE)
                return true;

            if (fobject.StrokeCap == StrokeCap.ROUND && fobject.StrokeWeight >= 2f)
                return true;

            return false;
        }

        public static bool HasActiveProperty<T>(this FObject fobject, Expression<Func<FObject, IEnumerable<T>>> propertySelector) where T : IVisible
        {
            var func = propertySelector.Compile();
            IEnumerable<T> collection = func(fobject);
            return !collection.IsEmpty() && collection.Any(item => item.Visible.ToBoolNullTrue());
        }

        public static bool IsSupportedRenderSize(this Vector2 sourceSize, FigmaConverterUnity fcu, out Vector2Int spriteSize, out Vector2Int renderSize)
        {
            spriteSize = (sourceSize * fcu.Settings.MainSettings.ImageScale).ToVector2Int();

            int maxRenderSize = FcuConfig.Instance.MaxRenderSize;
            int renderUpscaleFactor = FcuConfig.Instance.RenderUpscaleFactor;

            renderSize = spriteSize * renderUpscaleFactor;

            if (renderSize.x <= maxRenderSize && renderSize.y <= maxRenderSize)
            {
                return true;
            }

            return false;
        }

        public static bool CanUseUnityImageWhenUsingAnyProceduralImage(this FObject fobject, FigmaConverterUnity fcu)
        {
            if (!fcu.UsingAnyProceduralImage())
                return false;

            if (fcu.Settings.ComponentSettings.UseUnityImageWhen.HasFlag(UseUnityImageWhen.Sprite))
            {
                if (fobject.IsDownloadableType())
                {
                    return true;
                }
            }

            if (fcu.Settings.ComponentSettings.UseUnityImageWhen.HasFlag(UseUnityImageWhen.RectangleNoRoundedCorners))
            {
                bool b1 = fobject.Type == NodeType.RECTANGLE || fobject.Type == NodeType.FRAME;
                bool b2 = !fobject.ContainsRoundedCorners();
                bool b3 = !fcu.GraphicHelpers.IsDownloadableByFills(fobject, out string _reason).ToBoolNullFalse();
                bool b4 = !fobject.Data.Graphic.HasStroke;

                FcuLogger.Debug($"{nameof(CanUseUnityImageWhenUsingAnyProceduralImage)} | {fobject.Data.NameHierarchy} | {b1} | {b2} | {b3} | {b4}");

                return b1 && b2 && b3 && b4;
            }

            return false;
        }

        public static bool TryGetLocalPosition(this FObject fobject, out Vector3 rtPos)
        {
            try
            {
                rtPos = new Vector3(fobject.RelativeTransform[0][2].ToFloat(), -fobject.RelativeTransform[1][2].ToFloat(), 0);
                return true;
            }
            catch
            {
                rtPos = new Vector3(0, 0, 0);
                return false;
            }
        }
    }
}
