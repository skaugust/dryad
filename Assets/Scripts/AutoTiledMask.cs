using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTiledMask : MonoBehaviour
{
    public SpriteMask maskPrefab;
    private List<Texture2D> maskMap = new List<Texture2D>();
    private List<bool> dirty = new List<bool>();

    public Color initColor;

    private const int MASK_SIZE = 250;

    private const int MAX_TILE_ONE_AXIS = 30;
    private const int OFFSET = MAX_TILE_ONE_AXIS / 2;
    private int key(int x, int y)
    {
        return x + OFFSET + ((y + OFFSET) * MAX_TILE_ONE_AXIS);
    }

    void Start()
    {
        for (int i = 0; i < MAX_TILE_ONE_AXIS * MAX_TILE_ONE_AXIS; i++)
        {
            maskMap.Add(null);
            dirty.Add(false);
        }

        // Switch these for release/demo. Start up will be ~25 seconds, but should have less gameplay lag.
        //int size = 12;
        int size = 0;
        for (int i = -size; i <= size; i++)
        {
            for (int j = -size; j <= size; j++)
            {
                initLocation(i, j);
            }
        }
    }

    // https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
    private static int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public void SetPixel(int x, int y, Color color)
    {
        int i = Mathf.FloorToInt(x / (float)MASK_SIZE);
        x = mod(x, MASK_SIZE);

        int j = Mathf.FloorToInt(y / (float)MASK_SIZE);
        y = mod(y, MASK_SIZE);

        // Seems to write on the edges of the texture and wrap around. This apparently doesn't result in any awkward lines.
        x = Mathf.Clamp(x, 1, MASK_SIZE - 2);
        y = Mathf.Clamp(y, 1, MASK_SIZE - 2);

        int location = key(i, j);
        dirty[location] = true;
        if (maskMap[location] == null)
        {
            initLocation(i, j);
        }
        maskMap[location].SetPixel(x, y, color);
    }

    public void Apply()
    {
        for (int i = 0; i < dirty.Count; i++)
        {
            if (dirty[i])
            {
                maskMap[i].Apply();
            }
            dirty[i] = false;
        }
    }

    private void initLocation(int x, int y)
    {
        GameObject maskCopy = GameObject.Instantiate(maskPrefab.gameObject);
        maskCopy.transform.parent = this.transform;
        maskCopy.gameObject.SetActive(true);

        Texture2D theirTexture = maskPrefab.sprite.texture;
        Texture2D ourTexture = new Texture2D(MASK_SIZE, MASK_SIZE, theirTexture.format, false);
        maskMap[key(x, y)] = ourTexture;
        maskCopy.GetComponent<SpriteMask>().sprite = Sprite.Create(ourTexture, new Rect(new Vector2(0, 0), new Vector2(MASK_SIZE, MASK_SIZE)), new Vector2(.5f, .5f));
        maskCopy.transform.position += new Vector3(x * 2.5f, y * 2.5f);

        Fill(ourTexture);
    }

    private void Fill(Texture2D texture)
    {
        Color32[] clearColorArray = new Color32[texture.width * texture.height];
        for (int x = 0; x < clearColorArray.Length; x++)
        {
            clearColorArray[x] = initColor;
        }
        texture.SetPixels32(clearColorArray);
        texture.Apply();
    }
}
