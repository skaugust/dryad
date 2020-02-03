using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePowerScore : MonoBehaviour
{
    BalanceManager manager;
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("BalanceManager").GetComponent<BalanceManager>();
    }

    void Update()
    {
        gameObject.GetComponent<Text>().text = manager.globalPower.ToString();
    }
}
