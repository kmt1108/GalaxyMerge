using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour
{
    [SerializeField] Button btnSound,btnVibrate;
    [SerializeField] Sprite spriteOn, spriteOff;
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.OnSoundStateChanged += OnSoundChanged;
        AudioManager.OnVibrateStateChanged += OnVibrateChanged;
        AudioManager.SetButtonState(btnSound, AudioManager.Sound ? spriteOn : spriteOff);
        AudioManager.SetButtonState(btnVibrate, AudioManager.Vibration ? spriteOn : spriteOff);
        btnSound.onClick.AddListener(AudioManager.instance.ChangeSoundState);
        btnVibrate.onClick.AddListener(AudioManager.instance.ChangeVibrateState);
    }
    private void OnDestroy()
    {
        AudioManager.OnSoundStateChanged -= OnSoundChanged;
        AudioManager.OnVibrateStateChanged -= OnVibrateChanged;
    }
    void OnSoundChanged(bool isOn)
    {
        btnSound.GetComponent<Image>().sprite = isOn ? spriteOn : spriteOff;
    }
    void OnVibrateChanged(bool isOn)
    {
        btnVibrate.GetComponent<Image>().sprite = isOn ? spriteOn : spriteOff;
    }
}
