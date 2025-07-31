using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    const string RESOURCE_PATH = "UI/";
    const string losePopPrefab = "LosePopup";
    const string settingPopPrefab = "SettingPopup";
    public static UIManager instance;
    [SerializeField] Button btnSetting;
    [SerializeField] Text txtScore, txtBestScore, txtSunCountl;

    private GameObject losePop,settingPop;
    private void Awake()
    {
        instance = this;
        btnSetting.onClick.AddListener(ShowSettingPop);
        GameManager.OnScoreChanged += OnScoreChanged;
        GameManager.OnBestScoreChanged += OnBestScoreChanged;
        GameManager.OnSunCountChanged += OnSunCountChanged;
        txtScore.text = GameManager.Score.ToString("D4");
        txtBestScore.text = "BEST: " + GameManager.BestScore.ToString("D4");
        txtSunCountl.text = "x" + GameManager.SunCount.ToString();
    }

    private void OnSunCountChanged(int count)
    {
        txtSunCountl.text = "x" + count.ToString();
    }

    private void OnBestScoreChanged(int score)
    {
        txtBestScore.text = "BEST: "+score.ToString("D4");
    }

    private void OnScoreChanged(int score)
    {
        txtScore.text = score.ToString("D4");
    }

    public void ShowLosePop()
    {
        if (!losePop)
        {
            losePop = Instantiate(Resources.Load<GameObject>(RESOURCE_PATH + losePopPrefab),transform);
            return;
        }
        losePop.SetActive(true);
    }
    public void ShowSettingPop()
    {
        if (!settingPop)
        {
            settingPop = Instantiate(Resources.Load<GameObject>(RESOURCE_PATH + settingPopPrefab),transform);
            return;
        }
        settingPop.SetActive(true);
    }
}
