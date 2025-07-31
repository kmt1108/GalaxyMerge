using UnityEngine;
using UnityEngine.UI;

public class LosePop : MonoBehaviour
{
    [SerializeField] Button restartButton;
    [SerializeField] Text txtScore, txtBestScore;
    // Start is called before the first frame update
    void Start()
    {
        restartButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            GameManager.instance.RestartGame();
        });
    }
    private void OnEnable()
    {
        txtScore.text = GameManager.Score.ToString("D4");
        txtBestScore.text = "BEST: " + GameManager.BestScore.ToString("D4");
    }
}
