
using UnityEngine;

public class IllusionObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform ship;
    [SerializeField] private GameObject rockPrefab;

    [Header("Danger")]
    [SerializeField, Range(0f, 1f)]
    private float dangerLevel = 1f;

    [SerializeField, Range(0f, 1f)]
    private float firstDangerThreshold = 0.7f;

    [Header("Maze Settings")]
    [SerializeField] private float spawnDistance = 30f;
    [SerializeField] private float rowSpacing = 14f;
    [SerializeField] private int columns = 5;
    [SerializeField] private float columnSpacing = 3.5f;
    [SerializeField] private float rowWidthOffset = 0f;

    [Header("Rock Lifetime")]
    [SerializeField] private float rockLifetime = 20f;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 2f;

    private Vector3 lastSpawnShipPosition;
    private bool wasActive;
    private bool spawningEnabled = true;

    private void Start()
    {
        if (ship == null)
        {
            Debug.LogError("Obstacle Spawner: Ship is not assigned!", this);
            enabled = false;
            return;
        }

        if (rockPrefab == null)
        {
            Debug.LogError("Obstacle Spawner: Rock Prefab is not assigned!", this);
            enabled = false;
            return;
        }

        // Allow the first row to spawn immediately
        lastSpawnShipPosition = ship.position -
                                ship.forward * rowSpacing;
    }

    private void Update()
    {
        if (!spawningEnabled)
            return;
        // In this project, lower dangerLevel means more danger.
        bool active = dangerLevel <= firstDangerThreshold;

        if (!active)
        {
            wasActive = false;
            return;
        }

        if (!wasActive)
        {
            // Start a fresh sequence when danger first activates
            lastSpawnShipPosition = ship.position -
                                    ship.forward * rowSpacing;
            wasActive = true;
        }

        float distanceTravelled = Vector3.Distance(
            ship.position,
            lastSpawnShipPosition
        );

        if (distanceTravelled >= rowSpacing)
        {
            SpawnObstacleRow();
            lastSpawnShipPosition = ship.position;
        }
    }
    public void SetSpawningEnabled(bool value)
    {
        spawningEnabled = value;
    }
    
    public void SetDangerLevel(float value)
    {
        dangerLevel = Mathf.Clamp01(value);
    }

    private void SpawnObstacleRow()
    {
        Vector3 forward = ship.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = ship.right;
        right.y = 0f;
        right.Normalize();

        Vector3 rowCenter = ship.position +
                            forward * spawnDistance;

        // Choose a passage that shifts from row to row
        int passageColumn = Random.Range(0, columns);

        // Keep one guaranteed open passage
        for (int i = 0; i < columns; i++)
        {
            if (i == passageColumn)
                continue;

            Vector3 position = rowCenter +
                right * ((i - (columns - 1) / 2f) *
                         columnSpacing + rowWidthOffset);

            // Add small variation to avoid perfectly straight walls
            position += right * Random.Range(-0.4f, 0.4f);

            GameObject rock = Instantiate(
                rockPrefab,
                position,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            );

            IllusionRock illusion = rock.GetComponent<IllusionRock>();

            if (illusion != null)
            {
                illusion.Initialize(
                    fadeInDuration,
                    fadeOutDuration,
                    rockLifetime
                );
            }
        }
    }
}
