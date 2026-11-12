using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AutoSetupPipes
{
    private const string BOTTOM_PIPE_PATH = "Assets/Jovial Games/Basic 2D Side Scroller Art Kit/Sprites/Background/Obstacle.png";
    private const string TOP_PIPE_PATH = "Assets/Jovial Games/Basic 2D Side Scroller Art Kit/Sprites/Background/Obstacle_2.png";
    private const string PREFABS_DIR = "Assets/Prefabs";
    private const string PREFAB_PATH = "Assets/Prefabs/PipePair.prefab";

    [MenuItem("Tools/Flappy Bird/Auto Setup Pipes", false, 2)]
    public static void SetupPipes()
    {
        // 1. Cari Sprite Obstacle (Bottom Pipe) dan Obstacle_2 (Top Pipe)
        Sprite bottomSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BOTTOM_PIPE_PATH);
        Sprite topSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOP_PIPE_PATH);

        // Fallback jika path spesifik berpindah
        if (bottomSprite == null || topSprite == null)
        {
            string[] guids = AssetDatabase.FindAssets("Obstacle t:Texture2D");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("Obstacle.png"))
                {
                    bottomSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
                else if (path.EndsWith("Obstacle_2.png"))
                {
                    topSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
            }
        }

        if (bottomSprite == null || topSprite == null)
        {
            EditorUtility.DisplayDialog(
                "Error",
                "Sprite Obstacle atau Obstacle_2 tidak ditemukan di folder Sprites/Background!\nPastikan aset Jovial Games sudah terimpor.",
                "OK"
            );
            return;
        }

        // 2. Buat folder Assets/Prefabs jika belum ada
        if (!AssetDatabase.IsValidFolder(PREFABS_DIR))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // 3. Bangun GameObject sementara untuk Prefab PipePair
        GameObject pipePairRoot = new GameObject("PipePair");

        // Dimensi sprite: tinggi = 5.62 unit, pivot di tengah (0.5, 0.5)
        // Setengah tinggi sprite = 2.81 unit
        // Target celah (gap) = 3.5 unit di tengah (Y = -1.75 sampai Y = +1.75)
        // Posisi Y TopPipe = +1.75 + 2.81 = +4.56
        // Posisi Y BottomPipe = -1.75 - 2.81 = -4.56
        const float gapHalf = 1.75f;
        const float halfSpriteHeight = 2.81f;
        const float topY = gapHalf + halfSpriteHeight;    // +4.56f
        const float bottomY = -(gapHalf + halfSpriteHeight); // -4.56f

        // A. Susun TopPipe (Obstacle_2)
        GameObject topPipe = new GameObject("TopPipe");
        topPipe.transform.SetParent(pipePairRoot.transform);
        topPipe.transform.localPosition = new Vector3(0f, topY, 0f);

        SpriteRenderer topSr = topPipe.AddComponent<SpriteRenderer>();
        topSr.sprite = topSprite;
        // Obstacle_2 dari kit sudah memiliki ujung/rim menghadap ke bawah secara alami
        topSr.flipY = false;
        topSr.sortingOrder = 3;

        BoxCollider2D topCol = topPipe.AddComponent<BoxCollider2D>();
        topCol.size = new Vector2(1.44f, 5.62f);
        topCol.offset = Vector2.zero;

        // B. Susun BottomPipe (Obstacle)
        GameObject bottomPipe = new GameObject("BottomPipe");
        bottomPipe.transform.SetParent(pipePairRoot.transform);
        bottomPipe.transform.localPosition = new Vector3(0f, bottomY, 0f);

        SpriteRenderer bottomSr = bottomPipe.AddComponent<SpriteRenderer>();
        bottomSr.sprite = bottomSprite;
        // Obstacle memiliki ujung/rim menghadap ke atas secara alami
        bottomSr.flipY = false;
        bottomSr.sortingOrder = 3;

        BoxCollider2D bottomCol = bottomPipe.AddComponent<BoxCollider2D>();
        bottomCol.size = new Vector2(1.44f, 5.62f);
        bottomCol.offset = Vector2.zero;

        // 4. Simpan sebagai Prefab di Assets/Prefabs/PipePair.prefab
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(pipePairRoot, PREFAB_PATH);
        Object.DestroyImmediate(pipePairRoot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 5. Setup Scene: Cari atau buat GameManager di Scene aktif
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager == null)
        {
            gameManager = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gameManager, "Create GameManager");
        }

        // Tambahkan komponen PipeSpawner ke GameManager jika belum ada
        PipeSpawner spawner = gameManager.GetComponent<PipeSpawner>();
        if (spawner == null)
        {
            spawner = gameManager.AddComponent<PipeSpawner>();
            Undo.RegisterCreatedObjectUndo(gameManager, "Add PipeSpawner");
        }

        // Assign prefab PipePair ke field pipePrefab di PipeSpawner
        spawner.pipePrefab = savedPrefab;
        EditorUtility.SetDirty(spawner);
        EditorUtility.SetDirty(gameManager);

        // 6. Tandai scene dirty dan pilih GameManager di Hierarchy
        Selection.activeGameObject = gameManager;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[AutoSetupPipes] Sukses! Prefab 'PipePair' tersimpan di Assets/Prefabs dan telah di-assign ke GameManager di Scene.");
        EditorUtility.DisplayDialog(
            "Setup Pipa Selesai",
            "Prefab 'PipePair' berhasil dibuat dengan celah 3.5 unit dan di-assign ke GameObject 'GameManager' di Scene aktif!\n\nTekan Play untuk melihat pipa muncul dan bergerak ke kiri.",
            "OK"
        );
    }
}
