#if UNITY_2021_1_OR_NEWER

#if USING_HDRP

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


namespace ShaderCrew.SeeThroughShader
{
    using static HDMaterialProperties;

    /// <summary>
    /// Represents an advanced options material UI block.
    /// </summary>
    public class AdvancedOptionsUIBlock : MaterialUIBlock
    {
        /// <summary>Options that define the visibility of fields in the block.</summary>
        [Flags]
        public enum Features
        {
            /// <summary>Hide all the fields in the block.</summary>
            None = 0,
            /// <summary>Display the instancing field.</summary>
            Instancing = 1 << 0,
            /// <summary>Display the specular occlusion field.</summary>
            SpecularOcclusion = 1 << 1,
            /// <summary>Display the add precomputed velocity field.</summary>
            AddPrecomputedVelocity = 1 << 2,
            /// <summary>Display the double sided GI field.</summary>
            DoubleSidedGI = 1 << 3,
            /// <summary>Display the emission GI field.</summary>
            EmissionGI = 1 << 4,
            /// <summary>Display the motion vector field.</summary>
            MotionVector = 1 << 5,
            /// <summary>Display the fields for the shaders.</summary>
            StandardLit = Instancing | SpecularOcclusion | AddPrecomputedVelocity,
            /// <summary>Display all the field.</summary>
            All = ~0
        }

        internal class Styles
        {
            public static GUIContent header { get; } = EditorGUIUtility.TrTextContent("Advanced Options");
            public static GUIContent specularOcclusionModeText { get; } = EditorGUIUtility.TrTextContent("Specular Occlusion Mode", "Determines the mode used to compute specular occlusion");
            public static GUIContent addPrecomputedVelocityText { get; } = EditorGUIUtility.TrTextContent("Add Precomputed Velocity", "Requires additional per vertex velocity info");
            public static GUIContent motionVectorForVertexAnimationText { get; } = EditorGUIUtility.TrTextContent("Motion Vector For Vertex Animation", "When enabled, HDRP will correctly handle velocity for vertex animated object. Only enable if there is vertex animation in the ShaderGraph.");
            public static GUIContent bakedEmission = EditorGUIUtility.TrTextContent("Baked Emission", "");
        }

        MaterialProperty specularOcclusionMode = null;

        MaterialProperty addPrecomputedVelocity = null;
        const string kAddPrecomputedVelocity = HDMaterialProperties.kAddPrecomputedVelocity;

        Features m_Features;

        /// <summary>
        /// Constructs the AdvancedOptionsUIBlock based on the parameters.
        /// </summary>
        /// <param name="expandableBit">Bit index used to store the foldout state.</param>
        /// <param name="features">Features of the block.</param>
        public AdvancedOptionsUIBlock(ExpandableBit expandableBit, Features features = Features.All)
            : base(expandableBit, Styles.header)
        {
            m_Features = features;
        }

        /// <summary>
        /// Loads the material properties for the block.
        /// </summary>
        public override void LoadMaterialProperties()
        {
            specularOcclusionMode = FindProperty(kSpecularOcclusionMode);

            // Migration code for specular occlusion mode. If we find a keyword _ENABLESPECULAROCCLUSION
            // it mean the material have never been migrated, so force the specularOcclusionMode to 2 in this case
            if (materials.Length == 1)
            {
                if (materials[0].IsKeywordEnabled("_ENABLESPECULAROCCLUSION"))
                {
                    specularOcclusionMode.floatValue = 2.0f;
                }
            }

            addPrecomputedVelocity = FindProperty(kAddPrecomputedVelocity);
        }

        /// <summary>
        /// Renders the properties in the block.
        /// </summary>
        protected override void OnGUIOpen()
        {
            if ((m_Features & Features.Instancing) != 0)
                materialEditor.EnableInstancingField();

            if ((m_Features & Features.DoubleSidedGI) != 0)
            {
                // If the shader graph have a double sided flag, then we don't display this field.
                // The double sided GI value will be synced with the double sided property during the SetupBaseUnlitKeywords()
                if (!materials.All(m => m.HasProperty(kDoubleSidedEnable)))
                    materialEditor.DoubleSidedGIField();
            }

            if ((m_Features & Features.EmissionGI) != 0)
                materialEditor.LightmapEmissionFlagsProperty(0, true);

            if ((m_Features & Features.MotionVector) != 0)
                DrawMotionVectorToggle();
            if ((m_Features & Features.SpecularOcclusion) != 0)
                materialEditor.ShaderProperty(specularOcclusionMode, Styles.specularOcclusionModeText);
            if ((m_Features & Features.AddPrecomputedVelocity) != 0)
            {
                if (addPrecomputedVelocity != null)
                    materialEditor.ShaderProperty(addPrecomputedVelocity, Styles.addPrecomputedVelocityText);
            }
        }

        void DrawMotionVectorToggle()
        {
            // We have no way to setup motion vector pass to be false by default for a shader graph
            // So here we workaround it with materialTag system by checking if a tag exist to know if it is
            // the first time we display this information. And thus setup the MotionVector Pass to false.
            const string materialTag = "MotionVector";

            foreach (var material in materials)
            {
                string tag = material.GetTag(materialTag, false, "Nothing");
                if (tag == "Nothing")
                {
                    material.SetShaderPassEnabled(HDShaderPassNames.s_MotionVectorsStr, false);
                    material.SetOverrideTag(materialTag, "User");
                }
            }

            //In the case of additional velocity data we will enable the motion vector pass.
            bool enabled = GetEnabledMotionVectorVertexAnim(materials[0]);
            bool mixedValue = materials.Length > 1 && materials.Any(m => GetEnabledMotionVectorVertexAnim(m) != enabled);
            bool addPrecomputedVelocity = GetAddPrecomputeVelocity(materials[0]);
            bool mixedPrecomputedVelocity = materials.Length > 1 && materials.Any(m => GetAddPrecomputeVelocity(m) != addPrecomputedVelocity);

            EditorGUI.BeginChangeCheck();
            {
                using (new EditorGUI.DisabledScope(addPrecomputedVelocity || mixedPrecomputedVelocity))
                {
                    EditorGUI.showMixedValue = mixedValue;
                    enabled = EditorGUILayout.Toggle(Styles.motionVectorForVertexAnimationText, enabled);
                    EditorGUI.showMixedValue = false;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var mat in materials)
                    mat.SetShaderPassEnabled(HDShaderPassNames.s_MotionVectorsStr, enabled);
            }

            bool GetEnabledMotionVectorVertexAnim(Material material)
            {
                bool addPrecomputedVelocity = GetAddPrecomputeVelocity(material);
                bool currentMotionVectorState = material.GetShaderPassEnabled(HDShaderPassNames.s_MotionVectorsStr);

                return currentMotionVectorState || addPrecomputedVelocity;
            }

            bool GetAddPrecomputeVelocity(Material material)
            {
                bool addPrecomputedVelocity = false;

                if (materials[0].HasProperty(kAddPrecomputedVelocity))
                    addPrecomputedVelocity = materials[0].GetInt(kAddPrecomputedVelocity) != 0;

                return addPrecomputedVelocity;
            }
        }
    }
}

#endif


#endif