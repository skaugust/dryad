using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BalanceTileModel
{
    public enum Tier
    {
        DensePollution, LightPollution, Desolation, LightGrass, DenseGrass, TallGrass, FloweringGrass
    }

    public static int CalculateTierAffect(Tier tier)
    {
        switch (tier)
        {
            case Tier.DensePollution:
                return -8 + (int)Mathf.Floor(Time.time / 180);
            case Tier.LightPollution:
                return -4 + (int)Mathf.Floor(Time.time / 180);
            case Tier.Desolation:
                return 0;
            case Tier.LightGrass:
                return 1;
            case Tier.DenseGrass:
                return 2;
            case Tier.TallGrass:
                return 4;
            case Tier.FloweringGrass:
                return 6;
        }
        throw new System.Exception("Unknown tier, " + tier);
    }

    public Vector2Int location;
    public TerrainRenderFlags terrainRender;

    private List<BalanceTileModel> adjacentTiles;
    private List<BalanceTileModel> closeTiles;
    private List<BalanceTileModel> nearByTiles;

    private const float UNDERNEATH_DISTANCE = 0.5f;
    private List<Transform> undearneathFactory;
    private List<Transform> undearneathPollution;

    private const float CLOSE_DISTANCE = 2f;
    private List<Transform> closeFactory;
    private List<Transform> closePollution;

    private const float NEAR_BY_DISTANCE = 4f;
    private List<Transform> nearByFactory;
    private List<Transform> nearByPollution;
    private List<Transform> nearByTree;

    private Vector2 drawCenter;
    private Vector2 drawCenter2;
    private Vector2 drawCenter3;
    public Tier tier;

    public const float TILES_PER_GAME_UNIT = 1.2f;
    private const float TEXTURE_1000_UNIT_LENGTH = 50 / TILES_PER_GAME_UNIT;
    private readonly float TEXTURE_1000_HYPOTENUS = Mathf.Sqrt((TEXTURE_1000_UNIT_LENGTH * TEXTURE_1000_UNIT_LENGTH) + (TEXTURE_1000_UNIT_LENGTH * TEXTURE_1000_UNIT_LENGTH));

    private float ADJACENT_TILE_MODIFIER;
    private float CLOSE_TILE_MODIFIER;
    private float NEAR_BY_TILE_MODIFIER;

    private BalanceManager balanceManager;

    public BalanceTileModel(Vector2Int location, BalanceManager manager)
    {
        this.tier = Tier.Desolation;
        this.location = location;
        this.balanceManager = manager;
        Vector2 offset = UnityEngine.Random.insideUnitCircle * 0.07f;
        drawCenter = new Vector2(location.x / TILES_PER_GAME_UNIT, location.y / TILES_PER_GAME_UNIT) + offset;
        offset = UnityEngine.Random.insideUnitCircle * 0.5f;
        drawCenter2 = new Vector2(location.x / TILES_PER_GAME_UNIT, location.y / TILES_PER_GAME_UNIT) + offset;
        offset = UnityEngine.Random.insideUnitCircle * 0.5f;
        drawCenter3 = new Vector2(location.x / TILES_PER_GAME_UNIT, location.y / TILES_PER_GAME_UNIT) + offset;
    }

    public void init(List<BalanceTileModel> adjacentTiles, List<BalanceTileModel> closeTiles, List<BalanceTileModel> nearByTiles, List<FactoryTag> rangeFactory, List<PollutionTag> rangePollution, List<TreeTag> rangeTree)
    {
        this.adjacentTiles = adjacentTiles;
        this.closeTiles = closeTiles;
        this.nearByTiles = nearByTiles;

        UpdateFactories(rangeFactory);
        // Purposefully only register underneath on startup. Because we only remove factories, we skip this to avoid re-registering.
        foreach (Transform factoryTransform in this.undearneathFactory)
        {
            factoryTransform.gameObject.GetComponent<FactoryTag>().RegisterUnderneathTile(this);
        }

        this.nearByPollution = rangePollution.Select(t => t.transform).Where(f => Vector2.Distance(f.position, balanceManager.TileToWorldCoords(this.location)) < NEAR_BY_DISTANCE).ToList();
        this.closePollution = nearByPollution.Where(f => Vector2.Distance(f.position, balanceManager.TileToWorldCoords(this.location)) < CLOSE_DISTANCE).ToList();
        this.undearneathPollution = this.closePollution.Where(f => Vector2.Distance(f.position, balanceManager.TileToWorldCoords(this.location)) < UNDERNEATH_DISTANCE).ToList();

        UpdateTrees(rangeTree);

        ADJACENT_TILE_MODIFIER = balanceManager.ADJACENT_TILE_MODIFIER / Convert.ToSingle(adjacentTiles.Count);
        CLOSE_TILE_MODIFIER = balanceManager.CLOSE_TILE_MODIFIER / Convert.ToSingle(closeTiles.Count);
        NEAR_BY_TILE_MODIFIER = balanceManager.NEAR_BY_TILE_MODIFIER / Convert.ToSingle(nearByTiles.Count);
    }

    public void UpdateTrees(List<TreeTag> rangeTree)
    {
        this.nearByTree = rangeTree.Select(t => t.transform).Where(f => Vector2.Distance(f.position, balanceManager.TileToWorldCoords(this.location)) < NEAR_BY_DISTANCE).ToList();
    }

    public void UpdateFactories(List<FactoryTag> rangeFactory)
    {
        this.nearByFactory = rangeFactory.Select(t => t.transform).Where(f => Vector2.Distance(f.transform.position, balanceManager.TileToWorldCoords(this.location)) < NEAR_BY_DISTANCE).ToList();
        this.closeFactory = nearByFactory.Where(f => Vector2.Distance(f.position, balanceManager.TileToWorldCoords(this.location)) < CLOSE_DISTANCE).ToList();
        this.undearneathFactory = this.closeFactory.Where(f => Vector2.Distance(f.position, balanceManager.TileToWorldCoords(this.location)) < UNDERNEATH_DISTANCE).ToList();
    }

    private float CalcualteModifier(float extraModifier)
    {
        float modifier = extraModifier;

        foreach (BalanceTileModel other in adjacentTiles)
        {
            modifier += ADJACENT_TILE_MODIFIER * CalculateTierAffect(other.tier);
        }
        foreach (BalanceTileModel other in closeTiles)
        {
            modifier += CLOSE_TILE_MODIFIER * CalculateTierAffect(other.tier);
        }
        foreach (BalanceTileModel other in nearByTiles)
        {
            modifier += NEAR_BY_TILE_MODIFIER * CalculateTierAffect(other.tier);
        }

        if (undearneathPollution.Any())
        {
            modifier -= 20;
        }
        if (closePollution.Any())
        {
            modifier -= 9;
        }
        if (nearByPollution.Any())
        {
            modifier -= 3;
        }

        if (nearByFactory.Any())
        {
            if (modifier > 0)
            {
                modifier = modifier / 1.8f;
            }
        }
        if (closeFactory.Any())
        {
            if (modifier > 0)
            {
                modifier = modifier / 1.5f;
            }
        }

        if (nearByTree.Any())
        {
            modifier += 5;
        }

        // TODO(sky):
        // * Water

        return modifier;
    }

    private Tier CalculateTierFromModifier(float modifier)
    {
        float rand = UnityEngine.Random.Range(0f, 1f);

        float lightPollutionLower = -70;
        float desolationLower = -10;
        float lightGrassLower = 4;
        float denseGrassLower = 18;
        float tallGrassLower = 35;
        float floweringGrassLower = 60;

        if (modifier < lightPollutionLower)
        {
            if (this.tier == Tier.DensePollution)
            {
                return Tier.DensePollution;
            }
            if (modifier - lightPollutionLower + 20 < rand * 20)
            {
                return Tier.DensePollution;
            }
            return Tier.LightPollution;
        }
        else if (modifier < desolationLower) // Should be light pollution
        {
            if (this.tier == Tier.LightPollution)
            {
                return Tier.LightPollution;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.LightPollution)) // If you are below this tier
            {
                if (modifier - lightPollutionLower > rand * 20)
                {
                    return Tier.LightPollution;
                }
                return Tier.DensePollution;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.LightPollution)) // If you are above this tier
            {
                if (modifier - desolationLower + 10 < rand * 10)
                {
                    return Tier.LightPollution;
                }
                return Tier.Desolation;
            }
            return Tier.LightPollution; // Should never come up
        }
        else if (modifier < lightGrassLower) // Should be desolation
        {
            if (this.tier == Tier.Desolation)
            {
                return Tier.Desolation;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.Desolation)) // If you are below this tier
            {
                if (modifier - desolationLower > rand * 10)
                {
                    return Tier.Desolation;
                }
                return Tier.LightPollution;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.Desolation)) // If you are above this tier
            {
                if (modifier - lightGrassLower + 10 < rand * 10)
                {
                    return Tier.Desolation;
                }
                return Tier.LightGrass;
            }
            return Tier.Desolation; // Should never come up
        }
        else if (modifier < denseGrassLower) // Should be light grass
        {
            if (this.tier == Tier.LightGrass)
            {
                return Tier.LightGrass;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.LightGrass)) // If you are below this tier
            {
                if (modifier - lightGrassLower > rand * 5)
                {
                    return Tier.LightGrass;
                }
                return Tier.Desolation;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.LightGrass)) // If you are above this tier
            {
                if (modifier - denseGrassLower + 8 < rand * 8)
                {
                    return Tier.LightGrass;
                }
                return Tier.DenseGrass;
            }
            return Tier.LightGrass; // Should never come up
        }
        else if (modifier < tallGrassLower) // Should be dense grass
        {
            if (this.tier == Tier.DenseGrass)
            {
                return Tier.DenseGrass;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.DenseGrass)) // If you are below this tier
            {
                if (modifier - denseGrassLower > rand * 10)
                {
                    return Tier.DenseGrass;
                }
                return Tier.LightGrass;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.DenseGrass)) // If you are above this tier
            {
                if (modifier - tallGrassLower + 10 < rand * 10)
                {
                    return Tier.DenseGrass;
                }
                return Tier.TallGrass; // Don't need check since if it's currently above dense grass, must be legal tall grass location
            }
            return Tier.DenseGrass; // Should never come up
        }
        else if (modifier < floweringGrassLower) // Should be tall grass
        {
            if (this.tier == Tier.TallGrass)
            {
                return Tier.TallGrass;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.TallGrass)) // If you are below this tier
            {
                if (modifier - tallGrassLower > rand * 10)
                {
                    // TODO: CHECK IF TALL GRASS SPAWN WITHIN RANGE! If not, check if legal for this tile to become tall grass spawn point
                    return Tier.TallGrass;
                }
                return Tier.DenseGrass;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.TallGrass)) // If you are above this tier
            {
                if (modifier - floweringGrassLower + 10 < rand * 10)
                {
                    return Tier.TallGrass;
                }
                return Tier.FloweringGrass; // Don't need check since if it's currently above dense grass, must be legal tall grass location
            }
            return Tier.TallGrass; // Should never come up
        }
        else // Should be flowering grass
        {
            /* if (this.tier == Tier.FloweringGrass || this.tier == Tier.TallGrass)
             {
                 return Tier.FloweringGrass;         // If already in range, stay what you are
             }
             // TODO: CHECK IF TALL GRASS SPAWN WITHIN RANGE! If so, becomes flowering grass */
            return Tier.FloweringGrass;
        }
    }

    // extraModifier comes from Dryad's closeness, etc. TODO(sky): Pass in world power.
    public void Update(BalanceManager manager, float extraModifier, bool allowNegative)
    {
        float modifier = CalcualteModifier(extraModifier);
        if (modifier < 5 && !allowNegative) { return; }
        Tier newTier = CalculateTierFromModifier(modifier);

        if (capture == null)
        {
            capture = new MaskApplyCapture();
            capture.model = this;
            capture.manager = manager;
        }

        if (newTier == Tier.FloweringGrass && !nearByTree.Any())
        {
            balanceManager.MakeTree(this.location);
        }

        if (newTier != this.tier)
        {
            // If your new tier is lower than your current tier, we need to revert ourselves. This might remove mask changes our neighbors have performed. So re-apply them after that.
            // This also applies to switching sides, although magnitude cannot be trusted in that case.
            int newAffect = CalculateTierAffect(newTier);
            int oldAffect = CalculateTierAffect(this.tier);
            if (newAffect == 0 || Math.Abs(newAffect) < Math.Abs(oldAffect) || (newAffect * oldAffect) < 0)
            {
                capture.Apply(false);
                /*
                foreach (BalanceTileModel other in this.adjacentTiles)
                {
                    if (other.capture != null)
                    {
                        other.capture.Apply(true);
                    }
                }
                */
            }

            if (newTier == Tier.DensePollution)
            {
                capture.range1 = (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f));
                capture.range2 = 0;
                capture.range3 = 0;
                capture.maskType = BalanceManager.MaskType.Pollution;
            }
            else if (newTier == Tier.LightPollution)
            {
                capture.range1 = (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f));
                capture.range2 = UnityEngine.Random.Range(0f, 1f) > 0.5 ? (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)) : 0;
                capture.range3 = UnityEngine.Random.Range(0f, 1f) > 0.5 ? (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)) : 0;
                capture.maskType = BalanceManager.MaskType.Pollution;
            }
            else if (newTier == Tier.LightGrass)
            {
                capture.range1 = (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f));
                capture.range2 = UnityEngine.Random.Range(0f, 1f) > 0.5 ? (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)) : 0;
                capture.range3 = UnityEngine.Random.Range(0f, 1f) > 0.5 ? (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)) : 0;
                capture.maskType = BalanceManager.MaskType.ShortGrass;
            }
            else if (newTier == Tier.DenseGrass)
            {
                capture.range1 = (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f));
                capture.range2 = 0;
                capture.range3 = 0;
                capture.maskType = BalanceManager.MaskType.ShortGrass;
            }
            else if (newTier == Tier.TallGrass || newTier == Tier.FloweringGrass)
            {
                capture.range1 = (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f));
                capture.range2 = 0;
                capture.range3 = 0;
                capture.maskType = BalanceManager.MaskType.LongGrass;

                if (newTier == Tier.FloweringGrass)
                {
                    leaf = manager.MakeLeaf(location);
                }
            }
            else
            {
                capture.range1 = 0;
                capture.range2 = 0;
                capture.range3 = 0;
            }

            if (newTier != Tier.FloweringGrass && this.leaf != null)
            {
                GameObject.Destroy(leaf);
                this.leaf = null;
            }

            this.tier = newTier;
            capture.Apply(true);
        }
    }

    private GameObject leaf = null;

    // The capture is purposefully modifiable, to avoid GC hitches.
    private MaskApplyCapture capture;
    private class MaskApplyCapture
    {
        public BalanceTileModel model;
        public BalanceManager manager;

        public int range1;
        public int range2;
        public int range3;

        public BalanceManager.MaskType maskType;

        public void Apply(bool positive)
        {
            if (range1 > 0)
            {
                manager.ColorTextureMasks(model.drawCenter, range1, maskType, positive);
            }
            if (range2 > 0)
            {
                manager.ColorTextureMasks(model.drawCenter2, range2, maskType, positive);
            }
            if (range3 > 0)
            {
                manager.ColorTextureMasks(model.drawCenter3, range3, maskType, positive);
            }
        }
    }
}
