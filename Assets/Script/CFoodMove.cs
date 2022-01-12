using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFoodMove : MonoBehaviour
{
    public float coinSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        {
            Vector3 moveVelocity = Vector3.zero;
            moveVelocity = new Vector3(-0.12f, 0, 0);
            transform.position += moveVelocity * coinSpeed * Time.deltaTime;
        }
    }
}
