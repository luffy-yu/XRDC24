using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DA_Assets.FCU.Extensions;

#if TextMeshPro
using TMPro;
#endif

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class InputFieldDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        private List<FObject> inputFields;
        public List<FObject> InputFields => inputFields;

        public void Init()
        {
            inputFields = new List<FObject>();
        }

        public void Draw(FObject fobject)
        {
            switch (monoBeh.Settings.ComponentSettings.TextComponent)
            {
                case TextComponent.UnityText:
                    fobject.Data.GameObject.TryAddComponent(out InputField inputField);
                    break;
#if TextMeshPro
                case TextComponent.TextMeshPro:
                    fobject.Data.GameObject.TryAddComponent(out TMP_InputField tmpInputField);
                    break;
#endif
            }

            inputFields.Add(fobject);
        }

        public IEnumerator SetTargetGraphics()
        {
            switch (monoBeh.Settings.ComponentSettings.TextComponent)
            {
                case TextComponent.UnityText:
                    yield return SetTargetGraphicsInputFields();
                    break;
                case TextComponent.TextMeshPro:
                    yield return SetTargetGraphicsTmpInputFields();
                    break;
            }

            inputFields.Clear();
        }

        private IEnumerator SetTargetGraphicsInputFields()
        {
            foreach (FObject fobject in inputFields)
            {
                if (!fobject.Data.GameObject.TryGetComponentSafe(out InputField inputField))
                    continue;

                InputFieldModel ifm = GetGraphics(fobject.Data);

                if (ifm.TextArea.TryGetComponentSafe(out RectTransform textArea))
                {
                    textArea.SetSmartAnchor(AnchorType.StretchAll);
                }

                if (ifm.Background.TryGetComponentSafe(out Graphic bg))
                {
                    inputField.targetGraphic = bg;
                }

                if (ifm.Placeholder.TryGetComponentSafe(out Graphic ph))
                {
                    ph.gameObject.SetActive(true);  
                    inputField.placeholder = ph;
                }

                if (ifm.TextComponent.TryGetComponentSafe(out Text text))
                {
                    text.gameObject.SetActive(true);
                    inputField.textComponent = text;
                    inputField.textComponent.supportRichText = false;
                    text.resizeTextForBestFit = false;
                }

                if (fobject.ContainsTag(FcuTag.PasswordField))
                {
                    inputField.contentType = InputField.ContentType.Password;
                    inputField.asteriskChar = FcuConfig.AsterisksChar;
                }
                else
                {
                    inputField.contentType = InputField.ContentType.Standard;
                }

                inputField.enabled = false;
                yield return WaitFor.Delay001();
                inputField.enabled = true;
            }
        }

        private IEnumerator SetTargetGraphicsTmpInputFields()
        {
#if TextMeshPro
            foreach (FObject fobject in inputFields)
            {
                if (!fobject.Data.GameObject.TryGetComponentSafe(out TMP_InputField inputField))
                    continue;

                InputFieldModel ifm = GetGraphics(fobject.Data);

                if (ifm.TextArea.TryGetComponentSafe(out RectTransform textArea))
                {
                    textArea.SetSmartAnchor(AnchorType.StretchAll);

                    inputField.textViewport = textArea;
                    textArea.gameObject.TryAddComponent(out RectMask2D mask);
                }

                if (ifm.Background.TryGetComponentSafe(out Graphic bg))
                {
                    inputField.targetGraphic = bg;
                }

                if (ifm.Placeholder.TryGetComponentSafe(out Graphic ph))
                {
                    ph.gameObject.SetActive(true);
                    inputField.placeholder = ph;
                }

                if (ifm.TextComponent.TryGetComponentSafe(out TMP_Text text))
                {
                    text.gameObject.SetActive(true);
                    inputField.textComponent = text;
                    text.enableAutoSizing = false;
                }

                if (fobject.ContainsTag(FcuTag.PasswordField))
                {
                    inputField.contentType = TMP_InputField.ContentType.Password;
                    inputField.asteriskChar = FcuConfig.AsterisksChar;
                }
                else
                {
                    inputField.contentType = TMP_InputField.ContentType.Standard;
                }

                inputField.enabled = false;
                yield return WaitFor.Delay001();
                inputField.enabled = true;
            }
#endif
            yield return null;
        }

        private InputFieldModel GetGraphics(SyncData syncData)
        {
            SyncHelper[] syncHelpers = syncData.GameObject.GetChilds<SyncHelper>();

            InputFieldModel field = new InputFieldModel();

            foreach (SyncHelper item in syncHelpers)
            {
                if (field.Background == null && item.ContainsTag(FcuTag.Background))
                {
                    field.Background = item.gameObject;
                    break;
                }
            }

            foreach (SyncHelper item in syncHelpers)
            {
                if (field.TextArea == null && item.name.IsInputTextArea())
                {
                    field.TextArea = item.gameObject;
                }
                else if (field.TextComponent == null && item.ContainsTag(FcuTag.Text) && !item.ContainsTag(FcuTag.Placeholder))
                {
                    field.TextComponent = item.gameObject;
                }
                else if (field.Placeholder == null && item.ContainsTag(FcuTag.Placeholder))
                {
                    field.Placeholder = item.gameObject;
                }
                else if (field.Background == null && item.ContainsTag(FcuTag.Image))
                {
                    field.Background = item.gameObject;
                }
            }

            return field;
        }
    }

    internal struct InputFieldModel
    {
        public GameObject TextArea { get; set; }
        public GameObject Background { get; set; }
        public GameObject TextComponent { get; set; }
        public GameObject Placeholder { get; set; }
    }
}
