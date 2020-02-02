using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceManager : MonoBehaviour
{
    public GameObject dryad;

    public AutoTiledMask shortGrassMask;
    public AutoTiledMask longGrassMask;
    public AutoTiledMask pollutionMask;

    public GameObject factoryParentGroup;
    [System.NonSerialized]
    public List<Transform> factoryList = new List<Transform>();
    public GameObject pollutionSpawnParentGroup;
    [System.NonSerialized]
    public List<Transform> pollutionSpawnList = new List<Transform>();

    Dictionary<MaskType, AutoTiledMask> maskMap;
    public enum MaskType
    {
        ShortGrass, LongGrass, Pollution
    }

    private Dictionary<Vector2Int, BalanceTileModel> balanceMap = new Dictionary<Vector2Int, BalanceTileModel>();

    private int nextUpdateBucket = 0;
    private const int NUM_BUCKETS = 500;
    private List<List<BalanceTileModel>> bucketedModelsForUpdates = new List<List<BalanceTileModel>>();

    public float ADJACENT_TILE_MODIFIER = 10;
    public float CLOSE_TILE_MODIFIER = 4;
    public float NEAR_BY_TILE_MODIFIER = 1;
    public float DRYAD_STANDING_MODIFIER = 5;

    private const int TILES_TO_INIT = 100;

    void Start()
    {
        maskMap = new Dictionary<MaskType, AutoTiledMask>();
        maskMap.Add(MaskType.ShortGrass, shortGrassMask);
        maskMap.Add(MaskType.LongGrass, longGrassMask);
        maskMap.Add(MaskType.Pollution, pollutionMask);

        foreach (Transform child in factoryParentGroup.transform)
        {
            factoryList.Add(child);
        }
        foreach (Transform child in pollutionSpawnParentGroup.transform)
        {
            pollutionSpawnList.Add(child);
        }

        for (int i = 0; i < NUM_BUCKETS; i++)
        {
            bucketedModelsForUpdates.Add(new List<BalanceTileModel>());
        }

        for (int i = -TILES_TO_INIT; i <= TILES_TO_INIT; i++)
        {
            for (int j = -TILES_TO_INIT; j <= TILES_TO_INIT; j++)
            {
                Vector2Int location = new Vector2Int(i, j);
                BalanceTileModel model = new BalanceTileModel(location);
                balanceMap[location] = model;
                int index = Random.Range(0, NUM_BUCKETS);
                bucketedModelsForUpdates[index].Add(model);
            }
        }

        foreach (KeyValuePair<Vector2Int, BalanceTileModel> pair in balanceMap)
        {
            pair.Value.init(this, getAdjacentTiles(pair.Key), getCloseTiles(pair.Key), getNearByTiles(pair.Key), factoryList, pollutionSpawnList);
        }
    }

    void FixedUpdate()
    {
        foreach (BalanceTileModel model in getTilesNearby(dryad.transform.position))
        {
            model.Update(this, DRYAD_STANDING_MODIFIER);
        }

        foreach (BalanceTileModel model in bucketedModelsForUpdates[nextUpdateBucket])
        {
            model.Update(this, 0);
        }
        nextUpdateBucket++;
        nextUpdateBucket %= NUM_BUCKETS;

        // These might have changed.
        foreach (AutoTiledMask mask in maskMap.Values)
        {
            mask.Apply();
        }
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
                BalanceTileModel tile = GetTileByLocation(new Vector2Int(i, j));
                if (tile != null)
                {
                    list.Add(tile);
                }
            }
        }
        return list;
    }

    public BalanceTileModel GetTileByLocation(Vector2Int location)
    {
        if (balanceMap.ContainsKey(location))
        {
            return balanceMap[location];
        }
        else
        {
            return null;
        }
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

    public Vector2 TileToWorldCoords(Vector2Int location)
    {
        return new Vector2(location.x / BalanceTileModel.TILES_PER_GAME_UNIT, location.y / BalanceTileModel.TILES_PER_GAME_UNIT);
    }

    // |center| should be in game coordinates.
    public void ColorTextureMasks(Vector2 center, int radius, MaskType maskType)
    {
        AutoTiledMask mask = maskMap[maskType];
        int relativeX = (int)((center.x + 1.25f) * 100);
        int relativeY = (int)((center.y + 1.25f) * 100);
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (Mathf.Sqrt(i * i + j * j) <= radius)
                {
                    int x = i + relativeX;
                    int y = j + relativeY;
                    mask.SetPixel(x, y, Color.white);
                }
            }
        }
    }
}
