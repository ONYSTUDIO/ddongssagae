using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFoodController : MonoBehaviour
{
    public GameObject food;
    public GameObject foodSet;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("GenerateFood");
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GenerateFood()
    {
        SetFood();
        yield return new WaitForSeconds(10f);
        StartCoroutine("GenerateFood");
    }

    void SetFood()
    {
        GameObject _food = Instantiate(food, foodSet.transform.position, Quaternion.identity);
        _food.transform.parent = foodSet.transform;
    }
}
