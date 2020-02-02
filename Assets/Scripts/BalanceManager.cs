using System.Collections;
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

    private int nextUpdateBucket = 0;
    private const int NUM_BUCKETS = 500;
    private List<List<BalanceTileModel>> bucketedModelsForUpdates = new List<List<BalanceTileModel>>();

    public float ADJACENT_TILE_MODIFIER = 10;
    public float CLOSE_TILE_MODIFIER = 4;
    public float NEAR_BY_TILE_MODIFIER = 1;

    void Start()
    {
        natureMaskTexture = natureMask.sprite.texture;
        pollutionMaskTexture = pollutionMask.sprite.texture;

        Fill(natureMaskTexture, Color.clear);
        Fill(pollutionMaskTexture, Color.white);

        for (int i = 0; i < NUM_BUCKETS; i++)
        {
            bucketedModelsForUpdates.Add(new List<BalanceTileModel>());
        }

        for (int i = -100; i <= 100; i++)
        {
            for (int j = -100; j <= 100; j++)
            {
                Vector2Int location = new Vector2Int(i, j);
                BalanceTileModel model = new BalanceTileModel(location);
                balanceMap[location] = model;
                int index = Random.Range(0, NUM_BUCKETS);
                bucketedModelsForUpdates[index].Add(model);
            }
        }

        // TODO(sky): Could probably just iterate over the key/values in the map.
        for (int i = -100; i <= 100; i++)
        {
            for (int j = -100; j <= 100; j++)
            {
                Vector2Int location = new Vector2Int(i, j);
                balanceMap[location].init(getAdjacentTiles(location), getCloseTiles(location), getNearByTiles(location));
            }
        }
    }

    void FixedUpdate()
    {
        foreach (BalanceTileModel model in getTilesNearby(dryad.transform.position))
        {
            model.Update(this, 20);
        }

        foreach (BalanceTileModel model in bucketedModelsForUpdates[nextUpdateBucket])
        {
            model.Update(this, 0);
        }
        nextUpdateBucket++;
        nextUpdateBucket %= NUM_BUCKETS;

        // These might have changed.
        natureMaskTexture.Apply();
        pollutionMaskTexture.Apply();
    }

    public List<BalanceTileModel> getAdjacentTiles(Vector2Int location)
    {
        List<BalanceTileModel> result = new List<BalanceTileModel>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 || j != 0)
                {
                    Vector2Int target = new Vector2Int(location.x + i, location.y + j);
                    if (balanceMap.ContainsKey(target))
                    {
                        result.Add(balanceMap[target]);
                    }
                }
            }
        }
        return result;
    }

    public List<BalanceTileModel> getCloseTiles(Vector2Int location)
    {
        List<BalanceTileModel> result = new List<BalanceTileModel>();
        for (int i = -3; i <= -3; i++)
        {
            for (int j = -3; j <= -3; j++)
            {
                if (Mathf.Abs(i) >= 1 || Mathf.Abs(j) >= 1)
                {
                    Vector2Int target = new Vector2Int(location.x + i, location.y + j);
                    if (balanceMap.ContainsKey(target))
                    {
                        result.Add(balanceMap[target]);
                    }
                }
            }
        }
        return result;
    }

    public List<BalanceTileModel> getNearByTiles(Vector2Int location)
    {
        List<BalanceTileModel> result = new List<BalanceTileModel>();
        for (int i = -5; i <= -5; i++)
        {
            for (int j = -5; j <= -5; j++)
            {
                if (Mathf.Abs(i) >= 3 || Mathf.Abs(j) >= 3)
                {
                    Vector2Int target = new Vector2Int(location.x + i, location.y + j);
                    if (balanceMap.ContainsKey(target))
                    {
                        result.Add(balanceMap[target]);
                    }
                }
            }
        }
        return result;
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
