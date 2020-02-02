using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDryadMovement : MonoBehaviour
{
    private float speed = 5.0f;

    public BalanceManager balance;

    public AudioSource channelSound;
    public AudioSource desolaceSound;
    public AudioSource forestSound;
    public AudioSource grassSound;
    public AudioSource pollutionSound;

    void Start()
    {

    }

    public void PlayChannel()
    {
        channelSound.PlayOneShot(channelSound.clip);
    }

    public void AdjustDesolaceSound(float amplitude)
    {
        desolaceSound.volume = amplitude;
    }

    public void AdjustForestSound(float amplitude)
    {
        forestSound.volume = amplitude;
    }

    public void AdjustGrassSound(float amplitude)
    {
        grassSound.volume = amplitude;
    }

    public void AdjustPollutionSound(float amplitude)
    {
        pollutionSound.volume = amplitude;
    }

    void Update()
    {

        // TODO: change speed based on tile underneath
        List<BalanceTileModel> tiles = balance.getTilesNearby(balance.dryad.transform.position, 0);
        float standingOn = 0;
        foreach (BalanceTileModel a in tiles)
        {
            standingOn += BalanceTileModel.CalculateTierAffect(a.tier);
        }
        standingOn = standingOn / tiles.Count;

        if (standingOn >= 2)
        {
            speed = 7f;
        }
        else
        {
            speed = 5f;
        }

        if (Input.GetKey(KeyCode.Space) && balance.mana > 1)
        {
            speed = speed - 4f;
        }

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
