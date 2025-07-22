using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
public enum GameState { Playing, Lose }
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static GameState state;
    [SerializeField] PlanetPooling planetPool;
    [SerializeField] AnimationCurve curve;
    [Range(0f, 0.1f)]
    public float forceRatio = 0.005f;
    [SerializeField] Color normalColor,warningColor;
    public GameObject[] listTrajectory;
    public Transform targetPos,launchPos,nextPos;
    private Vector2 forceToAdd;
    private Planet currentPlanet;
    private float currentMass;
    private Planet nextPlanet;
    private SpriteRenderer targetSprite;
    private GameObject[] planetPrefabs=new GameObject[9];
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
            Debug.Log($"Planet {index} merged at position {position}");
            planetPool.GetPlanet(index+1, position, targetPos.position, true);
        };
        launchPosInScreen = Camera.main.WorldToScreenPoint(launchPos.position);
        targetSprite = targetPos.GetComponent<SpriteRenderer>();
        Planet.OnWarning += OnWarningPlanet;
        Planet.OnWarningCountChanged += OnWarningCountChanged;
    }

    private void OnWarningCountChanged(int count)
    {
        if (count == 1)
        {
            StartWarning();
        }
        else if (count == 0)
        {
            StopWarning();
        }
    }
    private void StopWarning(bool isLose=false)
    {
        sequence?.Pause();
        targetSprite.color = isLose?warningColor:normalColor;
    }
    Sequence sequence;
    private void StartWarning()
    {
        targetSprite.color = normalColor;
        if (sequence != null)
        {
            sequence.Restart();
        }
        else
        {
            sequence = DOTween.Sequence();
            sequence.Append(targetSprite.DOColor(warningColor, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine));
        }
    }

    private void OnWarningPlanet(Planet planet, float timeWarning)
    {
        if (!listWarning.Contains(planet))
        {
            listWarning.Add(planet);
        }
        sequence.timeScale = listWarning[0].WarningTime;
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
        SpawnCurrentPlanet();
        SpawnNextPlanet();
    }
    void Update()
    {
        if (state != GameState.Playing) return;
        if (Input.GetMouseButtonDown(0) && draggable)
        {
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
        return UnityEngine.Random.Range(0, 4);
    }
}
