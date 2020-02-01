using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDryadMovement : MonoBehaviour
{
    private float speed = 1.0f;

    public BalanceManager balance;

    void Start()
    {

    }

    void Update()
    {
        Vector2 movement = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            movement += Vector2.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector2.down;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector2.right;
        }
        Vector2 movementDelta = movement.normalized * Time.deltaTime * speed;
        this.transform.position += new Vector3(movementDelta.x, movementDelta.y, 0);
    }
}
