using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CPlayerController : MonoBehaviour
{
    [SerializeField]
    public Button m_JumpBtn;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        m_JumpBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### Jump!!");
            Jump();
        });
    }

    private void Jump()
    {
        rb.AddForce(new Vector3(0f, 300f, 0f));
    }
}
