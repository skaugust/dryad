using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTiledMask : MonoBehaviour
{
    public SpriteMask maskPrefab;
    private Dictionary<Vector2Int, Texture2D> maskMap = new Dictionary<Vector2Int, Texture2D>();
    private HashSet<Vector2Int> dirty = new HashSet<Vector2Int>();

    public Color initColor;

    void Start()
    {
    }

    void Update()
    {
        // Check for Camera movement, move.
    }

    public void SetPixel(int x, int y, Color color)
    {
        int i = 0;
        while (x < 0)
        {
            x += 1000;
            i--;
        }
        while (x >= 1000)
        {
            x -= 1000;
            i++;
        }

        int j = 0;
        while (y < 0)
        {
            y += 1000;
            j--;
        }
        while (y >= 1000)
        {
            y -= 1000;
            j++;
        }
        Vector2Int location = new Vector2Int(i, j);
        dirty.Add(location);
        if (!maskMap.ContainsKey(location))
        {
            initLocation(location);
        }
        maskMap[location].SetPixel(x, y, color);
    }

    public void Apply()
    {
        foreach (Vector2Int location in dirty)
        {
            maskMap[location].Apply();
        }
        dirty.Clear();
    }

    private void initLocation(Vector2Int location)
    {
        GameObject maskCopy = GameObject.Instantiate(maskPrefab.gameObject);
        maskCopy.transform.parent = this.transform;
        maskCopy.gameObject.SetActive(true);

        Texture2D theirTexture = maskPrefab.sprite.texture;
        Texture2D ourTexture = new Texture2D(theirTexture.width, theirTexture.height, theirTexture.format, false);
        maskMap[location] = ourTexture;
        maskCopy.GetComponent<SpriteMask>().sprite = Sprite.Create(ourTexture, new Rect(new Vector2(0, 0), new Vector2(ourTexture.width, ourTexture.height)), new Vector2(.5f, .5f));
        maskCopy.transform.position += new Vector3(location.x * 10, location.y * 10);

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
