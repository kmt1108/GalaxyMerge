using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
public enum GameState { Playing, Lose }
public class GameManager : MonoBehaviour
{
    public static int Score { get; set; } = 0;
    public static int SunCount { 
        get
        {
            return PlayerPrefs.GetInt("SunCount", 0);
        } 
        set
        {
            PlayerPrefs.SetInt("SunCount", value);
        }
    }
    public static int BestScore { 
        get
        {
            return PlayerPrefs.GetInt("BestScore", 0);
        } 
        set
        {
            PlayerPrefs.SetInt("BestScore", value);
        }
    }
    //declare const score array for each planet index
    int[] scoreEarned= new int[9] { 2, 3, 5, 9, 17, 33, 65, 129, 257};

    public static GameManager instance;
    public static GameState state;
    public static Action<int> OnScoreChanged;
    public static Action<int> OnBestScoreChanged;
    public static Action<int> OnSunCountChanged;
    [SerializeField] PlanetPooling planetPool;
    [SerializeField] AnimationCurve curve;
    [Range(0f, 0.1f)]
    public float forceRatio = 0.005f;
    [SerializeField] Color normalColor,warningColor;
    public GameObject[] listTrajectory;
    public Transform targetPos,launchPos,nextPos;
    [SerializeField] Transform sunTrs,sunCountTrs;
    private Vector2 forceToAdd;
    private Planet currentPlanet;
    private float currentMass;
    private Planet nextPlanet;
    private SpriteRenderer targetSprite;
    Rigidbody2D rb;
    TargetJoint2D joint;
    bool draggable = true;
    bool isDragging = false;
    Vector2 launchPosInScreen;
    List<Planet> listWarning = new List<Planet>();
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        SpawnCurrentPlanet();
        SpawnNextPlanet();
        Planet.OnPlanetMerge += (index, position) =>
        {
            // Handle planet merge logic here
            planetPool.GetPlanet(index+1, position, targetPos.position, true).PlayVFX();
            Score += scoreEarned[index];
            OnScoreChanged?.Invoke(Score);
            if (Score > BestScore)
            {
                BestScore = Score;
                OnBestScoreChanged?.Invoke(BestScore);
            }
            if (index == 8)
            {
                var pos = targetPos.position;
                pos.z = -5;
                var target = Camera.main.ScreenToWorldPoint(sunCountTrs.position);
                sunTrs.position = pos;
                sunTrs.localScale = Vector3.one;
                sunTrs.gameObject.SetActive(true);
                sunTrs.DOScale(0.1f, 1f).SetDelay(1);
                sunTrs.DOMove(target, 1f).SetDelay(1).OnComplete(() =>
                {
                    SunCount++;
                    OnSunCountChanged?.Invoke(SunCount);
                    sunTrs.gameObject.SetActive(false);
                    sunTrs.DOKill();
                });
            }
        };
        launchPosInScreen = Camera.main.WorldToScreenPoint(launchPos.position);
        targetSprite = targetPos.GetComponent<SpriteRenderer>();
        Planet.OnWarning += OnWarningPlanet;
        Planet.OnWarningStart += OnWarningStart;
        Planet.OnWarningEnded += OnWarningEnded;
    }
    private void OnWarningStart(Planet planet)
    {
        listWarning.Add(planet);
        if (listWarning.Count == 1)
        {
            StartWarning();
        }
    }
    private void OnWarningEnded(Planet planet)
    {
        listWarning.Remove(planet);
        if (listWarning.Count == 0)
        {
            StopWarning();
        }
    }
    Tween warningTween;
    private void StartWarning()
    {
        if (warningTween == null)
        {
            warningTween = targetSprite.DOColor(warningColor, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false);
        }
        else
        {
            warningTween.Rewind(false);
            warningTween.Play();
        }
        warningTween.timeScale = 1;
    }
    private void StopWarning(bool isLose=false)
    {
        warningTween.Pause();
        targetSprite.color = isLose ? warningColor : normalColor;
    }
    private void OnWarningPlanet(Planet planet, float timeWarning)
    {
        if (!listWarning.Contains(planet))
        {
            listWarning.Add(planet);
        }
        if(listWarning.Count>0) warningTween.timeScale = listWarning[0].WarningTime;
    }
    public void LoseGame()
    {
        if (state != GameState.Lose)
        {
            state = GameState.Lose;
            StopWarning(isLose: true);
            Debug.Log("Lose");
            UIManager.instance.ShowLosePop();
        }
    }
    public void RestartGame()
    {
        state = GameState.Playing;
        StopWarning();
        planetPool.StoreAllPlanets();
        Score = 0;
        OnScoreChanged?.Invoke(Score);
        OnSunCountChanged?.Invoke(SunCount);
        SpawnCurrentPlanet();
        SpawnNextPlanet();
    }
    void Update()
    {
        if(warningTween!=null) Debug.Log($"Tween active: {warningTween.timeScale}");
        if (state != GameState.Playing) return;
        if (Input.GetMouseButtonDown(0) && draggable)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            isDragging = true;
            rb = currentPlanet.Rb;
            joint = currentPlanet.Joint;
            currentMass = rb.mass;
            forceToAdd = Vector2.ClampMagnitude(((Vector2)Input.mousePosition - launchPosInScreen) * -1*forceRatio, 5);
            DrawTrajectory();
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
            forceToAdd = Vector2.ClampMagnitude(((Vector2)Input.mousePosition - launchPosInScreen) * -1*currentMass*forceRatio, currentMass*5);
            DrawTrajectory();
        }
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            draggable = false;
            HideTrajectory();
            currentPlanet.Rb.simulated = true;
            currentPlanet.StartLaunch(forceToAdd);
            forceToAdd = Vector2.zero;
            CheckSwitchPlanet();
        }
    }
    public void DrawTrajectory()
    {
        Vector2 position = currentPlanet.transform.position;
        Vector2 velocity = forceToAdd / rb.mass;

        float k = Mathf.Pow(2 * Mathf.PI * joint.frequency, 2) * rb.mass; // ?? c?ng lò xo
        float c = 4 * Mathf.PI * joint.frequency * joint.dampingRatio * rb.mass; // H? s? c?n
        float lastV = 0;
        for (int i = 0; i < listTrajectory.Length; i++)
        {
            // L?c h?i v?
            Vector2 springForce = -k * (position - joint.target);
            Vector2 dampingForce = -c * velocity;
            Vector2 totalForce = springForce + dampingForce;
            if (totalForce.magnitude > joint.maxForce)
                totalForce = totalForce.normalized * joint.maxForce;

            // C?p nh?t gia t?c, v?n t?c, v? trí
            var crV = curve.Evaluate((float)i / listTrajectory.Length);
            Vector2 acceleration = totalForce / rb.mass;
            velocity += acceleration * (crV - lastV) * 0.5f;
            position += velocity * (crV - lastV) * 0.5f;
            listTrajectory[i].transform.position = position;
            lastV = crV;
        }
    }
    private void HideTrajectory()
    {
        foreach(var point in listTrajectory)
        {
            point.transform.position = launchPos.position;
        }
    }

    private void CheckSwitchPlanet()
    {
        nextPlanet.transform.DOMove(launchPos.position, 0.5f).onComplete+=()=>draggable = true;
        currentPlanet=nextPlanet;
        SpawnNextPlanet();
    }
    public void SpawnNextPlanet()
    {
        nextPlanet = planetPool.GetPlanet(RandomplanetIndex(), nextPos.position, targetPos.position);
    }
    public void SpawnCurrentPlanet()
    {
        currentPlanet = planetPool.GetPlanet(RandomplanetIndex(), launchPos.position, targetPos.position);
    }

    private int RandomplanetIndex()
    {
        return UnityEngine.Random.Range(0, 5);
    }
}
