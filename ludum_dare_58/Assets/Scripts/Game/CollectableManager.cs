using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LudumDare58.Game
{
    public class CollectableManager : MonoBehaviour
    {
        [Header("Collectable Settings")]
        [SerializeField] GameObject collectablePrefab;
        [SerializeField] int maxActiveCollectables = 5;
        [SerializeField] float spawnInterval = 3f;
        [SerializeField] int totalCollectablesToSpawn = 20;

        [Header("Spawn Points")]
        [SerializeField] List<Transform> spawnPoints = new List<Transform>();

        [Header("UI")]
        [SerializeField] ScoreAndComboUI scoreUI;

        public UnityAction OnAllCollected;

        int collectedCount = 0;
        int spawnedCount = 0;

        List<GameObject> activeCollectables = new List<GameObject>();
        Coroutine spawnRoutine;

        private void Awake()
        {
            // ✅ Register manually placed collectables that are children of this manager
            var existingCollectables = GetComponentsInChildren<Collectable>(includeInactive: true);

            foreach (var c in existingCollectables)
            {
                // Subscribe to collect event
                c.OnCollect += HandleCollect;

                // Add to active list if enabled
                if (c.gameObject.activeSelf)
                    activeCollectables.Add(c.gameObject);
            }

            // Set counts accordingly
            collectedCount = 0;
            spawnedCount = activeCollectables.Count;

            Debug.Log($"[CollectableManager] Registered {activeCollectables.Count} pre-placed collectables.");
        }

        private void Start()
        {
            if (collectablePrefab == null)
            {
                Debug.LogWarning("[CollectableManager] No collectable prefab assigned.");
                return;
            }

            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                Debug.LogWarning("[CollectableManager] No spawn points assigned.");
                return;
            }

            // Start spawning more collectables if needed
            spawnRoutine = StartCoroutine(SpawnCollectables());
        }

        private IEnumerator SpawnCollectables()
        {
            while (spawnedCount < totalCollectablesToSpawn)
            {
                // Clean inactive ones
                activeCollectables.RemoveAll(c => c == null || !c.activeSelf);

                // Spawn new ones if under limit
                if (activeCollectables.Count < maxActiveCollectables)
                {
                    SpawnOne();
                }

                yield return new WaitForSeconds(spawnInterval);
            }

            // Wait until all spawned or manual collectables are collected
            while (activeCollectables.Exists(c => c != null && c.activeSelf))
                yield return null;

            OnAllCollected?.Invoke();
        }

        private void SpawnOne()
        {
            if (spawnedCount >= totalCollectablesToSpawn)
                return;

            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            var newCollectable = Instantiate(collectablePrefab, spawnPoint.position, spawnPoint.rotation, transform);
            activeCollectables.Add(newCollectable);
            spawnedCount++;

            // Subscribe to collect event
            var collectable = newCollectable.GetComponent<Collectable>();
            if (collectable != null)
                collectable.OnCollect += HandleCollect;
        }

        private void HandleCollect()
        {
            if (scoreUI != null)
                scoreUI.OnCollectableCollected();

            collectedCount++;
            activeCollectables.RemoveAll(c => c == null || !c.activeSelf);

            if (spawnedCount >= totalCollectablesToSpawn && activeCollectables.Count == 0)
            {
                OnAllCollected?.Invoke();
            }
        }

        public void StopSpawning()
        {
            if (spawnRoutine != null)
                StopCoroutine(spawnRoutine);
        }
    }
}
