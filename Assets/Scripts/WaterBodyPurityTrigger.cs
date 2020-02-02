using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBodyPurityTrigger : MonoBehaviour
{
    public bool pure = false;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        pure = true;
        gameObject.GetComponent<MeshRenderer>().enabled = pure;
    }
}
