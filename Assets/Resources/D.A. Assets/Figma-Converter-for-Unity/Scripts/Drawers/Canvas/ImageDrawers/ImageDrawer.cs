using DA_Assets.DAG;
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class ImageDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, GameObject customGameObject = null)
        {
            GameObject target = customGameObject == null ? fobject.Data.GameObject : customGameObject;

            if (fobject.Data.GameObject.IsPartOfAnyPrefab() == false)
            {
                if (target.TryGetComponentSafe(out Graphic oldGraphic))
                {
                    Type curType = monoBeh.GetCurrentImageType();

                    if (oldGraphic.GetType().Equals(curType) == false)
                    {
                        oldGraphic.RemoveComponentsDependingOn();
                        oldGraphic.Destroy();
                    }
                }
            }

            Sprite sprite = monoBeh.SpriteWorker.GetSprite(fobject);

            if (sprite == null)
            {
                if (fobject.IsSingleImageOrVideoType() || fobject.IsSprite())
                {
                    sprite = FcuConfig.Instance.MissingImageTexture128px;
                }
            }

            if (monoBeh.IsNova())
            {
#if NOVA_UI_EXISTS
                this.NovaImageDrawer.Draw(fobject, sprite, target);
#endif
            }
            else if (monoBeh.UsingSpriteRenderer())
            {
                this.SpriteRendererDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingUnityImage() || monoBeh.UsingRawImage() || fobject.IsObjectMask() || fobject.CanUseUnityImageWhenUsingAnyProceduralImage(monoBeh))
            {
                this.UnityImageDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingShapes2D())
            {
                this.Shapes2DDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingJoshPui())
            {
                this.JoshPuiDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingDttPui())
            {
                this.DttPuiDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingMPUIKit())
            {
                this.MPUIKitDrawer.Draw(fobject, sprite, target);
            }
        }

        public void AddUnityOutline(FObject fobject)
        {
            FGraphic graphic = fobject.Data.Graphic;

            fobject.Data.GameObject.TryAddComponent(out UnityEngine.UI.Outline outline);
            outline.useGraphicAlpha = false;
            outline.effectDistance = new Vector2(fobject.StrokeWeight, -fobject.StrokeWeight);

            if (graphic.Stroke.HasSolid)
            {
                outline.effectColor = graphic.Stroke.SolidPaint.Color;
            }
            else if (graphic.Stroke.HasGradient)
            {
                outline.effectColor = graphic.Stroke.SingleColor;
            }
            else
            {
                outline.effectColor = default;
            }
        }

        public void AddGradient(FObject fobject, Paint gradientColor, bool strokeOnly = false)
        {
            GameObject gameObject = fobject.Data.GameObject;
            List<GradientColorKey> gradientColorKeys = gradientColor.ToGradientColorKeys();
            List<GradientAlphaKey> gradientAlphaKeys = gradientColor.ToGradientAlphaKeys();

            float angle;

            switch (gradientColor.Type)
            {
                case PaintType.GRADIENT_RADIAL:
                    angle = monoBeh.GraphicHelpers.ToRadialAngle(fobject, gradientColor.GradientHandlePositions);
                    break;
                default:
                    angle = monoBeh.GraphicHelpers.ToLinearAngle(fobject, gradientColor.GradientHandlePositions);
                    break;
            }

            if (monoBeh.UsingShapes2D() && !strokeOnly)
            {
                AddDAGradient();
            }
            else if (monoBeh.UsingMPUIKit())
            {
#if MPUIKIT_EXISTS
                if (gameObject.TryGetComponentSafe(out MPUIKIT.MPImage img))
                {
                    Gradient gradient = new Gradient
                    {
                        mode = GradientMode.Blend,
                    };

                    MPUIKIT.GradientEffect ge = new MPUIKIT.GradientEffect();
                    ge.Enabled = true;

                    switch (gradientColor.Type)
                    {
                        case PaintType.GRADIENT_RADIAL:
                            ge.GradientType = MPUIKIT.GradientType.Radial;
                            break;
                        default:
                            ge.GradientType = MPUIKIT.GradientType.Linear;
                            break;
                    }

                    ge.Gradient = gradient;
                    ge.Rotation = angle;
                    img.GradientEffect = ge;

                    gradient.colorKeys = gradientColorKeys.ToArray();
                    gradient.alphaKeys = gradientAlphaKeys.ToArray();
                }
                else
                {
                    AddDAGradient();
                }
#endif
            }
            else if (monoBeh.UsingDttPui())
            {
#if PROCEDURAL_UI_ASSET_STORE_RELEASE
                if (gameObject.TryGetComponentSafe(out DTT.UI.ProceduralUI.RoundedImage roundedImage))
                {
                    gameObject.TryAddComponent(out DTT.UI.ProceduralUI.GradientEffect gradient);

                    Gradient newGradient = new Gradient();
                    newGradient.colorKeys = gradientColorKeys.ToArray();
                    newGradient.alphaKeys = gradientAlphaKeys.ToArray();

                    Type objectType = gradient.GetType();
                    System.Reflection.FieldInfo fieldInfo = objectType.GetField("_gradient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(gradient, newGradient);
                    }

                    System.Reflection.FieldInfo fieldInfo1 = objectType.GetField("_type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (fieldInfo1 != null)
                    {
                        DTT.UI.ProceduralUI.GradientEffect.GradientType gt;

                        /// At the time of writing this code in the <see cref="DTT.UI.ProceduralUI"/> asset, 
                        /// the enums <see cref="DTT.UI.ProceduralUI.GradientEffect.GradientType.RADIAL"/> 
                        /// and <see cref="DTT.UI.ProceduralUI.GradientEffect.GradientType.ANGULAR"/> were swapped.
                        switch (gradientColor.Type)
                        {
                            case PaintType.GRADIENT_RADIAL:
                                gt = DTT.UI.ProceduralUI.GradientEffect.GradientType.ANGULAR;
                                break;
                            case PaintType.GRADIENT_ANGULAR:
                                gt = DTT.UI.ProceduralUI.GradientEffect.GradientType.RADIAL;
                                break;
                            default:
                                gt = DTT.UI.ProceduralUI.GradientEffect.GradientType.LINEAR;
                                break;
                        }

                        fieldInfo1.SetValue(gradient, gt);
                    }

                    System.Reflection.FieldInfo fieldInfo3 = objectType.GetField("_rotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (fieldInfo3 != null)
                    {
                        fieldInfo3.SetValue(gradient, angle);
                    }
                }
                else
                {
                    AddDAGradient();
                }
#endif
            }
            else
            {
                AddDAGradient();
            }

            void AddDAGradient()
            {
                gameObject.TryAddComponent(out DAGradient gradient);

                gradient.Angle = angle;
                gradient.BlendMode = DAColorBlendMode.Multiply;

                gradient.Gradient.colorKeys = gradientColorKeys.ToArray();
                gradient.Gradient.alphaKeys = gradientAlphaKeys.ToArray();
            }
        }

        public bool TryAddCornerRounder(FObject fobject, GameObject target)
        {
            if (fobject.IsSprite())
            {
                return false;
            }

            if (fobject.ContainsRoundedCorners())
            {
                target.TryAddComponent(out CornerRounder cornerRounder);
                Vector4 cr = monoBeh.GraphicHelpers.GetCornerRadius(fobject);
                cornerRounder.SetRadii(cr);
            }

            return false;
        }

        [SerializeField] UnityImageDrawer unityImageDrawer;
        [SerializeProperty(nameof(unityImageDrawer))]
        public UnityImageDrawer UnityImageDrawer => unityImageDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] Shapes2DDrawer shapes2DDrawer;
        [SerializeProperty(nameof(shapes2DDrawer))]
        public Shapes2DDrawer Shapes2DDrawer => shapes2DDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] JoshPuiDrawer joshPuiDrawer;
        [SerializeProperty(nameof(joshPuiDrawer))]
        public JoshPuiDrawer JoshPuiDrawer => joshPuiDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] DttPuiDrawer dttPuiDrawer;
        [SerializeProperty(nameof(dttPuiDrawer))]
        public DttPuiDrawer DttPuiDrawer => dttPuiDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] MPUIKitDrawer mpuikitDrawer;
        [SerializeProperty(nameof(mpuikitDrawer))]
        public MPUIKitDrawer MPUIKitDrawer => mpuikitDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] SpriteRendererDrawer spriteRendererDrawer;
        [SerializeProperty(nameof(spriteRendererDrawer))]
        public SpriteRendererDrawer SpriteRendererDrawer => spriteRendererDrawer.SetMonoBehaviour(monoBeh);

#if NOVA_UI_EXISTS
        [SerializeField] NovaImageDrawer novaImageDrawer;
        [SerializeProperty(nameof(novaImageDrawer))]
        public NovaImageDrawer NovaImageDrawer => novaImageDrawer.SetMonoBehaviour(monoBeh);
#endif
    }
}