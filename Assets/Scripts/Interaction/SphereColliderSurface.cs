﻿using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using UnityEngine;
using UnityEngine.Assertions;

namespace XRDC24.Interaction
{
    public class SphereColliderSurface : MonoBehaviour, ISurface, IBounds
    {
        /// <summary>
        /// The Surface will be represented by this collider.
        /// </summary>
        [Tooltip("The Surface will be represented by this collider.")] [SerializeField]
        private Collider _collider;

        private SphereCollider _sphereCollider;

        public System.Action<bool> OnHit;
        
        private PokeInteractable _pokeInteractable;
        
        protected virtual void Start()
        {
            this.AssertField(_collider, nameof(_collider));
            _sphereCollider = _collider as SphereCollider;
            
            _pokeInteractable = _collider.GetComponent<PokeInteractable>();
        }

        public Transform Transform => transform;

        public Bounds Bounds => _collider.bounds;

        private float threshold => _sphereCollider.radius * transform.localScale.x;

        public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0)
        {
            hit = new SurfaceHit();

            RaycastHit hitInfo;

            // fix: Assertion failed on expression: 'IsNormalized(direction)'
            //      UnityEngine.Collider:Raycast (UnityEngine.Ray,UnityEngine.RaycastHit&,single)
            // Reference: https://discussions.unity.com/t/collider-raycast-is-complaining-that-my-ray-does-not-have-a-normalized-direction/81409
            var newRay = new Ray(ray.origin, ray.direction);

            var touched = false;

            if (ray.direction == Vector3.zero)
            {
                var point = ray.origin;
                Vector3 direction = Bounds.center - point;
                newRay.direction = direction.normalized;
            }

            if (_collider.Raycast(newRay, out hitInfo, maxDistance))
            {
                hit.Point = hitInfo.point;
                hit.Normal = hitInfo.normal;
                hit.Distance = hitInfo.distance;

                var distance = Vector3.Distance(hit.Point, Bounds.center);

                if (distance <= threshold)
                {
                    if (OnHit != null)
                    {
                        OnHit(true);
                    }

                    return true;
                }
            }

            return false;
        }

        public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0)
        {
            Vector3 closest = _collider.ClosestPoint(point);

            Vector3 delta = closest - point;
            if (delta.x == 0f && delta.y == 0f && delta.z == 0f)
            {
                Vector3 direction = Bounds.center - point;
                return Raycast(new Ray(point - direction,
                    direction.normalized), out hit, float.MaxValue);
            }

            return Raycast(new Ray(point, delta.normalized), out hit, maxDistance);
        }

        #region Inject

        public void InjectAllColliderSurface(Collider collider)
        {
            InjectCollider(collider);
        }

        public void InjectCollider(Collider collider)
        {
            _collider = collider;
        }

        #endregion
    }
}