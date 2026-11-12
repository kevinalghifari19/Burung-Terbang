using System.Collections.Generic;
using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    private class PipeItem
    {
        public GameObject gameObject;
        public bool hasScored;
    }

    [Header("Prefab Reference")]
    [Tooltip("Prefab rintangan sepasang pipa")]
    public GameObject pipePrefab;

    [Header("Spawn Settings")]
    [Tooltip("Interval waktu spawn pipa dalam detik")]
    [SerializeField] private float spawnRate = 2f;

    [Tooltip("Batas minimum posisi Y acak")]
    [SerializeField] private float minY = -2f;

    [Tooltip("Batas maksimum posisi Y acak")]
    [SerializeField] private float maxY = 2f;

    [Tooltip("Posisi koordinat X awal saat pipa di-spawn")]
    [SerializeField] private float spawnX = 12f;

    [Header("Movement Settings")]
    [Tooltip("Kecepatan pipa bergerak ke kiri")]
    [SerializeField] private float speed = 3f;

    [Tooltip("Batas koordinat X paling kiri untuk menghancurkan pipa")]
    [SerializeField] private float destroyX = -12f;

    private float timer = 0f;
    private readonly List<PipeItem> activePipes = new List<PipeItem>();

    private void Start()
    {
        SpawnPipe();
    }

    private void Update()
    {
        // Jika game over, pipa berhenti bergerak/spawn
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        // 1. Hitung timer spawn pipa
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnPipe();
            timer = 0f;
        }

        // 2. Gerakkan seluruh pipa aktif ke kiri dan deteksi skor / destroy
        for (int i = activePipes.Count - 1; i >= 0; i--)
        {
            PipeItem item = activePipes[i];
            if (item != null && item.gameObject != null)
            {
                // Gerakkan ke kiri dengan transform.Translate
                item.gameObject.transform.Translate(Vector3.left * (speed * Time.deltaTime), Space.World);

                // Deteksi skor: saat pipa melewati posisi burung (X <= 0)
                if (!item.hasScored && item.gameObject.transform.position.x <= 0f)
                {
                    item.hasScored = true;
                    if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
                    {
                        GameManager.Instance.AddScore();
                    }
                }

                // Hancurkan pipa jika melewati batas kiri layar
                if (item.gameObject.transform.position.x < destroyX)
                {
                    Destroy(item.gameObject);
                    activePipes.RemoveAt(i);
                }
            }
            else
            {
                activePipes.RemoveAt(i);
            }
        }
    }

    public void SpawnPipe()
    {
        if (pipePrefab == null)
        {
            Debug.LogWarning("[PipeSpawner] pipePrefab belum di-assign di Inspector!");
            return;
        }

        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(spawnX, randomY, 0f);

        GameObject newPipe = Instantiate(pipePrefab, spawnPosition, Quaternion.identity);
        activePipes.Add(new PipeItem { gameObject = newPipe, hasScored = false });
    }
}
