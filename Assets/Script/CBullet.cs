using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBullet : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BoarderBullet")
        {
            Debug.Log("### Destroy Bullet!!");
            Destroy(gameObject);
        }
    }
}
