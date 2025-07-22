using UnityEngine;

public class UIManager : MonoBehaviour
{
    const string RESOURCE_PATH = "UI/";
    const string losePopPrefab = "LosePopup";
    public static UIManager instance;
    private GameObject losePop;
    private void Awake()
    {
        instance = this;
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
}
