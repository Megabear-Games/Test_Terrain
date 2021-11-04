using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player being Damaged");
        }
    }
}
