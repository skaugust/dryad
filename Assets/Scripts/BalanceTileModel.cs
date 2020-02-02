using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BalanceTileModel
{
    public enum Tier
    {
        DensePollution, LightPollution, Desolation, LightGrass, DenseGrass, TallGrass, FloweringGrass
    }

    private static int CalculateTierAffect(Tier tier)
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

    private List<Transform> rangeFactory;
    private List<Transform> rangePollution;

    private Vector2 drawCenter;
    private Vector2 drawCenter2;
    private Vector2 drawCenter3;
    private Tier tier;

    public const float TILES_PER_GAME_UNIT = 5;
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

    public void init(List<BalanceTileModel> adjacentTiles, List<BalanceTileModel> closeTiles, List<BalanceTileModel> nearByTiles, List<Transform> rangeFactory, List<Transform> rangePollution)
    {
        this.adjacentTiles = adjacentTiles;
        this.closeTiles = closeTiles;
        this.nearByTiles = nearByTiles;
        this.rangeFactory = rangeFactory;
        this.rangePollution = rangePollution;

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

        // TODO(sky):
        // * Water
        // * Tree
        // * Pollution source
        // * Factory

        return modifier;
    }

    private Tier CalculateTierFromModifier(float modifier)
    {
        /*
        So reasonable bounds are -143 to + 123
        So like heavy pollution should be -80 or so
        Light pollution is -30 to -80
        Light grass is 5-15
        Dense grass is 15-40
        Tall grass is 40-80
        Flowering tall grass is 80+
        */
        float rand = UnityEngine.Random.Range(0f, 1f);

        if (modifier < -80)
        {
            if (this.tier == Tier.DensePollution)
            {
                return Tier.DensePollution;
            }
            if (modifier + 100 < rand * 20)
            {
                return Tier.DensePollution;
            }
            return Tier.LightPollution;
        }
        else if (modifier < -30) // Should be light pollution
        {
            if (this.tier == Tier.LightPollution)
            {
                return Tier.LightPollution;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.LightPollution)) // If you are below this tier
            {
                if (modifier + 80 > rand * 20)
                {
                    return Tier.LightPollution;
                }
                return Tier.DensePollution;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.LightPollution)) // If you are above this tier
            {
                if (modifier + 50 < rand * 20)
                {
                    return Tier.LightPollution;
                }
                return Tier.Desolation;
            }
            return Tier.LightPollution; // Should never come up
        }
        else if (modifier < 5) // Should be desolation
        {
            if (this.tier == Tier.Desolation)
            {
                return Tier.Desolation;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.Desolation)) // If you are below this tier
            {
                if (modifier + 30 > rand * 20)
                {
                    return Tier.Desolation;
                }
                return Tier.LightPollution;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.Desolation)) // If you are above this tier
            {
                if (modifier + 5 < rand * 10)
                {
                    return Tier.Desolation;
                }
                return Tier.LightGrass;
            }
            return Tier.Desolation; // Should never come up
        }
        else if (modifier < 15) // Should be light grass
        {
            if (this.tier == Tier.LightGrass)
            {
                return Tier.LightGrass;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.LightGrass)) // If you are below this tier
            {
                if (modifier - 5 > rand * 5)
                {
                    return Tier.LightGrass;
                }
                return Tier.Desolation;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.LightGrass)) // If you are above this tier
            {
                if (modifier - 10 < rand * 5)
                {
                    return Tier.LightGrass;
                }
                return Tier.DenseGrass;
            }
            return Tier.LightGrass; // Should never come up
        }
        else if (modifier < 40) // Should be dense grass
        {
            if (this.tier == Tier.DenseGrass)
            {
                return Tier.DenseGrass;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.DenseGrass)) // If you are below this tier
            {
                if (modifier - 15 > rand * 10)
                {
                    return Tier.DenseGrass;
                }
                return Tier.LightGrass;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.DenseGrass)) // If you are above this tier
            {
                if (modifier - 30 < rand * 10)
                {
                    return Tier.DenseGrass;
                }
                return Tier.TallGrass; // Don't need check since if it's currently above dense grass, must be legal tall grass location
            }
            return Tier.DenseGrass; // Should never come up
        }
        else if (modifier < 80) // Should be tall grass
        {
            if (this.tier == Tier.TallGrass)
            {
                return Tier.TallGrass;         // If already in range, stay what you are
            }
            if (CalculateTierAffect(this.tier) < CalculateTierAffect(Tier.TallGrass)) // If you are below this tier
            {
                if (modifier - 40 > rand * 10)
                {
                    // TODO: CHECK IF TALL GRASS SPAWN WITHIN RANGE! If not, check if legal for this tile to become tall grass spawn point
                    return Tier.TallGrass;
                }
                return Tier.DenseGrass;
            }
            if (CalculateTierAffect(this.tier) > CalculateTierAffect(Tier.TallGrass)) // If you are above this tier
            {
                if (modifier - 70 < rand * 10)
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
            if (newTier == Tier.DensePollution || newTier == Tier.LightPollution)
            {
                manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f)), BalanceManager.MaskType.Pollution);
            }
            else if (newTier == Tier.LightGrass)
            {
                manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.ShortGrass);
                if (UnityEngine.Random.Range(0f, 1f) > 0.5)
                {
                    manager.ColorTextureMasks(drawCenter2, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.ShortGrass);
                }
                if (UnityEngine.Random.Range(0f, 1f) > 0.5)
                {
                    manager.ColorTextureMasks(drawCenter2, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.3f, .6f)), BalanceManager.MaskType.ShortGrass);
                }
            }
            else if (newTier == Tier.TallGrass)
            {
                manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f)), BalanceManager.MaskType.ShortGrass);
            }
            else if (newTier == Tier.DenseGrass || newTier == Tier.FloweringGrass)
            {
                manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.1f, 1.3f)), BalanceManager.MaskType.LongGrass);
            }

            this.tier = newTier;
        }
    }
}
