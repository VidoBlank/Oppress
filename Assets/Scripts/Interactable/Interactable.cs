using UnityEngine;
using System.Collections;

public abstract class Interactable : MonoBehaviour
{
    [HideInInspector]
    public bool isInteracted = false;   //是否完成交互
    public abstract void Interact(PlayerController player, bool isFKeyInteraction = false);

    private void Start()
    {
    }

    // 可交互物体

    //重置交互状态
    public void FinishInteraction()
    {
        isInteracted = true;
        StartCoroutine("ResetInteractionState1", 1f);
    }

    //重置交互状态
    IEnumerator ResetInteractionState1(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInteracted = false;
    }


}
