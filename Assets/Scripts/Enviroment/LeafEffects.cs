using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leaf_Effects : MonoBehaviour
{

    public GameObject[] prefabs;

    public bool ShowEffect01 = false;
    public bool ShowEffect02 = false;

    public void Start()
    {
        if (ShowEffect01) ShowEffect1();
        if (ShowEffect02) ShowEffect2();
    }
    public void ShowEffect1()
    {
        prefabs[0].SetActive(true);
    }


    public void ShowEffect2()
    {
        prefabs[1].SetActive(true);
    }

}
