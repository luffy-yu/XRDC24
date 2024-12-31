// Reference: https://communityforums.atmeta.com/t5/Unity-Development/How-to-make-a-collider-pokeable-via-Poke-Interactable/td-p/1253378

using System;
using Oculus.Interaction.Surfaces;
using UnityEngine;


namespace XRDC24.Interaction
{
    [RequireComponent(typeof(SphereColliderSurface))]
    public class ColliderSurfacePatch : MonoBehaviour, ISurfacePatch
    {
        public SphereColliderSurface surface;

        public ISurface BackingSurface => surface;

        public Transform Transform => surface.Transform;

        public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0)
        {
            return surface.ClosestSurfacePoint(point, out hit, maxDistance);
        }

        public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0)
        {
            return surface.Raycast(ray, out hit, maxDistance);
        }
    }
}