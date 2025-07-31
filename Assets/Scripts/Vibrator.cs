using UnityEngine;
public static class Vibrator
{
#if UNITY_ANDROID && !UNITY_EDITOR
    public readonly static AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
    public readonly static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public readonly static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
#endif
    public static void Vibrate(long miliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            vibrator.Call("vibrate",miliseconds);
#else
        Handheld.Vibrate();
#endif

    }
}