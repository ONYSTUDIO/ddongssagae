using System.Collections;
using System.Collections.Generic;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CLoginManager : MonoBehaviour
{

    [SerializeField]
    public Button m_LoginBtn;

    // Start is called before the first frame update
    void Start()
    {
        m_LoginBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("###");
            SceneManager.LoadScene("SelectScene");
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

}
