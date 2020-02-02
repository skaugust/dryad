using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBodyController : MonoBehaviour
{
    WaterBodyPurityStatus state;
    public enum WaterBodyPurityStatus
    {
        PURE = 0,
        DIRTY = 1,
        POLLUTED = 2
    }
    private Dictionary<WaterBodyPurityStatus, SortingLayer> layerMap;

    // Start is called before the first frame update
    void Start()
    {
        SortingLayer pureLayer = Array.Find(SortingLayer.layers, layer => (layer as SortingLayer?).Value.name == "WaterPure");
        SortingLayer dirtyLayer = Array.Find(SortingLayer.layers, layer => (layer as SortingLayer?).Value.name == "WaterDirty");
        SortingLayer pollutionLayer = Array.Find(SortingLayer.layers, layer => (layer as SortingLayer?).Value.name == "WaterPolluted");

        layerMap = new Dictionary<WaterBodyPurityStatus, SortingLayer>() {
            {WaterBodyPurityStatus.PURE, pureLayer},
            {WaterBodyPurityStatus.DIRTY, dirtyLayer},
            {WaterBodyPurityStatus.POLLUTED, pollutionLayer}
        };
        Debug.Log(pureLayer.name);
    }

    // Update is called once per frame
    void Update()
    {
        IEnumerator iter = transform.GetEnumerator();
        iter.Reset();

        bool isFullyPure = true;
        bool isPartiallyPure = false;

        while (iter.MoveNext())
        {
            Transform t = iter.Current as Transform;
            isFullyPure = isFullyPure && t.GetComponent<WaterBodyPurityTrigger>().pure;
            isPartiallyPure = isPartiallyPure || t.GetComponent<WaterBodyPurityTrigger>().pure;
        }

        state = isFullyPure ? WaterBodyPurityStatus.PURE :
            isPartiallyPure ? WaterBodyPurityStatus.DIRTY :
            WaterBodyPurityStatus.POLLUTED;

        transform.GetComponent<SpriteMask>().frontSortingLayerID = layerMap[state].id;
    }
}
