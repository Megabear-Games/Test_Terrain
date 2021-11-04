using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public EnemyTest goblin;
    public float damage = 10f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            goblin.TakeDamage(damage);
        }
        else if(other.CompareTag("Shield"))
        {
            Rigidbody rb    = goblin.GetComponent<Rigidbody>();
            rb.AddForce(-transform.forward * 5.0f, ForceMode.VelocityChange);
        }
    }
}
