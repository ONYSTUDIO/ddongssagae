using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFoodMove : MonoBehaviour
{
    public float foodSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Move();
    }

    public void Move()
    {
        Debug.Log("### Food Move!!");
        // Vector3 moveVelocity = Vector3.zero;
        // moveVelocity = new Vector3(-0.12f, 0, 0);
        // transform.position += moveVelocity * foodSpeed * Time.deltaTime;

        Rigidbody2D rigid = gameObject.GetComponent<Rigidbody2D>();
        rigid.AddForce(Vector2.left * 10, ForceMode2D.Impulse);

        if (transform.position.x < -9f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // 획득
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "BoarderBullet")
        {
            Debug.Log("### Destroy Food!!");
            Destroy(gameObject);
        }
    }
}
