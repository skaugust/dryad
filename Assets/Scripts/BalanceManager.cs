﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceManager : MonoBehaviour
{
    public GameObject dryad;
    public SpriteMask natureMask;
    public SpriteMask pollutionMask;

    private Texture2D natureMaskTexture;
    private Texture2D pollutionMaskTexture;

    private Dictionary<Vector2Int, BalanceTileModel> balanceMap = new Dictionary<Vector2Int, BalanceTileModel>();

    void Start()
    {
        natureMaskTexture = natureMask.sprite.texture;
        pollutionMaskTexture = pollutionMask.sprite.texture;

        Fill(natureMaskTexture, Color.clear);
        Fill(pollutionMaskTexture, Color.white);

        for (int i = -100; i <= 100; i++)
        {
            for (int j = -100; j <= 100; j++)
            {
                Vector2Int location = new Vector2Int(i, j);
                balanceMap[location] = new BalanceTileModel(location);
            }
        }
    }

    void FixedUpdate()
    {
        // TODO(sky): Also update 1/600th of the tiles.

        foreach (BalanceTileModel model in getTilesNearby(dryad.transform.position))
        {
            model.Update(this);
        }

        // These might have changed.
        natureMaskTexture.Apply();
        pollutionMaskTexture.Apply();
    }

    public List<BalanceTileModel> getTilesNearby(Vector2 gameCoordinates)
    {
        Vector2Int upper = new Vector2Int(Mathf.CeilToInt(gameCoordinates.x * BalanceTileModel.TILES_PER_GAME_UNIT), Mathf.CeilToInt(gameCoordinates.y * BalanceTileModel.TILES_PER_GAME_UNIT));
        Vector2Int lower = new Vector2Int(Mathf.FloorToInt(gameCoordinates.x * BalanceTileModel.TILES_PER_GAME_UNIT), Mathf.FloorToInt(gameCoordinates.y * BalanceTileModel.TILES_PER_GAME_UNIT));
        List<BalanceTileModel> list = new List<BalanceTileModel>();
        for (int i = lower.x; i <= upper.x; i++)
        {
            for (int j = lower.y; j <= upper.y; j++)
            {
                list.Add(getTileByLocation(new Vector2Int(i, j)));
            }
        }
        return list;
    }

    public BalanceTileModel getTileByLocation(Vector2Int location)
    {
        return balanceMap[location];
    }

    public void Fill(Texture2D texture, Color32 color)
    {
        Color32[] clearColorArray = new Color32[texture.width * texture.height];
        for (int x = 0; x < clearColorArray.Length; x++)
        {
            clearColorArray[x] = color;
        }
        texture.SetPixels32(clearColorArray);
        texture.Apply();
    }

    // |center| should be in game coordinates.
    public void ColorTextureMasks(Vector2 center, int radius)
    {
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (Mathf.Sqrt(i * i + j * j) <= radius)
                {
                    int x = i + (int)((center.x + 5) * 100);
                    int y = j + (int)((center.y + 5) * 100);
                    natureMaskTexture.SetPixel(x, y, Color.white);
                    pollutionMaskTexture.SetPixel(x, y, Color.clear);
                }
            }
        }
    }
}
