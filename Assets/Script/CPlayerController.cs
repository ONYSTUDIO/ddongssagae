using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CPlayerController : MonoBehaviour
{
    [SerializeField]
    public Button m_JumpBtn;

    [SerializeField]
    public Button m_ShootBtn;

    [SerializeField]
    public GameObject m_BulletNormal;

    private Rigidbody2D rb;
    private float width;
    //private GameObject m_AttackPrefab = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        m_JumpBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### Jump!!");
            BoxCollider2D backgroundCollider = GetComponent<BoxCollider2D>();
            width = backgroundCollider.size.x;

            Jump();
        });

        m_ShootBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### Shoot!!");
            Shoot();
        });
    }

    private void Jump()
    {
        rb.AddForce(new Vector3(0f, 300f, 0f));
    }

    private void Shoot()
    {
        //m_AttackPrefab = Resources.Load<GameObject>("Prefabs/Bullet_Normal");
        GameObject _bullet = Instantiate(m_BulletNormal, this.transform.position + Vector3.left, Quaternion.identity);
        Rigidbody2D rigid = _bullet.GetComponent<Rigidbody2D>();
        rigid.AddForce(Vector2.left * 10, ForceMode2D.Impulse);
    }
}
