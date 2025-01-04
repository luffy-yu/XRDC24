using UnityEditor;


namespace AmazingAssets.AdvancedDissolveEditor
{
    public class DefaultGUI : ShaderGUI
    {
        public override void OnGUI(UnityEditor.MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (AmazingAssets.AdvancedDissolveEditor.MaterialEditor.DrawDefaultOptionsHeader("Default", null, null))
                base.OnGUI(materialEditor, properties);


            //AmazingAssets
            AmazingAssets.AdvancedDissolveEditor.MaterialEditor.Init(properties);

            //AmazingAssets
            AmazingAssets.AdvancedDissolveEditor.MaterialEditor.DrawDissolveOptions(materialEditor, false, false, false, false, false);
        }
    }
}