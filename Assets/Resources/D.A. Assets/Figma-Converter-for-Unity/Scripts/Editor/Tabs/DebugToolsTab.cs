using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DA_Assets.FCU
{
    internal class DebugToolsTab : ScriptableObjectBinder<FcuSettingsWindow, FigmaConverterUnity>
    {

        private Editor fcuConfigEditor;

        public override void Init()
        {
            fcuConfigEditor = Editor.CreateEditor(FcuConfig.Instance);
        }

        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_debug_tools.Localize());
            gui.Space15();

            FcuConfig.Instance.DebugSettings.DebugMode = gui.Toggle(
                new GUIContent(FcuLocKey.label_debug_mode.Localize(), FcuLocKey.tooltip_debug_mode.Localize()),
                FcuConfig.Instance.DebugSettings.DebugMode);

            if (FcuConfig.Instance.DebugSettings.DebugMode)
            {
                FcuConfig.Instance.DebugSettings.LogDefault = gui.Toggle(new GUIContent(FcuLocKey.label_log_default.Localize()),
                    FcuConfig.Instance.DebugSettings.LogDefault);

                FcuConfig.Instance.DebugSettings.LogSetTag = gui.Toggle(new GUIContent(FcuLocKey.label_log_set_tag.Localize()),
                    FcuConfig.Instance.DebugSettings.LogSetTag);

                FcuConfig.Instance.DebugSettings.LogIsDownloadable = gui.Toggle(new GUIContent(FcuLocKey.label_log_downloadable.Localize()),
                    FcuConfig.Instance.DebugSettings.LogIsDownloadable);

                FcuConfig.Instance.DebugSettings.LogTransform = gui.Toggle(new GUIContent(FcuLocKey.label_log_transform.Localize()),
                    FcuConfig.Instance.DebugSettings.LogTransform);

                FcuConfig.Instance.DebugSettings.LogGameObjectDrawer = gui.Toggle(new GUIContent(FcuLocKey.label_log_go_drawer.Localize()),
                    FcuConfig.Instance.DebugSettings.LogGameObjectDrawer);

                FcuConfig.Instance.DebugSettings.LogComponentDrawer = gui.Toggle(new GUIContent(FcuLocKey.label_log_component_drawer.Localize()),
                    FcuConfig.Instance.DebugSettings.LogComponentDrawer);

                FcuConfig.Instance.DebugSettings.LogHashGenerator = gui.Toggle(new GUIContent(FcuLocKey.label_log_hash_generator_drawer.Localize()),
                     FcuConfig.Instance.DebugSettings.LogHashGenerator);
            }

            gui.Space15();

            if (gui.OutlineButton("Open logs folder"))
            {
                FcuConfig.LogPath.OpenFolderInOS();
            }

            gui.Space15();

            if (gui.OutlineButton("Open cache folder"))
            {
                FcuConfig.CachePath.OpenFolderInOS();
            }

            gui.Space15();

            if (gui.OutlineButton("Open backup folder"))
            {
                SceneBackuper.GetBackupsPath().OpenFolderInOS();
            }

            gui.Space15();

            if (gui.OutlineButton("Test Button"))
            {
                GameObject go = null;
                go.TryGetComponentSafe<RectTransform>(out var rt);
            }

            if (FcuConfig.Instance.DebugSettings.DebugMode)
            {
                gui.Space30();

                fcuConfigEditor.OnInspectorGUI();

                gui.Space30();
                scriptableObject.Inspector.DrawBaseOnInspectorGUI();
            }
        }


    }
}