using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    // Statics
    private static AIManager _instance = null;
    public static AIManager instance
    {
        get
        {
            if (_instance == null)
                _instance = (AIManager)FindObjectOfType(typeof(AIManager));
            return _instance;
        }
    }

    //private
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();

    // Public Methods
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if(!_stateMachines.ContainsKey(key))
        {
            _stateMachines[key] = stateMachine;
        }
    }

    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;
        if(_stateMachines.TryGetValue(key, out machine))
        {
            return machine;
        }

        return null;
    }
}
