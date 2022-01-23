using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CSelectPetManager : MonoBehaviour
{

    [SerializeField]
    public Button m_JjajangBtn;
    [SerializeField]
    public Button m_HoduBtn;
    [SerializeField]
    public Button m_JjangaBtn;
    [SerializeField]
    public Button m_DubuBtn;

    private string seletedPetNm;

    private void OnClickeBtn(GameObject target)
    {
        Debug.Log("### Target : " + target.name);
    }
    // Start is called before the first frame update
    void Start()
    {
        Select();
    }

    private void Select()
    {
        m_JjajangBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### 짜장");
            //seletedPetNm = _.toString();
            Debug.Log("### _Select : " + _);
        });

        m_HoduBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### 호두");
            //seletedPetNm = _;
            Debug.Log("### _Select : " + _);
        });

        m_JjangaBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### 짱아");
            //seletedPetNm = _;
            Debug.Log("### _Select : " + _);
        });

        m_DubuBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### 두부");
            //seletedPetNm = _;
            Debug.Log("### _Select : " + _);
        });
    }
    // Update is called once per frame
    void Update()
    {

    }
}
