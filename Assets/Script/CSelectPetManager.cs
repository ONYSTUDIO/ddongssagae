using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static CConstants;

public class CSelectPetManager : MonoBehaviour
{

    [SerializeField]
    public Button m_JjajangBtn;

    [SerializeField]
    public Sprite m_JjajangImg;

    [SerializeField]
    public Button m_HoduBtn;

    [SerializeField]
    public Sprite m_HoduImg;

    [SerializeField]
    public Button m_JjangaBtn;

    [SerializeField]
    public Sprite m_JjangaImg;

    [SerializeField]
    public Button m_DubuBtn;

    [SerializeField]
    public Sprite m_DubuImg;

    [SerializeField]
    public Button m_NextBtn;

    private string seletedPetSprite;
    private int seletedPetType;
    private static CPlayerController cPlayerController;

    private void OnClickeBtn(GameObject target)
    {
        Debug.Log("### Target : " + target.name);
    }
    // Start is called before the first frame update
    void Start()
    {
        seletedPetSprite = "JJAJANG_SPRITE_PATH";
        seletedPetType = 0;
        Select();
    }

    private void Select()
    {
        m_JjajangBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### 짜장");
            //seletedPetSprite = "JJAJANG_SPRITE_PATH";
            seletedPetType = 0;
            ShowOnButton();
        });

        m_HoduBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### 호두");
            //seletedPetSprite = "HODU_SPRITE_PATH";
            seletedPetType = 1;
            ShowOnButton();
        });

        m_JjangaBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### 짱아");
            //seletedPetSprite = "JJANGA_SPRITE_PATH";
            seletedPetType = 2;
            ShowOnButton();
        });

        m_DubuBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Debug.Log("### 두부");
            //seletedPetSprite = "DUBU_SPRITE_PATH";
            seletedPetType = 3;
            ShowOnButton();
        });

        m_NextBtn.OnClickAsObservable().Subscribe(_ =>
        {
            
            //this.GetComponent<CPlayerController>().playerType = seletedPetSprite;
            CConstants.PLAYER_TYPE = seletedPetType;
            //DontDestroyOnLoad(gameObject);
            Debug.Log("### _Select : " + seletedPetType);
            SceneManager.LoadScene("StageScene");
        });
    }

    private void ShowOnButton()
    {
        m_NextBtn.transform.GetChild(0).gameObject.SetActive(true);
    }

    // private void TransferScene()
    // {
    //     SceneManager.LoadScene("StageScene");
    // }
}
