using UnityEngine;
using System.Collections;

public abstract class Interactable : MonoBehaviour
{
    [HideInInspector]
    public bool isInteracted = false;   //�Ƿ���ɽ���
    public abstract void Interact(PlayerController player, bool isFKeyInteraction = false);

    private void Start()
    {
    }

    // �ɽ�������

    //���ý���״̬
    public void FinishInteraction()
    {
        isInteracted = true;
        StartCoroutine("ResetInteractionState1", 1f);
    }

    //���ý���״̬
    IEnumerator ResetInteractionState1(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInteracted = false;
    }


}
