using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_Action : MonoBehaviour
{
    public GameObject enemy;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Axe"))
        {
            Debug.Log("Diffenced");
        }
    }
}
