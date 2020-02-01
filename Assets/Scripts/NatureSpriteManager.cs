using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureSpriteManager : MonoBehaviour
{
    public GameObject natureSpitePrefab;
    public GameObject dryad;

    // Init to Int32.MinValue because Vector2Int doesn't do nulls.
    private Vector2Int previousDryadLocation = new Vector2Int(Int32.MinValue, Int32.MinValue);

    private int boxRadius = 15;
    Dictionary<Vector2Int, GameObject> natureMap = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
    }

    void Update()
    {
        bool madeMoreNatureFlag = false;
        Vector2Int current = new Vector2Int((int)dryad.gameObject.transform.position.x, (int)dryad.gameObject.transform.position.y);
        if (current != previousDryadLocation)
        {
            for (int i = -boxRadius; i <= boxRadius; i++)
            {
                for (int j = -boxRadius; j <= boxRadius; j++)
                {
                    Vector2Int location = new Vector2Int(i + current.x, j + current.y);
                    if (!natureMap.ContainsKey(location))
                    {
                        GameObject newNature = GameObject.Instantiate(natureSpitePrefab);
                        newNature.SetActive(true);
                        newNature.transform.position = new Vector3(location.x, location.y, 0);
                        newNature.transform.parent = gameObject.transform;
                        newNature.GetComponent<SpriteRenderer>().color = new Color(UnityEngine.Random.Range(0f, 0.2f), UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0f, 0.4f), 1f);
                        natureMap.Add(location, newNature);
                        madeMoreNatureFlag = true;
                    }
                }
            }

            previousDryadLocation = current;
        }
        if (madeMoreNatureFlag)
        {
            // print("Nature Count: " + this.transform.childCount);
        }
    }
}
