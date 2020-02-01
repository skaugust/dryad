using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingSize : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        transform.localScale = new Vector3(transform.localScale.x + Time.deltaTime, transform.localScale.y + Time.deltaTime, transform.localScale.z);
    }
}
