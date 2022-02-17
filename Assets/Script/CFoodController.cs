using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFoodController : MonoBehaviour
{
    public GameObject food;
    public GameObject foodSet;
    private CFoodMove cFoodMove;

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
        Debug.Log("### Generate Food!!");
        SetFood();
        yield return new WaitForSeconds(1f);
        StartCoroutine("GenerateFood");
    }

    void SetFood()
    {
        GameObject _food = Instantiate(food, foodSet.transform.position + Vector3.left, Quaternion.identity);
        cFoodMove = _food.GetComponent<CFoodMove>();
        _food.transform.parent = foodSet.transform;
        _food.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        cFoodMove.Move();
    }
}
