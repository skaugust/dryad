using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manabar : MonoBehaviour
{
    private const int MAX_MANA_SCALE = 1;
    private const int MAX_MANA_VALUE = 10;

    private BalanceManager manaSource;

    void Start()
    {
        manaSource = FindObjectOfType<BalanceManager>();
    }

    void Update()
    {
        float percent = manaSource.mana / MAX_MANA_VALUE;
        float manaScale = percent * MAX_MANA_SCALE;
        this.transform.localScale = new Vector3(manaScale, this.transform.localScale.y, 1);
    }
}
