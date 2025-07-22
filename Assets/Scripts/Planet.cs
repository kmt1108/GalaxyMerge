using System;
using UnityEngine;
public class Planet : MonoBehaviour
{
    public static int WarningCount = 0;
    public static Action<Planet, float> OnWarning;
    public static Action<Planet> OnHidden;
    public static Action<int> OnWarningCountChanged;
    public static Action<int, Vector3> OnPlanetMerge;
    private Rigidbody2D rb;
    private TargetJoint2D joint;
    public int planetIndex;
    [SerializeField] GameObject warning;
    public Rigidbody2D Rb => rb;
    public TargetJoint2D Joint => joint;
    [HideInInspector]
    public bool isMerged = false;
    private float timeTrigger = 0f;
    public float WarningTime => timeTrigger;
    private bool checkTrigger = false;
    bool isWarning;
    private void Update()
    {
        if (checkTrigger&&GameManager.state==GameState.Playing)
        {
            timeTrigger += Time.deltaTime;
            if (timeTrigger > 0.5f)
            {
                if (!isWarning) {
                    isWarning = true;
                    WarningCount++;
                    OnWarningCountChanged?.Invoke(WarningCount);
                }
                OnWarning?.Invoke(this, timeTrigger);
                warning.SetActive(true);
            }
            if (timeTrigger >= 5)
            {
                GameManager.instance.LoseGame();
            }
        }
    }
    public void Setup(Vector3 target,bool simulate)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = simulate;
        joint = GetComponent<TargetJoint2D>();
        joint.enabled = simulate;
        joint.target = target;
        isMerged = false;
    }
    public void StartLaunch(Vector2 forceToAdd)
    {
        rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        joint.enabled = true;
    }
    public void Reset()
    {
        checkTrigger = false;
        timeTrigger = 0f;
        isWarning = false;
        warning.SetActive(false);
        rb.simulated = false;
        joint.enabled = false;
        gameObject.SetActive(false);
        transform.rotation = Quaternion.identity;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Planet"))
        {
            Planet otherPlanet = collision.gameObject.GetComponent<Planet>();
            if (otherPlanet != null && otherPlanet.planetIndex == planetIndex)
            {
                // If the collided planet is the same type, we can merge or do something else
                // For now, we just destroy the other planet
                if (isMerged || otherPlanet.isMerged) return; // Prevent merging if already merged
                isMerged = true;
                otherPlanet.isMerged = true;
                gameObject.SetActive(false);
                otherPlanet.gameObject.SetActive(false);
                OnHidden?.Invoke(this);
                OnHidden?.Invoke(otherPlanet);
                OnPlanetMerge?.Invoke(planetIndex, collision.contacts[0].point);
            }
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Target"))
        {
            checkTrigger = true;
            timeTrigger = 0;
        } 
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Target"))
        {
            if (isWarning)
            {
                isWarning = false;
                WarningCount--;
                OnWarningCountChanged?.Invoke(WarningCount);
                warning.SetActive(false);
            }
            checkTrigger = false;
            timeTrigger = 0;
        }

    }
}
