using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

#if JOSH_PUI_EXISTS
using UnityEngine.UI.ProceduralImage;
#endif

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class JoshPuiDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, Sprite sprite, GameObject target)
        {
#if JOSH_PUI_EXISTS
            target.TryAddGraphic(out ProceduralImage img);

            img.sprite = sprite;
            img.type = monoBeh.Settings.JoshPuiSettings.Type;
            img.raycastTarget = monoBeh.Settings.JoshPuiSettings.RaycastTarget;
            img.preserveAspect = monoBeh.Settings.JoshPuiSettings.PreserveAspect;
            img.FalloffDistance = monoBeh.Settings.JoshPuiSettings.FalloffDistance;
#if UNITY_2020_1_OR_NEWER
            img.raycastPadding = monoBeh.Settings.JoshPuiSettings.RaycastPadding;
#endif
            if (fobject.Type == NodeType.ELLIPSE)
            {
                target.TryAddComponent(out RoundModifier roundModifier);
            }
            else
            {
                if (fobject.CornerRadiuses != null)
                {
                    target.TryAddComponent(out FreeModifier freeModifier);
                    freeModifier.Radius = monoBeh.GraphicHelpers.GetCornerRadius(fobject);
                }
                else
                {
                    target.TryAddComponent(out UniformModifier uniformModifier);
                    uniformModifier.Radius = fobject.CornerRadius.ToFloat();
                }
            }

            SetColor(fobject, img);
#endif
        }

        public void SetProceduralColor(FObject fobject, Image img, Action setStrokeOnlyWidth, Action setStroke)
        {
            FGraphic graphic = fobject.Data.Graphic;

            FcuLogger.Debug($"SetUnityImageColor | {fobject.Data.NameHierarchy} | {fobject.Data.FcuImageType} | hasFills: {graphic.HasFill} | hasStroke: {graphic.HasStroke}", FcuLogType.ComponentDrawer);

            bool strokeOnly = graphic.HasStroke && !graphic.HasFill;

            if (strokeOnly)
            {
                setStrokeOnlyWidth();

                if (graphic.Stroke.HasSolid)
                {
                    img.color = graphic.Stroke.SolidPaint.Color;   
                }
                else if (graphic.Stroke.HasGradient)
                {
                    img.color = Color.white;
                    monoBeh.CanvasDrawer.ImageDrawer.AddGradient(fobject, graphic.Stroke.GradientPaint);
                }
            }
            else
            {
                if (graphic.Fill.HasSolid)
                {
                    img.color = graphic.Fill.SolidPaint.Color;
                }
                else if (graphic.Fill.HasGradient)
                {
                    img.color = Color.white;
                    monoBeh.CanvasDrawer.ImageDrawer.AddGradient(fobject, graphic.Fill.GradientPaint);
                }

                if (graphic.HasStroke)
                {
                    setStroke();
                }
            }

            if (!graphic.HasStroke)
            {
                fobject.Data.GameObject.TryDestroyComponent<UnityEngine.UI.Outline>();
            }
        }

#if JOSH_PUI_EXISTS
        public void SetColor(FObject fobject, ProceduralImage img)
        {
            FGraphic graphic = fobject.Data.Graphic;

            FcuLogger.Debug($"SetUnityImageColor | {fobject.Data.NameHierarchy} | {fobject.Data.FcuImageType} | hasFills: {graphic.HasFill} | hasStroke: {graphic.HasStroke}", FcuLogType.ComponentDrawer);

            if (fobject.IsDrawableType())
            {
                SetProceduralColor(fobject, img,
                setStrokeOnlyWidth: () =>
                {
                    img.BorderWidth = fobject.StrokeWeight;
                },
                setStroke: () =>
                {
                    switch (graphic.Stroke.Align)
                    {
                        case StrokeAlign.OUTSIDE:
                            {
                                monoBeh.CanvasDrawer.ImageDrawer.AddUnityOutline(fobject);
                            }
                            break;
                        default:
                            {
                                fobject.Data.GameObject.TryDestroyComponent<UnityEngine.UI.Outline>();
                                break;
                            }
                    }
                });
            }
            else
            {
                monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.SetColor(fobject, img);
            }
        }
#endif
    }
}
