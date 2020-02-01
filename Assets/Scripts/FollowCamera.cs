using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject dryad;

    void LateUpdate()
    {
        Vector2 delta = dryad.transform.position - this.transform.position;
        this.transform.position += Vector3.Lerp(Vector2.zero, delta, Time.deltaTime);
    }
}
