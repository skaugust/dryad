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
    private Vector2 drawCenter;
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
    }


    public void init(List<BalanceTileModel> adjacentTiles, List<BalanceTileModel> closeTiles, List<BalanceTileModel> nearByTiles)
    {
        this.adjacentTiles = adjacentTiles;
        this.closeTiles = closeTiles;
        this.nearByTiles = nearByTiles;

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

        if (modifier < -80)
        {
            return Tier.DensePollution;
        }
        else if (modifier < -30)
        {
            return Tier.LightPollution;
        }
        else if (modifier < 5)
        {
            return Tier.Desolation;
        }
        else if (modifier < 15)
        {
            return Tier.LightGrass;
        }
        else if (modifier < 40)
        {
            return Tier.DenseGrass;
        }
        else if (modifier < 80)
        {
            return Tier.TallGrass;
        }
        else
        {
            return Tier.FloweringGrass;
        }
    }

    // extraModifier comes from Dryad's closeness, etc. TODO(sky): Pass in world power.
    public void Update(BalanceManager manager, float extraModifier)
    {
        float modifier = CalcualteModifier(extraModifier);
        Tier newTier = CalculateTierFromModifier(modifier);

        if (newTier != this.tier)
        {
            if (newTier == Tier.LightGrass)
            {
                manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(.4f, .8f)), BalanceManager.MaskType.ShortGrass);
            }
            else if (newTier == Tier.TallGrass)
            {
                manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.2f, 1.8f)), BalanceManager.MaskType.ShortGrass);
            }
            else if (newTier == Tier.DenseGrass || newTier == Tier.FloweringGrass)
            {
                manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * UnityEngine.Random.Range(1.2f, 1.8f)), BalanceManager.MaskType.LongGrass);
            }
            this.tier = newTier;
        }
    }
}
