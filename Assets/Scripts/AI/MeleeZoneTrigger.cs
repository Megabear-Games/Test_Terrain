using UnityEngine;
using System.Collections;

public class MeleeZoneTrigger : MonoBehaviour
{
	void OnTriggerEnter(Collider col)
	{
		AIStateMachine machine = AIManager.instance.GetAIStateMachine(col.GetInstanceID());
		if (machine)
		{
			machine.inMeleeRange = true;
		}
	}

	void OnTriggerExit(Collider col)
	{
		AIStateMachine machine = AIManager.instance.GetAIStateMachine(col.GetInstanceID());
		if (machine)
		{
			machine.inMeleeRange = false;
		}
	}
}