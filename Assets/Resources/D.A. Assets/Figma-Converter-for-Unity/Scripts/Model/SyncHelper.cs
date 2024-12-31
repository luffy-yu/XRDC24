using System;
using UnityEngine;

namespace DA_Assets.FCU.Model
{
    //TODO: set current GameObject to SyncHelper
    [Serializable]
    public class SyncHelper : MonoBehaviour
    {
        void OnValidate()
        {
            try
            {
                data.DisplayNameHierarchyInField();
            }
            catch
            {

            }
        }

        [SerializeField] SyncData data;
        public SyncData Data { get => data; set => data = value; }

        public int HierarchyLevel
        {
            get
            {
                int level = 0;
                Transform current = transform;

                while (current.parent != null)
                {
                    level++;
                    current = current.parent;
                }

                return level;
            }
        }
    }
}
