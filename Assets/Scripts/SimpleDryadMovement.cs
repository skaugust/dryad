using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDryadMovement : MonoBehaviour
{
    private float speed = 1.0f;

    public SpriteMask natureMask;
    public SpriteMask pollutionMask;

    private Texture2D natureMaskTexture;
    private Texture2D pollutionMaskTexture;

    void Start()
    {
        natureMaskTexture = natureMask.sprite.texture;
        pollutionMaskTexture = pollutionMask.sprite.texture;

        Fill(natureMaskTexture, Color.clear);
        Fill(pollutionMaskTexture, Color.white);
    }

    private void Fill(Texture2D texture, Color32 color)
    {
        Color32[] clearColorArray = new Color32[texture.width * texture.height];
        for (int x = 0; x < clearColorArray.Length; x++)
        {
            clearColorArray[x] = color;
        }
        texture.SetPixels32(clearColorArray);
        texture.Apply();
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ColorTextureMasks(50);
        }
        else
        {
            ColorTextureMasks(5);
        }
        natureMaskTexture.Apply();
        pollutionMaskTexture.Apply();
    }

    private void ColorTextureMasks(int radius)
    {
        for (int i = -radius; i < radius; i++)
        {
            for (int j = -radius; j < radius; j++)
            {
                if (Mathf.Sqrt(i * i + j * j) <= radius)
                {
                    int x = i + (int)((this.transform.position.x + 5) * 100);
                    int y = j + (int)((this.transform.position.y + 5) * 100);
                    natureMaskTexture.SetPixel(x, y, Color.white);
                    pollutionMaskTexture.SetPixel(x, y, Color.clear);
                }
            }
        }
    }
}
