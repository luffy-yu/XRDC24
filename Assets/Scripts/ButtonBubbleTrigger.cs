using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class ButtonBubbleTrigger : MonoBehaviour
{
    public enum BubbleButtonType
    {
        Start,
        Next
    }

    public BubbleButtonType m_Type;
    
    #region event

    public Action<BubbleButtonType> OnBubbleButtonClicked;

    #endregion
    
    private void OnTriggerEnter(Collider other)
    {
        // Add SphereCollider to XRHand_IndexTip of the Synthetic hand
        // set Radius 0.01 and check Is Trigger

        Debug.Log("trigger enter in bubble button ++");

        var name = other.gameObject.name;
        if (name.Contains("Hand")) // name for poke interaction
        {
            OnBubbleButtonClicked.Invoke(m_Type);
        }
       
    }

    public void ManualTrigger()
    {
        OnBubbleButtonClicked.Invoke(m_Type);
    }
}
