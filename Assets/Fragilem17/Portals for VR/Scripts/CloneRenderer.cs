using Fragilem17.MirrorsAndPortals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Fragilem17.MirrorsAndPortals
{
    public class CloneRenderer : MonoBehaviour
    {
        [Tooltip("Leave empty to target this gameObject")]
        public GameObject GameObjectToClone;

        protected static GameObject _cloneHolder;
        protected static List<GameObject> _clones;
        protected GameObject _clone;

        protected static readonly Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

        protected Dictionary<Transform, Transform> _tranforms = new Dictionary<Transform, Transform>();
        protected Dictionary<MeshRenderer, MeshRenderer> _meshRenderers = new Dictionary<MeshRenderer, MeshRenderer>();
        protected Dictionary<MeshRenderer, MeshFilter> _meshFilters = new Dictionary<MeshRenderer, MeshFilter>();

        protected Dictionary<SkinnedMeshRenderer, MeshRenderer> _skinnedMeshRenderers = new Dictionary<SkinnedMeshRenderer, MeshRenderer>();
        protected Dictionary<SkinnedMeshRenderer, MeshFilter> _skinnedMeshRendererFilters = new Dictionary<SkinnedMeshRenderer, MeshFilter>();
        protected Dictionary<SkinnedMeshRenderer, Mesh> _skinnedMeshRendererMeshes = new Dictionary<SkinnedMeshRenderer, Mesh>();

        protected Portal _inPortal;

        protected Transform _inTransform;
        protected Transform _outTransform;

        // todo: layer! changing.. add options to choose a layer for the clone, or copy it from the source

        protected virtual void OnEnable()
        {
            if (GameObjectToClone == null)
            {
                GameObjectToClone = gameObject;
            }
            GenerateClone();
            _clone.SetActive(false);

        }

        protected virtual void OnDisable()
        {
            DestroyClone();
        }

        protected virtual void OnDestroy()
        {
            DestroyClone();
        }

        protected virtual void DestroyClone()
        {
            if (_clones != null)
            {
                _clones.Remove(_clone);
            }

            if (_clones.Count == 0 || !_cloneHolder)
            {
                // destroy the holder
                DestroyImmediate(_cloneHolder);
                _cloneHolder = null;
            }

            _meshRenderers.Clear();
            _meshFilters.Clear();
            _skinnedMeshRenderers.Clear();
            _skinnedMeshRendererMeshes.Clear();

            if (_clone)
            {
                DestroyImmediate(_clone);
                _clone = null;
            }

        }

        protected virtual void GenerateClone()
        {
            if (!_cloneHolder)
            {
                _cloneHolder = new GameObject();
                _cloneHolder.transform.SetParent(null);
                _cloneHolder.hideFlags = HideFlags.DontSave;
                _cloneHolder.name = "Portal ClonesHolder";
                _clones = new List<GameObject>();
            }

            if (!_clone)
            {
                _clone = new GameObject();
                _clones.Add(_clone);
                _clone.hideFlags = HideFlags.DontSave;
                _clone.name = "clone_" + name;
                _clone.layer = gameObject.layer;
                CreateMeshrendererChildren(_clone, GameObjectToClone, _cloneHolder.transform);
            }
        }

        protected virtual void CreateMeshrendererChildren(GameObject target, GameObject original, Transform parent)
        {
            _tranforms[original.transform] = target.transform;
            target.transform.localScale = original.transform.lossyScale;
            target.transform.SetParent(parent);
            target.transform.SetPositionAndRotation(original.transform.position, original.transform.rotation);


            MeshRenderer originalMr = original.GetComponent<MeshRenderer>();
            if (originalMr != null)
            {
                MeshFilter originalMf = originalMr.GetComponent<MeshFilter>();
                if (originalMf != null)
                {
                    _meshFilters[originalMr] = target.AddComponent<MeshFilter>();
                    _meshFilters[originalMr].sharedMesh = originalMf.sharedMesh;
                    _meshRenderers[originalMr] = target.AddComponent<MeshRenderer>();
                    _meshRenderers[originalMr].sharedMaterials = originalMr.sharedMaterials;
                }
            }

            SkinnedMeshRenderer originalSmr = original.GetComponent<SkinnedMeshRenderer>();
            if (originalSmr != null)
            {
                _skinnedMeshRenderers[originalSmr] = target.AddComponent<MeshRenderer>();
                _skinnedMeshRendererFilters[originalSmr] = target.AddComponent<MeshFilter>();
                _skinnedMeshRendererMeshes[originalSmr] = new Mesh();
                originalSmr.BakeMesh(_skinnedMeshRendererMeshes[originalSmr], true);
                _skinnedMeshRendererFilters[originalSmr].sharedMesh = _skinnedMeshRendererMeshes[originalSmr];
                _skinnedMeshRenderers[originalSmr].sharedMaterials = originalSmr.sharedMaterials;
            }

            for (int i = 0; i < original.transform.childCount; i++)
            {
                Transform child = original.transform.GetChild(i);
                CloneRenderer childsCloneRenderer = child.GetComponent<CloneRenderer>();
                MeshRenderer originalMrChildren = child.GetComponentInChildren<MeshRenderer>(false);
                SkinnedMeshRenderer originalSmrChildren = child.GetComponentInChildren<SkinnedMeshRenderer>(false);

                if (childsCloneRenderer == null && (originalMrChildren != null || originalSmrChildren != null))
                {
                    GameObject clonedChild = new GameObject(child.gameObject.name);
                    clonedChild.hideFlags = HideFlags.DontSave;
                    clonedChild.layer = child.gameObject.layer;
                    CreateMeshrendererChildren(clonedChild, child.gameObject, target.transform);
                }
            }

        }


        public virtual void SetIsInPortal(Portal portal, bool physicsDriven = false)
        {
            if (_clone != null && _inPortal != portal)
            {
                //Debug.Log(gameObject.name + " IN, inPortal: " + portal.name + " physicsDriven: " + physicsDriven);
                _inPortal = portal;

                _inTransform = _inPortal.PortalSurface.transform;
                _outTransform = _inPortal.OtherPortal.PortalSurface.transform;

                _clone.SetActive(true);
                CloneAnimations();
                PositionClone();
            }
        }

        public virtual void ExitPortal(Portal portal, bool physicsDriven = false)
        {
            if (!_clone) {
                return;
            }

            //Debug.Log(gameObject.name + " OUT, portal: " + portal?.name + " : " + physicsDriven);
            _clone.SetActive(false);

            foreach (KeyValuePair<MeshRenderer, MeshRenderer> entry in _meshRenderers)
            {
                if (entry.Key.sharedMaterial.HasProperty("_SectionPos"))
                {
                    foreach (Material m in entry.Key.materials)
                    {
                        m.SetFloat("_DoClipping", 0);
                    }
                }
            }

            foreach (KeyValuePair<SkinnedMeshRenderer, MeshRenderer> entry in _skinnedMeshRenderers)
            {
                if (entry.Key.sharedMaterial.HasProperty("_SectionPos"))
                {
                    foreach (Material m in entry.Key.materials)
                    {
                        m.SetFloat("_DoClipping", 0);
                    }
                }
            }

            _inPortal = null;
        }

        void LateUpdate()
        {
            if (_inPortal)
            {

                if (!_clone)
                {
                    DestroyClone();
                    GenerateClone();
                }

                CloneAnimations();
                PositionClone();
            }
        }

        protected virtual void CloneAnimations()
        {
            if (_inPortal != null && _clone.activeSelf)
            {
                // reset pos of root element
                //float scaleFactor = (inPortal.OtherPortal.transform.localScale.x / inPortal.transform.localScale.x);
                _clone.transform.localScale = GameObjectToClone.transform.lossyScale;
                _clone.transform.SetPositionAndRotation(GameObjectToClone.transform.position, transform.rotation);

                foreach (KeyValuePair<Transform, Transform> entry in _tranforms)
                {
                    entry.Value.gameObject.SetActive(entry.Key.gameObject.activeSelf);
                    if (entry.Key.gameObject.activeSelf)
                    {
                        entry.Value.transform.localScale = entry.Key.transform.localScale;
                        entry.Value.transform.localPosition = entry.Key.transform.localPosition;
                        entry.Value.transform.localRotation = entry.Key.transform.localRotation;
                        //cloneMr.transform.SetPositionAndRotation(orginalMr.transform.position, entry.Key.transform.rotation);
                    }
                }

                foreach (KeyValuePair<SkinnedMeshRenderer, Mesh> entry in _skinnedMeshRendererMeshes)
                {
                    entry.Key.BakeMesh(entry.Value, true);
                }
            }
        }

        protected virtual  void PositionClone()
        {
            if (_inPortal != null && _inTransform != null)
            {
                float scaleFactor = (_inPortal.OtherPortal.transform.lossyScale.x / _inPortal.transform.lossyScale.x);
                _clone.transform.localScale = GameObjectToClone.transform.lossyScale * scaleFactor;

                // Update position of clone.
                Vector3 relativePos = _inTransform.InverseTransformPoint(GameObjectToClone.transform.position);
                relativePos = halfTurn * relativePos;
                _clone.transform.position = _outTransform.TransformPoint(relativePos);

                // Update rotation of clone.
                Quaternion relativeRot = Quaternion.Inverse(_inTransform.rotation) * GameObjectToClone.transform.rotation;
                relativeRot = halfTurn * relativeRot;
                _clone.transform.rotation = _outTransform.rotation * relativeRot;


                foreach (KeyValuePair<MeshRenderer, MeshRenderer> entry in _meshRenderers)
                {
                    if (entry.Key.sharedMaterial.HasProperty("_SectionPos"))
                    {
                        foreach (Material m in entry.Key.materials)
                        {
                            m.SetVector("_SectionPos", _inTransform.position + (_inTransform.forward * 0.02f));
                            m.SetVector("_SectionNormal", _inTransform.forward);
                            m.SetFloat("_DoClipping", 1);
                        }

                        foreach (Material m in entry.Value.materials)
                        {
                            m.SetVector("_SectionPos", _outTransform.position + (_outTransform.forward * 0.02f));
                            m.SetVector("_SectionNormal", _outTransform.forward);
                            m.SetFloat("_DoClipping", 1);
                        }
                    }
                }

                foreach (KeyValuePair<SkinnedMeshRenderer, MeshRenderer> entry in _skinnedMeshRenderers)
                {
                    if (entry.Key && entry.Key.sharedMaterial.HasProperty("_SectionPos"))
                    {
                        foreach (Material m in entry.Key.materials)
                        {
                            m.SetVector("_SectionPos", _inTransform.position + (_inTransform.forward * 0.02f));
                            m.SetVector("_SectionNormal", _inTransform.forward);
                            m.SetFloat("_DoClipping", 1);
                        }

                        foreach (Material m in entry.Value.materials)
                        {
                            m.SetVector("_SectionPos", _outTransform.position + (_outTransform.forward * 0.02f));
                            m.SetVector("_SectionNormal", _outTransform.forward);
                            m.SetFloat("_DoClipping", 1);
                        }
                    }
                }
            }
        }

        public static void DrawBounds(Bounds b, float delay = 0)
        {
            // bottom
            var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
            var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
            var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
            var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

            Debug.DrawLine(p1, p2, Color.blue, delay);
            Debug.DrawLine(p2, p3, Color.red, delay);
            Debug.DrawLine(p3, p4, Color.yellow, delay);
            Debug.DrawLine(p4, p1, Color.magenta, delay);

            // top
            var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
            var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
            var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
            var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

            Debug.DrawLine(p5, p6, Color.blue, delay);
            Debug.DrawLine(p6, p7, Color.red, delay);
            Debug.DrawLine(p7, p8, Color.yellow, delay);
            Debug.DrawLine(p8, p5, Color.magenta, delay);

            // sides
            Debug.DrawLine(p1, p5, Color.white, delay);
            Debug.DrawLine(p2, p6, Color.gray, delay);
            Debug.DrawLine(p3, p7, Color.green, delay);
            Debug.DrawLine(p4, p8, Color.cyan, delay);
        }

    }
}