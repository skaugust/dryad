﻿using System.Collections;
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
                return -8;
            case Tier.LightPollution:
                return -4;
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

    public BalanceTileModel(Vector2Int location)
    {
        this.tier = Tier.Desolation;
        this.location = location;
        Vector2 offset = UnityEngine.Random.insideUnitCircle * 0.07f;
        drawCenter = new Vector2(location.x / TILES_PER_GAME_UNIT, location.y / TILES_PER_GAME_UNIT) + offset;
        offset = UnityEngine.Random.insideUnitCircle * 0.5f;
        drawCenter2 = new Vector2(location.x / TILES_PER_GAME_UNIT, location.y / TILES_PER_GAME_UNIT) + offset;
        offset = UnityEngine.Random.insideUnitCircle * 0.5f;
        drawCenter3 = new Vector2(location.x / TILES_PER_GAME_UNIT, location.y / TILES_PER_GAME_UNIT) + offset;
    }

    public void init(BalanceManager manager, List<BalanceTileModel> adjacentTiles, List<BalanceTileModel> closeTiles, List<BalanceTileModel> nearByTiles, List<Transform> rangeFactory, List<Transform> rangePollution, List<Transform> rangeTree)
    {
        this.adjacentTiles = adjacentTiles;
        this.closeTiles = closeTiles;
        this.nearByTiles = nearByTiles;

        this.nearByFactory = rangeFactory.Where(f => Vector2.Distance(f.position, manager.TileToWorldCoords(this.location)) < NEAR_BY_DISTANCE).ToList();
        this.closeFactory = nearByFactory.Where(f => Vector2.Distance(f.position, manager.TileToWorldCoords(this.location)) < CLOSE_DISTANCE).ToList();
        this.undearneathFactory = this.closeFactory.Where(f => Vector2.Distance(f.position, manager.TileToWorldCoords(this.location)) < UNDERNEATH_DISTANCE).ToList();

        this.nearByPollution = rangePollution.Where(f => Vector2.Distance(f.position, manager.TileToWorldCoords(this.location)) < NEAR_BY_DISTANCE).ToList();
        this.closePollution = nearByPollution.Where(f => Vector2.Distance(f.position, manager.TileToWorldCoords(this.location)) < CLOSE_DISTANCE).ToList();
        this.undearneathPollution = this.closePollution.Where(f => Vector2.Distance(f.position, manager.TileToWorldCoords(this.location)) < UNDERNEATH_DISTANCE).ToList();

        this.nearByTree = rangeTree.Where(f => Vector2.Distance(f.position, manager.TileToWorldCoords(this.location)) < NEAR_BY_DISTANCE).ToList();

        balanceManager = GameObject.FindObjectOfType<BalanceManager>();
        ADJACENT_TILE_MODIFIER = balanceManager.ADJACENT_TILE_MODIFIER / Convert.ToSingle(adjacentTiles.Count);
        CLOSE_TILE_MODIFIER = balanceManager.CLOSE_TILE_MODIFIER / Convert.ToSingle(closeTiles.Count);
        NEAR_BY_TILE_MODIFIER = balanceManager.NEAR_BY_TILE_MODIFIER / Convert.ToSingle(nearByTiles.Count);
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
            modifier -= 8;
        }
        if (nearByPollution.Any())
        {
            modifier -= 3;
        }

        if (nearByFactory.Any())
        {
            if (modifier > 0)
            {
                modifier = modifier / 2;
            }
        }
        if (closeFactory.Any())
        {
            if (modifier > 0)
            {
                modifier = modifier / 2;
            }
        }

        if (nearByTree.Any())
        {
            modifier += 3;
        }

        // TODO(sky):
        // * Water

        return modifier;
    }

    private Tier CalculateTierFromModifier(float modifier)
    {
        float rand = UnityEngine.Random.Range(0f, 1f);

        float lightPollutionLower = -60;
        float desolationLower = -15;
        float lightGrassLower = 5;
        float denseGrassLower = 20;
        float tallGrassLower = 40;
        float floweringGrassLower = 80;

        if (modifier < lightPollutionLower)
        {
            if (this.tier == Tier.DensePollution)
            {
                return Tier.DensePollution;
            }
            if (modifier + 90 < rand * 10)
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
                if (modifier - lightPollutionLower > rand * 10)
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
                if (modifier - denseGrassLower + 5 < rand * 5)
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
            if (this.tier == Tier.FloweringGrass || this.tier == Tier.TallGrass)
            {
                return Tier.FloweringGrass;         // If already in range, stay what you are
            }
            // TODO: CHECK IF TALL GRASS SPAWN WITHIN RANGE! If so, becomes flowering grass
            return Tier.DenseGrass;
        }
    }

    // extraModifier comes from Dryad's closeness, etc. TODO(sky): Pass in world power.
    public void Update(BalanceManager manager, float extraModifier)
    {
        float modifier = CalcualteModifier(extraModifier);
        Tier newTier = CalculateTierFromModifier(modifier);

        if (newTier != this.tier)
        {
            // If your new tier is lower than your current tier, we need to revert ourselves. This might remove mask changes our neighbors have performed. So re-apply them after that.
            // This also applies to switching sides, although magnitude cannot be trusted in that case.
            int newAffect = CalculateTierAffect(newTier);
            int oldAffect = CalculateTierAffect(this.tier);
            if (newAffect == 0 || Math.Abs(newAffect) < Math.Abs(oldAffect) || (newAffect * oldAffect) < 0)
            {
                RunColorTextureCallbacks(false);
                reverse1 = null;
                reverse2 = null;
                reverse3 = null;
                foreach (BalanceTileModel other in this.adjacentTiles)
                {
                    other.RunColorTextureCallbacks(true);
                }
            }

            if (newTier == Tier.DensePollution)
            {
                reverse1 = manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f)), BalanceManager.MaskType.Pollution, true);
            }
            else if (newTier == Tier.LightPollution)
            {
                reverse1 = manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.Pollution, true);
                if (UnityEngine.Random.Range(0f, 1f) > 0.5)
                {
                    reverse2 = manager.ColorTextureMasks(drawCenter2, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.Pollution, true);
                }
                if (UnityEngine.Random.Range(0f, 1f) > 0.5)
                {
                    reverse3 = manager.ColorTextureMasks(drawCenter3, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.Pollution, true);
                }
            }
            else if (newTier == Tier.LightGrass)
            {
                reverse1 = manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.ShortGrass, true);
                if (UnityEngine.Random.Range(0f, 1f) > 0.5)
                {
                    reverse2 = manager.ColorTextureMasks(drawCenter2, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.ShortGrass, true);
                }
                if (UnityEngine.Random.Range(0f, 1f) > 0.5)
                {
                    reverse3 = manager.ColorTextureMasks(drawCenter3, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.ShortGrass, true);
                }
            }
            else if (newTier == Tier.DenseGrass)
            {
                reverse1 = manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f)), BalanceManager.MaskType.ShortGrass, true);
            }
            else if (newTier == Tier.TallGrass || newTier == Tier.FloweringGrass)
            {
                reverse1 = manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f)), BalanceManager.MaskType.LongGrass, true);
            }

            this.tier = newTier;
        }
    }

    private void RunColorTextureCallbacks(bool positive)
    {
        if (reverse1 != null)
        {
            reverse1.Invoke(positive);
        }
        if (reverse2 != null)
        {
            reverse2.Invoke(positive);
        }
        if (reverse3 != null)
        {
            reverse3.Invoke(positive);
        }
    }

    private Action<bool> reverse1 = null;
    private Action<bool> reverse2 = null;
    private Action<bool> reverse3 = null;
}
