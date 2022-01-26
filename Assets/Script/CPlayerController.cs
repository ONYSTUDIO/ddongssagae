using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static CConstants;

public class CPlayerController : MonoBehaviour
{
    [SerializeField]
    public GameObject m_Player;

    [SerializeField]
    public Button m_JumpBtn;

    [SerializeField]
    public Button m_ShootBtn;

    [SerializeField]
    public GameObject m_BulletNormal;

    public static int playerType;
    private Rigidbody2D rb;
    private float jumpPower = 5.0f;
    private bool isJumping = false;
    private SpriteRenderer spriteRenderer;
    //private float width; // 이거 어디에 쓰지..??
    //private GameObject m_AttackPrefab = null;

    // public void Setup()
    // {
    //     spriteRenderer = m_Player.GetComponent<SpriteRenderer>();
    //     spriteRenderer.sprite = CConstants.PLAYER_TYPE;
    // }

    // void Awake() {
    //     Debug.Log("### 1) Select : " + CConstants.PLAYER_TYPE);
    //     Debug.Log("### 2) Select : " + CConstants.GetResourcesPath(playerType));
    //     playerType = CConstants.PLAYER_TYPE;
    //     //Setup();
    //     spriteRenderer = m_Player.GetComponent<SpriteRenderer>();
    //     spriteRenderer.sprite = Resources.Load<Sprite>(CConstants.GetResourcesPath(playerType)) as Sprite;
    // }
    void Start()
    {
        Debug.Log("### 1) Select : " + CConstants.PLAYER_TYPE);
        Debug.Log("### 2) Select : " + CConstants.GetResourcesPath(playerType));
        playerType = CConstants.PLAYER_TYPE;
        // //Setup();
        spriteRenderer = m_Player.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>(CConstants.GetResourcesPath(playerType)) as Sprite;

        rb = GetComponent<Rigidbody2D>();

        m_JumpBtn.OnClickAsObservable().Subscribe(_ =>
        {
            //BoxCollider2D backgroundCollider = GetComponent<BoxCollider2D>();
            //width = backgroundCollider.size.x;
            if (!isJumping)
            {
                if (isJumping == false) //점프 중이지 않을 때
                {
                    rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse); //위쪽으로 힘을 준다.
                    isJumping = true;
                }
                else return; //점프 중일 때는 실행하지 않고 바로 return.

                // Debug.Log("### Jump!!");
                // isJumping = true;
                // rb.AddForce(Vector3.up * 300f);
                //Jump();
            }
            else
            {
                Debug.Log("### Disable Jump..");
                return;
            }
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
        rb.AddForce(Vector3.up * 300f);
        //rb.AddForce(new Vector2(0.0f, jumppower * Time.deltaTime), ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       if (collision.gameObject.CompareTag("Ground"))
       {
           Debug.Log("### Touch Ground");
           isJumping = false;
       }
    }

    private void Shoot()
    {
        //m_AttackPrefab = Resources.Load<GameObject>("Prefabs/Bullet_Normal");
        GameObject _bullet = Instantiate(m_BulletNormal, this.transform.position + Vector3.left, Quaternion.identity);
        Rigidbody2D rigid = _bullet.GetComponent<Rigidbody2D>();
        rigid.AddForce(Vector2.left * 10, ForceMode2D.Impulse);
    }
}
