using DA_Assets.FCU.Model;
using DA_Assets.Shared.Extensions;
using System.Linq;

namespace DA_Assets.FCU.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsShadowType(this Effect effect) => effect.Type.ToString().Contains("SHADOW");
        public static bool IsBlurType(this Effect effect) => effect.Type.ToString().Contains("BLUR");

        public static bool IsGradientType(this Paint paint) => paint.Type.ToString().Contains("GRADIENT");

        public static bool IsSingleImageOrVideoType(this FObject paint)
        {
            if (paint.Fills.IsEmpty())
                return false;

            var enabledFills = paint.Fills.Where(x => x.IsVisible());

            if (enabledFills.Count() != 1)
                return false;

            Paint firstPaint = paint.Fills.First();

            bool isImageOrVideo = 
                firstPaint.Type == PaintType.IMAGE || 
                firstPaint.Type == PaintType.VIDEO || 
                firstPaint.Type == PaintType.EMOJI;

            return isImageOrVideo;
        }

        public static bool IsAnyMask(this FObject fobject) => fobject.IsObjectMask() || fobject.IsClipMask() || fobject.IsFrameMask();
        public static bool IsFrameMask(this FObject fobject) => fobject.ContainsTag(FcuTag.Frame);
        public static bool IsClipMask(this FObject fobject) => fobject.ClipsContent.ToBoolNullFalse();
        public static bool IsObjectMask(this FObject fobject) => fobject.IsMask.ToBoolNullFalse();
        public static bool IsGenerativeType(this FObject fobject) => fobject.Data.FcuImageType == FcuImageType.Generative;
        public static bool IsDrawableType(this FObject fobject) => fobject.Data.FcuImageType == FcuImageType.Drawable;
        public static bool IsDownloadableType(this FObject fobject) => fobject.Data.FcuImageType == FcuImageType.Downloadable;
        public static bool IsMaskType(this FObject fobject) => fobject.Data.FcuImageType == FcuImageType.Mask;
    }
}
