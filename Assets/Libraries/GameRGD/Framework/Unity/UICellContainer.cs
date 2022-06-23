using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICellContainer : MonoBehaviour
{
    public GameObject cellPrefab;

    public GameObject GetInstance()
    {
        return Instantiate(cellPrefab, transform);
    }

    public void ClearContainer()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
