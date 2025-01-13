using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRDC24.GestureDriven
{
    public class TreeController : MonoBehaviour
    {
        public List<GameObject> trees;

        public Dictionary<GameObject, bool> grownFlags = new Dictionary<GameObject, bool>();
        
        private Animator animator;

        private void Start()
        {
            // disable all trees at beginning
            foreach (var tree in trees)
            {
                tree.SetActive(false);
            }
        }

        IEnumerator GrowAllTrees()
        {
            foreach (var tree in trees)
            {
                grownFlags[tree] = true;
                tree.SetActive(true);
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
        }

        public void GrowTree()
        {
            if (grownFlags.Count == trees.Count)
            {
                Debug.LogWarning("All trees are grown up.");
                return;
            }

            // grow all trees
            StartCoroutine(GrowAllTrees());

            return;
            
            // if there is a tree growing
            if (animator != null)
            {
                var time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (time < 1.0)
                {
                    Debug.LogWarning("One tree is growing.");
                    return;
                }
            }

            foreach (var tree in trees)
            {
                if (grownFlags.ContainsKey(tree)) continue;
                
                print($"{tree.name} is growing.");
                grownFlags[tree] = true;
                tree.SetActive(true);
                animator = tree.GetComponent<Animator>();
                break;
            }
        }

    }
}