﻿using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using UnityEngine.UI;

#if UNITY_UI_EXTENSIONS_EXISTS
using UnityEngine.UI.Extensions;
#endif

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class GridLayoutDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject)
        {
            if (fobject.Data.GameObject.TryGetComponentSafe(out LayoutGroup oldLayoutGroup))
            {
                oldLayoutGroup.Destroy();
            }

#if UNITY_UI_EXTENSIONS_EXISTS
            fobject.Data.GameObject.TryAddComponent(out FlowLayoutGroup layoutGroup);

            layoutGroup.childAlignment = fobject.GetHorLayoutAnchor();
            layoutGroup.padding = fobject.GetPadding();

            float spacingX;
            float spacingY;

            if (fobject.PrimaryAxisAlignItems == PrimaryAxisAlignItem.SPACE_BETWEEN)
            {
                layoutGroup.ChildForceExpandWidth = true;
                spacingX = 0;
            }
            else
            {
                layoutGroup.ChildForceExpandWidth = false;
                spacingX = fobject.ItemSpacing.ToFloat();
            }

            if (fobject.CounterAxisAlignContent == "SPACE_BETWEEN")
            {
                layoutGroup.ChildForceExpandHeight = true;
                spacingY = 0;
            }
            else
            {
                layoutGroup.ChildForceExpandHeight = false;
                spacingY = fobject.CounterAxisSpacing.ToFloat();
            }

            layoutGroup.SpacingX = spacingX;
            layoutGroup.SpacingY = spacingY;
#endif
        }
    }
}
