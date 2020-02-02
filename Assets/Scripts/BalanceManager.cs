using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceManager : MonoBehaviour
{
    public GameObject dryad;
    public float mana = 10;
    private float lastChannelTick = 0;

    public GameObject treeGroup;
    public TreeTag treePrefab;

    public AutoTiledMask shortGrassMask;
    public AutoTiledMask longGrassMask;
    public AutoTiledMask pollutionMask;

    public List<FactoryTag> factoryList = new List<FactoryTag>();
    public List<PollutionTag> pollutionList = new List<PollutionTag>();
    public List<TreeTag> treeList = new List<TreeTag>();
    // TODO(sky): Replace with PureWaterTag after we implement this.
    public List<MonoBehaviour> pureWaterList = new List<MonoBehaviour>();
    public int globalPower = 1;

    Dictionary<MaskType, AutoTiledMask> maskMap;
    public enum MaskType
    {
        ShortGrass, LongGrass, Pollution
    }

    private Dictionary<Vector2Int, BalanceTileModel> balanceMap = new Dictionary<Vector2Int, BalanceTileModel>();

    private int nextUpdateBucket = 0;
    private const int NUM_BUCKETS = 500;
    private List<List<BalanceTileModel>> bucketedModelsForUpdates = new List<List<BalanceTileModel>>();

    public int ADJACENT_TILE_MODIFIER = 10;
    public int CLOSE_TILE_MODIFIER = 4;
    public int NEAR_BY_TILE_MODIFIER = 1;
    public int DRYAD_STANDING_MODIFIER = 5;
    public int dryadChannellingModifier = 2;

    private const int TILES_TO_INIT = 35;

    void Start()
    {
        maskMap = new Dictionary<MaskType, AutoTiledMask>();
        maskMap.Add(MaskType.ShortGrass, shortGrassMask);
        maskMap.Add(MaskType.LongGrass, longGrassMask);
        maskMap.Add(MaskType.Pollution, pollutionMask);

        factoryList = new List<FactoryTag>(GameObject.FindObjectsOfType<FactoryTag>());
        pollutionList = new List<PollutionTag>(GameObject.FindObjectsOfType<PollutionTag>());
        treeList = new List<TreeTag>(GameObject.FindObjectsOfType<TreeTag>());

        for (int i = 0; i < NUM_BUCKETS; i++)
        {
            bucketedModelsForUpdates.Add(new List<BalanceTileModel>());
        }

        for (int i = -TILES_TO_INIT; i <= TILES_TO_INIT; i++)
        {
            for (int j = -TILES_TO_INIT; j <= TILES_TO_INIT; j++)
            {
                Vector2Int location = new Vector2Int(i, j);
                BalanceTileModel model = new BalanceTileModel(location, this);
                balanceMap[location] = model;
                int index = UnityEngine.Random.Range(0, NUM_BUCKETS);
                bucketedModelsForUpdates[index].Add(model);
            }
        }

        foreach (KeyValuePair<Vector2Int, BalanceTileModel> pair in balanceMap)
        {
            pair.Value.init(getAdjacentTiles(pair.Key), getCloseTiles(pair.Key), getNearByTiles(pair.Key), this.factoryList, this.pollutionList, this.treeList);
        }
    }

    private void UpdateGlobalPower()
    {
        // World power = 1 + floor(0.1 * # of trees + .05 * # of pure water tiles)
        this.globalPower = 1 + UnityEngine.Mathf.FloorToInt(0.1f * treeList.Count + 0.05f * pureWaterList.Count);
    }

    public void MakeTree(Vector2Int location)
    {
        TreeTag newTree = GameObject.Instantiate(treePrefab);
        newTree.transform.position = TileToWorldCoords(location);
        newTree.transform.parent = treeGroup.transform;

        treeList.Add(newTree);
        foreach (KeyValuePair<Vector2Int, BalanceTileModel> pair in balanceMap)
        {
            pair.Value.UpdateTrees(this.treeList);
        }
    }

    public void RemoveFactory(FactoryTag factory)
    {
        factoryList.Remove(factory);
        foreach (KeyValuePair<Vector2Int, BalanceTileModel> pair in balanceMap)
        {
            pair.Value.UpdateFactories(this.factoryList);
        }
    }

    private const float TIME_STEP = 0.02f;
    private float carryOverTime = 0f;

    void Update()
    {
        // Using this instead of FixedUpdate, but trying to mirror the behavior.
        carryOverTime += Time.deltaTime;
        if (carryOverTime > TIME_STEP)
        {
            carryOverTime -= TIME_STEP;
        }
        else
        {
            return;
        }

        List<BalanceTileModel> underneathTiles = getTilesNearby(dryad.transform.position, 0);
        bool isChannelling = (Input.GetKey(KeyCode.Space) && Time.time > lastChannelTick + .25 && mana > 1);
        if (isChannelling)
        {
            lastChannelTick = Time.time;
            mana = mana - 1;
            int channelingMod = globalPower * (dryadChannellingModifier + 1);
            HashSet<BalanceTileModel> underneathTilesSet = new HashSet<BalanceTileModel>(underneathTiles);
            foreach (BalanceTileModel model in getTilesNearby(dryad.transform.position, 4).Where(t => !underneathTilesSet.Contains(t)))
            {
                model.Update(this, channelingMod);
            }
        }

        int standingMod = globalPower * (DRYAD_STANDING_MODIFIER + 1 + (isChannelling ? dryadChannellingModifier : 0));
        foreach (BalanceTileModel model in underneathTiles)
        {
            model.Update(this, standingMod);
        }

        // This could re-update channelled/stood upon tiles. Unclear if we care.
        foreach (BalanceTileModel model in bucketedModelsForUpdates[nextUpdateBucket])
        {
            model.Update(this, globalPower);
        }
        nextUpdateBucket++;
        nextUpdateBucket %= NUM_BUCKETS;

        // These might have changed.
        foreach (AutoTiledMask mask in maskMap.Values)
        {
            mask.Apply();
        }

        mana = mana + 0.01f;
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
        for (int i = -3; i <= 3; i++)
        {
            for (int j = -3; j <= 3; j++)
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
        for (int i = -5; i <= 5; i++)
        {
            for (int j = -5; j <= 5; j++)
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

    public List<BalanceTileModel> getTilesNearby(Vector2 gameCoordinates, int extend)
    {
        Vector2Int upper = new Vector2Int(Mathf.CeilToInt(gameCoordinates.x * BalanceTileModel.TILES_PER_GAME_UNIT) + extend, Mathf.CeilToInt(gameCoordinates.y * BalanceTileModel.TILES_PER_GAME_UNIT) + extend);
        Vector2Int lower = new Vector2Int(Mathf.FloorToInt(gameCoordinates.x * BalanceTileModel.TILES_PER_GAME_UNIT) - extend, Mathf.FloorToInt(gameCoordinates.y * BalanceTileModel.TILES_PER_GAME_UNIT) - extend);
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
        return balanceMap.ContainsKey(location) ? balanceMap[location] : null;
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
    public void ColorTextureMasks(Vector2 center, int radius, MaskType maskType, bool positive)
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
                    mask.SetPixel(x, y, positive ? Color.white : Color.clear);
                }
            }
        }
    }
}
