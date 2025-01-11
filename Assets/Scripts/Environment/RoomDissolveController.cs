using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRDC24.Environment
{
    public class RoomDissolveController : MonoBehaviour
    {
        public GameObject room;
        public Shader dissolveShader;
        [Tooltip("Duration in seconds")] public float duration = 1.0f;
        private Dictionary<GameObject, Material> roomMaterials = new Dictionary<GameObject, Material>();

        private Shader defaultShader;

        const string dissolveKey = "_Dissolve";

        private void Start()
        {

        }

        void BackupMaterials()
        {
            var count = room.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                var go = room.transform.GetChild(i).gameObject;
                Renderer m;
                if (go.TryGetComponent<Renderer>(out m))
                {
                    // backup
                    roomMaterials.Add(go, m.material);

                    if (defaultShader == null)
                    {
                        defaultShader = m.material.shader;
                    }
                }
            }
        }

        public void Dissolve()
        {
            // enable room
            room.SetActive(true);
            
            // backup material
            if (roomMaterials.Count == 0)
            {
                BackupMaterials();
            }

            // change shader
            foreach (var m in roomMaterials.Values)
            {
                m.shader = dissolveShader;
                // set value (not dissolve)
                m.SetFloat(dissolveKey, 0f);
            }

            // start dislove
            StartCoroutine(DynamicDissolve());
        }

        IEnumerator DynamicDissolve()
        {
            var gap = duration / 10f;
            for (var i = 0; i < 10; i++)
            {
                foreach (var m in roomMaterials.Values)
                {
                    m.SetFloat(dissolveKey, i * 0.1f);
                }

                yield return new WaitForSeconds(gap);
            }
            // set inactive
            room.SetActive(false);
        }

        public void Revert()
        {
            room.SetActive(true);
            
            foreach (var m in roomMaterials.Values)
            {
                // m.shader = defaultShader;
                m.SetFloat(dissolveKey, 0f);
            }
        }

        #region Debug

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Dissolve"))
            {
                Dissolve();
            }

            if (GUILayout.Button("Revert"))
            {
                Revert();
            }

            GUILayout.EndVertical();
        }

        #endregion


    }
}