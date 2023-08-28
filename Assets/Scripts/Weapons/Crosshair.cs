using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    void Start()
    {
        transform.parent = null;
    }
    
    void Update()
    {
        transform.Rotate(Vector3.forward * -60f * Time.deltaTime);   
    }
}
