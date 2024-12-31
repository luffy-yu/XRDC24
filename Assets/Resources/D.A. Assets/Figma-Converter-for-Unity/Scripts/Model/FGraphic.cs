using DA_Assets.FCU.Model;
using DA_Assets.Shared.Extensions;
using System;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    internal class FGraphic
    {
        [SerializeField] bool hasFill;
        internal bool HasFill { get => hasFill; set => hasFill = value; }

        [SerializeField] bool hasStroke;
        internal bool HasStroke { get => hasStroke; set => hasStroke = value; }

        [SerializeField] FFill fill;
        internal FFill Fill { get => fill; set => fill = value; }

        [SerializeField] FStroke stroke;
        internal FStroke Stroke { get => stroke; set => stroke = value; }

        [SerializeField] Color spriteSingleColor;
        internal Color SpriteSingleColor { get => spriteSingleColor; set => spriteSingleColor = value; }

        [SerializeField] Paint spriteSingleLinearGradient;
        internal Paint SpriteSingleLinearGradient { get => spriteSingleLinearGradient; set => spriteSingleLinearGradient = value; }

        [SerializeField] bool fillAlpha1;
        internal bool FillAlpha1 { get => fillAlpha1; set => fillAlpha1 = value; }
    }

    [Serializable]
    internal struct FFill
    {
        [SerializeField] bool hasSolid;
        internal bool HasSolid { get => hasSolid; set => hasSolid = value; }

        [SerializeField] bool hasGradient;
        internal bool HasGradient { get => hasGradient; set => hasGradient = value; }

        [SerializeField] Paint gradientPaint;
        internal Paint GradientPaint { get => gradientPaint; set => gradientPaint = value; }

        [SerializeField] Paint solidPaint;
        internal Paint SolidPaint { get => solidPaint; set => solidPaint = value; }

        [SerializeField] Color singleColor;
        internal Color SingleColor { get => singleColor; set => singleColor = value; }
    }

    [Serializable]
    internal struct FStroke
    {
        [SerializeField] StrokeAlign align;
        internal StrokeAlign Align { get => align; set => align = value; }

        [SerializeField] float weight;
        internal float Weight { get => weight; set => weight = value; }

        [SerializeField] bool hasSolid;
        internal bool HasSolid { get => hasSolid; set => hasSolid = value; }

        [SerializeField] bool hasGradient;
        internal bool HasGradient { get => hasGradient; set => hasGradient = value; }

        [SerializeField] Paint solidPaint;
        internal Paint SolidPaint { get => solidPaint; set => solidPaint = value; }

        [SerializeField] Paint gradientPaint;
        internal Paint GradientPaint { get => gradientPaint; set => gradientPaint = value; }

        [SerializeField] Color singleColor;
        internal Color SingleColor { get => singleColor; set => singleColor = value; }
    }
}