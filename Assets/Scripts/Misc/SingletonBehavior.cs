using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    
    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of " + typeof(T) + " found!");
            Destroy(gameObject);
            return;
        }
        
        Instance = this as T;
    }
}
