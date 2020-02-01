using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnClick : MonoBehaviour
{
    void OnMouseDown()
    {
        GameObject.Destroy(this.gameObject);
    }
}
