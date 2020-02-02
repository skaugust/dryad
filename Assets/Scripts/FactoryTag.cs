using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryTag : MonoBehaviour
{
    public GameObject broken;
    public GameObject healed;

    private List<BalanceTileModel> tileList = new List<BalanceTileModel>();
    public void RegisterUnderneathTile(BalanceTileModel tile)
    {
        tileList.Add(tile);
    }

    void Update()
    {
        int count = 0;
        int sum = 0;
        foreach (BalanceTileModel tile in tileList)
        {
            count++;
            sum += BalanceTileModel.CalculateTierAffect(tile.tier);
        }

        if (sum / (float)count > 1.7f)
        {
            GameObject.FindObjectOfType<BalanceManager>().RemoveFactory(this);
            if (healed != null && broken != null)
            {
                healed.SetActive(true);
                broken.SetActive(false);
            }
            else
            {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
