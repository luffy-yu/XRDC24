using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if NOVA_UI_EXISTS
using Nova;
#endif

namespace DA_Assets.FCU
{
    [Serializable]
    public class TransformSetter : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator SetTransformPos(List<FObject> fobjects)
        {
            DALogger.Log(FcuLocKey.log_start_setting_transform.Localize());

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.RectGameObject == null)
                    continue;

                fobject.Data.Angle = fobject.GetFigmaRotationAngle();
            }

            Transform tempParent = MonoBehExtensions.CreateEmptyGameObject(nameof(tempParent), monoBeh.transform).transform;

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.RectGameObject == null)
                    continue;

                RectTransform rt = fobject.Data.RectGameObject.GetComponent<RectTransform>();
                rt.SetSmartAnchor(AnchorType.TopLeft);
                rt.SetSmartPivot(PivotType.TopLeft);

                Rect rect = GetGlobalRect(fobject);
                fobject.Data.Size = rect.size;
                fobject.Data.Position = rect.position;

                rt.sizeDelta = rect.size;

                fobject.ExecuteWithTemporaryParent(tempParent, x => x.Data.RectGameObject, () =>
                {
                    rt.position = rect.position;
                });


                fobject.Data.RectGameObject.transform.localScale = Vector3.one;
            }

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.RectGameObject == null)
                    continue;

                RectTransform rt = fobject.Data.RectGameObject.GetComponent<RectTransform>();

                fobject.ExecuteWithTemporaryParent(tempParent, x => x.Data.RectGameObject, () =>
                {
                    rt.SetSmartPivot(PivotType.MiddleCenter);
                    SetFigmaRotation(fobject, fobject.Data.RectGameObject);
                });
            }

            tempParent.gameObject.Destroy();

            yield return null;
        }

        public IEnumerator SetTransformPosAndAnchors(List<FObject> fobjects)
        {
            DALogger.Log(FcuLocKey.log_start_setting_transform.Localize());

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.RectGameObject == null)
                    continue;

                if (!fobject.IsInsideAutoLayout(out FObject _, out var __, monoBeh))
                    continue;

                RectTransform rt = fobject.Data.RectGameObject.GetComponent<RectTransform>();
                Rect rect = GetAutolayoutRect(fobject);
                fobject.Data.Size = rect.size;
                rt.sizeDelta = fobject.Data.Size;
            }

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.RectGameObject == null)
                    continue;

                RectTransform rt = fobject.Data.RectGameObject.GetComponent<RectTransform>();

                rt.SetSmartPivot(monoBeh.Settings.MainSettings.PivotType);

                if (!fobject.ContainsTag(FcuTag.Frame) && !fobject.Data.Parent.ContainsTag(FcuTag.AutoLayoutGroup))
                {
                    rt.SetSmartAnchor(fobject.GetFigmaAnchor());
                }
            }

            yield return SetRootFramesPosition(fobjects);

            yield return monoBeh.ReEnableRectTransform();
        }

        public Rect GetAutolayoutRect(FObject fobject)
        {
            Rect rect = new Rect();
            Vector2 position = new Vector2();
            Vector2 size = new Vector2();

            size = fobject.Data.Size;

            if (fobject.TryFixSizeWithStroke(size.y, out float newY))
            {
                size.y = newY;
            }

            FcuLogger.Debug($"{nameof(GetAutolayoutRect)} | {fobject.Data.NameHierarchy} | {size} | {position}", FcuLogType.Transform);

            rect.size = size;
            rect.position = position;

            return rect;
        }

        public Rect GetGlobalRect(FObject fobject)
        {
            Rect rect = new Rect();
            Vector2 position = new Vector2();
            Vector2 size = new Vector2();

            bool hasBoundingSize = fobject.GetBoundingSize(out Vector2 bSize);
            bool hasBoundingPos = fobject.GetBoundingPosition(out Vector2 bPos);

            bool hasRenderSize = fobject.GetRenderSize(out Vector2 rSize);
            bool hasRenderPos = fobject.GetRenderPosition(out Vector2 rPos);

            bool hasLocalPos = fobject.TryGetLocalPosition(out Vector3 lPos);

            float scale = 1;
            bool hasScaleInName = fobject.IsDownloadableType() && fobject.Data.SpritePath.TryParseSpriteName(out scale, out var _);

            int state = 0;

            if (hasScaleInName)
            {
                if (hasRenderPos)
                {
                    state = 1;

                    position.x = rPos.x;
                    position.y = monoBeh.IsUGUI() || monoBeh.IsNova() ? -rPos.y : rPos.y;
                }
                else
                {
                    state = 2;

                    position.x = bPos.x;
                    position.y = monoBeh.IsUGUI() || monoBeh.IsNova() ? -bPos.y : bPos.y;
                }

                size.x = fobject.Data.SpriteSize.x / scale;
                size.y = fobject.Data.SpriteSize.y / scale;
            }
            else if (fobject.IsGenerativeType() || fobject.IsDrawableType())
            {
                state = 3;
                size = fobject.Size;

                position.x = bPos.x;
                position.y = monoBeh.IsUGUI() || monoBeh.IsNova() ? -bPos.y : bPos.y;
            }
            else
            {
                state = 4;
                size.x = bSize.x;
                size.y = bSize.y;

                position.x = bPos.x;
                position.y = monoBeh.IsUGUI() || monoBeh.IsNova() ? -bPos.y : bPos.y;
            }

            if (fobject.TryFixSizeWithStroke(size.y, out float newY))
            {
                size.y = newY;
            }

            FcuLogger.Debug($"{nameof(GetGlobalRect)} | {fobject.Data.NameHierarchy} | state: {state} | {size} | {position}", FcuLogType.Transform);

            rect.size = size;
            rect.position = position;

            return rect;
        }

        private IEnumerator SetRootFramesPosition(List<FObject> fobjects)
        {
            IEnumerable<FrameGroup> fobjectsByFrame = fobjects
                .GroupBy(x => x.Data.RootFrame)
                .Select(g => new FrameGroup
                {
                    Childs = g.Select(x => x).ToList(),
                    RootFrame = g.First()
                });

            if (monoBeh.Settings.MainSettings.PositioningMode == PositioningMode.Absolute)
            {
                foreach (FrameGroup rootFrame in fobjectsByFrame)
                {
                    if (rootFrame.RootFrame.Data.RectGameObject == null)
                        continue;

                    RectTransform rt = rootFrame.RootFrame.Data.RectGameObject.GetComponent<RectTransform>();
                    rt.SetSmartAnchor(AnchorType.TopLeft);
                }
            }
            else
            {
                yield return monoBeh.AssetTools.ReselectFcu();
                monoBeh.AssetTools.CacheResolutionData();

                foreach (FrameGroup rootFrame in fobjectsByFrame)
                {
                    if (rootFrame.RootFrame.Data.RectGameObject == null)
                        continue;

                    yield return WaitFor.Delay001();
                    monoBeh.DelegateHolder.SetGameViewSize(rootFrame.RootFrame.Size);
                    yield return WaitFor.Delay01();

                    RectTransform rt = rootFrame.RootFrame.Data.RectGameObject.GetComponent<RectTransform>();
                    rt.SetSmartAnchor(AnchorType.StretchAll);
                    rt.offsetMin = new Vector2(0, 0);
                    rt.offsetMax = new Vector2(0, 0);
                    rt.localScale = Vector3.one;
                }

                yield return monoBeh.AssetTools.ReselectFcu();
                monoBeh.AssetTools.RestoreResolutionData();
            }
        }

        internal IEnumerator MoveUguiTransforms(List<FObject> currPage)
        {
           // yield break;
            foreach (FObject fobject in currPage)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                if (fobject.Data.RectGameObject == null)
                    continue;

                fobject.Data.GameObject.TryAddComponent(out RectTransform goRt);
                fobject.Data.RectGameObject.TryGetComponentSafe(out RectTransform rectRt);

                goRt.CopyFrom(rectRt);
            }

            yield return null;
        }

        internal void MoveNovaTransforms(List<FObject> currPage)
        {
            Transform tempParent = MonoBehExtensions.CreateEmptyGameObject(nameof(tempParent), monoBeh.transform).transform;

            foreach (FObject fobject in currPage)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                if (fobject.Data.RectGameObject == null)
                    continue;

                fobject.Data.RectGameObject.TryGetComponentSafe(out RectTransform rectRt);
                fobject.Data.UguiTransformData = UguiTransformData.Create(rectRt);

#if NOVA_UI_EXISTS
                if (fobject.ContainsTag(FcuTag.Text))
                {
                    fobject.Data.GameObject.TryAddComponent(out TextBlock textBlock);
                }
                else
                {
                    fobject.Data.GameObject.TryAddComponent(out UIBlock2D uiBlock2d);
                }

                UIBlock uiBlock = fobject.Data.GameObject.GetComponent<UIBlock>();
                uiBlock.Color = default;

                uiBlock.Layout.Size = new Length3
                {
                    X = fobject.Data.Size.x,
                    Y = fobject.Data.Size.y,
                };

                fobject.ExecuteWithTemporaryParent(tempParent, x => x.Data.GameObject, () =>
                {
                    SetFigmaRotation(fobject, fobject.Data.GameObject);
                });

                uiBlock.Layout.Position = new Length3
                {
                    X = fobject.Data.UguiTransformData.LocalPosition.x,
                    Y = fobject.Data.UguiTransformData.LocalPosition.y,
                };
#endif
            }

            tempParent.gameObject.Destroy();
        }

        public IEnumerator SetNovaAnchors(List<FObject> fobjects)
        {
            int total = fobjects.Count;
            int processed = 0;

            IEnumerable<FrameGroup> fobjectsByFrame = fobjects
                .GroupBy(x => x.Data.RootFrame)
                .Select(g => new FrameGroup
                {
                    Childs = g.Select(x => x).ToList(),
                    RootFrame = g.First()
                });

            foreach (FrameGroup rootFrame in fobjectsByFrame)
            {
                if (rootFrame.RootFrame.Data.RectGameObject == null)
                    continue;

                SetNovaAnchorsRoutine(rootFrame.Childs, () => processed++).StartDARoutine(monoBeh);
            }

            int tempCount = -1;
            while (FcuLogger.WriteLogBeforeEqual(
                ref processed,
                ref total,
                FcuLocKey.log_set_anchors.Localize(processed, total),
                ref tempCount))
            {
                yield return WaitFor.Delay1();
            }
        }

        private IEnumerator SetNovaAnchorsRoutine(List<FObject> fobjects, Action onProcess)
        {
#if NOVA_UI_EXISTS
            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                fobject.Data.GameObject.TryGetComponentSafe(out UIBlock uiBlock);
                yield return uiBlock.SetNovaAnchor(fobject.GetFigmaAnchor());

                onProcess.Invoke();
            }
#endif

            yield return null;
        }

        internal IEnumerator RestoreNovaFramePositions(List<FObject> fobjects)
        {
            IEnumerable<FrameGroup> fobjectsByFrame = fobjects
                .GroupBy(x => x.Data.RootFrame)
                .Select(g => new FrameGroup
                {
                    Childs = g.Select(x => x).ToList(),
                    RootFrame = g.First()
                });

            foreach (FrameGroup rootFrame in fobjectsByFrame)
            {
                if (rootFrame.RootFrame.Data.GameObject == null)
                    continue;

#if NOVA_UI_EXISTS
                rootFrame.RootFrame.Data.GameObject.TryGetComponentSafe(out UIBlock uiBlock);

                yield return uiBlock.SetNovaAnchor(AnchorType.TopLeft);
#endif
                yield return WaitFor.Delay01();

#if NOVA_UI_EXISTS
                uiBlock.Layout.Position = new Length3
                {
                    X = rootFrame.RootFrame.AbsoluteBoundingBox.X.ToFloat(),
                    Y = rootFrame.RootFrame.AbsoluteBoundingBox.Y.ToFloat(),
                };
#endif
            }
        }

        public void SetFigmaRotation(FObject fobject, GameObject target)
        {
            if (IsNeedRotate(fobject))
            {
                float rotationAngle;

                Transform rect = target.GetComponent<Transform>();

                if (IsNeedRotate(fobject))
                {
                    rotationAngle = fobject.Data.Angle;
                }
                else
                {
                    rotationAngle = 0;
                }

                rect.SetRotation(rotationAngle);
            }
        }

        public bool IsNeedRotate(FObject fobject)
        {
            float angle = fobject.Data.Angle;

            bool? needRotate = null;

            if (fobject.ContainsTag(FcuTag.Text) && angle != 0)
            {
                needRotate = true;
            }
            else if (fobject.Data.ChildIndexes.IsEmpty())
            {
                if (fobject.GetRenderSize(out Vector2 size))
                {
                    //No need rotation for squares/circles.
                    if (size.x == size.y)
                    {
                        needRotate = false;
                    }
                    //No need rotation - at 0 and 180 degrees a shape without children looks the same.
                    else if (angle == 180f)
                    {
                        needRotate = false;
                    }
                }
            }

            if (needRotate == null)
            {
                switch (fobject.Data.FcuImageType)
                {
                    case FcuImageType.Downloadable:
                        {
                            needRotate = false;
                        }
                        break;
                    default:
                        {
                            if (fobject.Type == NodeType.GROUP)
                            {
                                bool hasrot = false;
                                foreach (var cindex in fobject.Data.ChildIndexes)
                                {
                                    if (monoBeh.CurrentProject.TryGetByIndex(cindex, out FObject child))
                                    {
                                        float a = child.Data.Angle;

                                        if (a != angle)
                                        {
                                            hasrot = true;
                                            break;
                                        }
                                    }
                                }

                                if (!hasrot)
                                {
                                    needRotate = false;
                                }
                                else
                                {
                                    needRotate = true;
                                }

                            }
                            else
                            {
                                needRotate = angle != 0;
                            }

                        }
                        break;
                }
            }

            FcuLogger.Debug($"IsNeedRotate | {fobject.Data.NameHierarchy} | needRotate: {needRotate} | angle: {angle} | {fobject.Data.FcuImageType}", FcuLogType.Transform);
            return needRotate.ToBoolNullFalse();
        }
    }


    public struct FrameGroup
    {
        public FObject RootFrame { get; set; }
        public List<FObject> Childs { get; set; }
    }
}
