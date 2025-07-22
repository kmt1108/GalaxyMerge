using UnityEngine;
using UnityEngine.UI;

public class LosePop : MonoBehaviour
{
    [SerializeField] Button restartButton;
    // Start is called before the first frame update
    void Start()
    {
        restartButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            GameManager.instance.RestartGame();
        });
    }
}
