using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceTileModel
{
    private Vector2Int location;
    private Vector2 drawCenter;

    public const float TILES_PER_GAME_UNIT = 5;
    private const float TEXTURE_1000_UNIT_LENGTH = 50 / TILES_PER_GAME_UNIT;
    private float TEXTURE_1000_HYPOTENUS = Mathf.Sqrt((TEXTURE_1000_UNIT_LENGTH * TEXTURE_1000_UNIT_LENGTH) + (TEXTURE_1000_UNIT_LENGTH * TEXTURE_1000_UNIT_LENGTH));

    public BalanceTileModel(Vector2Int location)
    {
        this.location = location;
        Vector2 offset = Random.insideUnitCircle * 0.07f;
        drawCenter = new Vector2(location.x / TILES_PER_GAME_UNIT, location.y / TILES_PER_GAME_UNIT) + offset;
    }

    public void Update(BalanceManager manager)
    {
        // TODO(sky): We currently don't support removing anything from the mask, because neighbours need to be re-generated.
        int tier = Random.Range(-2, 3);
        if (tier == -1)
        {
            // Some amount of pollution.
        }
        else if (tier == 0)
        {
            // Desolate, no coverage by anything.
            manager.ColorTextureMasks(drawCenter, 0);
        }
        else if (tier == 1)
        {
            // Small coverage.
            manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * Random.Range(.2f, .4f)));
        }
        else if (tier == 2)
        {
            // Medium coverage.
            manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS * Random.Range(.5f, .8f)));
        }
        else if (tier == 3)
        {
            // Cover entirety of tile and then some. Radius of cirle should be equal to sqrt(2).
            manager.ColorTextureMasks(drawCenter, (int)(TEXTURE_1000_HYPOTENUS));
        }
    }
}
