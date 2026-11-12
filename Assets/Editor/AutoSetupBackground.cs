using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AutoSetupBackground
{
    private const string BG_DIR = "Assets/Jovial Games/Basic 2D Side Scroller Art Kit/Sprites/Background/";

    [MenuItem("Tools/Flappy Bird/Auto Setup Background", false, 4)]
    public static void SetupBackground()
    {
        // 1. Muat semua sprite yang dibutuhkan
        Sprite skySprite = LoadSprite(BG_DIR + "Sky_Background.png", "Sky_Background");
        Sprite mountains1Sprite = LoadSprite(BG_DIR + "Moutains_1.png", "Moutains_1");
        Sprite mountain2Sprite = LoadSprite(BG_DIR + "Mountain_2.png", "Mountain_2");
        Sprite clouds1Sprite = LoadSprite(BG_DIR + "Clouds_1.png", "Clouds_1");
        Sprite clouds2Sprite = LoadSprite(BG_DIR + "Clouds_2.png", "Clouds_2");
        Sprite bush1Sprite = LoadSprite(BG_DIR + "Bush_1.png", "Bush_1");
        Sprite bush2Sprite = LoadSprite(BG_DIR + "Bush_2.png", "Bush_2");
        Sprite bush3Sprite = LoadSprite(BG_DIR + "Bush_3.png", "Bush_3");
        Sprite groundSprite = LoadSprite(BG_DIR + "Ground.png", "Ground");

        if (skySprite == null || groundSprite == null)
        {
            EditorUtility.DisplayDialog("Error", "Sprite background tidak ditemukan di folder Sprites/Background!", "OK");
            return;
        }

        // 2. Cek dan hapus BackgroundLayers lama jika sudah ada
        GameObject existingRoot = GameObject.Find("BackgroundLayers");
        if (existingRoot != null)
        {
            Undo.DestroyObjectImmediate(existingRoot);
        }

        // 3. Buat GameObject root BackgroundLayers
        GameObject root = new GameObject("BackgroundLayers");
        Undo.RegisterCreatedObjectUndo(root, "Create BackgroundLayers");
        root.transform.position = Vector3.zero;

        const float layerWidth = 19.2f;

        // LAYER 1: SKY (Sorting Order = -5, Statis speed = 0)
        GameObject skyGo = new GameObject("BG_Sky");
        skyGo.transform.SetParent(root.transform, false);
        skyGo.transform.localPosition = Vector3.zero;

        SpriteRenderer skySr = skyGo.AddComponent<SpriteRenderer>();
        skySr.sprite = skySprite;
        skySr.sortingOrder = -5;

        ParallaxScroller skyScroller = skyGo.AddComponent<ParallaxScroller>();
        skyScroller.scrollSpeed = 0f;

        // LAYER 2: MOUNTAINS (Sorting Order = -4, Kecepatan lambat = 0.5f)
        GameObject mountainsGo = new GameObject("BG_Mountains");
        mountainsGo.transform.SetParent(root.transform, false);

        GameObject m1 = new GameObject("Mountains_Part1");
        m1.transform.SetParent(mountainsGo.transform, false);
        m1.transform.localPosition = new Vector3(0f, -2.8f, 0f);
        SpriteRenderer m1Sr = m1.AddComponent<SpriteRenderer>();
        m1Sr.sprite = mountains1Sprite != null ? mountains1Sprite : mountain2Sprite;
        m1Sr.sortingOrder = -4;

        GameObject m2 = new GameObject("Mountains_Part2");
        m2.transform.SetParent(mountainsGo.transform, false);
        m2.transform.localPosition = new Vector3(layerWidth, -2.8f, 0f);
        SpriteRenderer m2Sr = m2.AddComponent<SpriteRenderer>();
        m2Sr.sprite = mountain2Sprite != null ? mountain2Sprite : mountains1Sprite;
        m2Sr.sortingOrder = -4;

        ParallaxScroller mountainScroller = mountainsGo.AddComponent<ParallaxScroller>();
        mountainScroller.scrollSpeed = 0.5f;
        mountainScroller.spriteWidth = layerWidth;
        mountainScroller.sprite1 = m1.transform;
        mountainScroller.sprite2 = m2.transform;

        // LAYER 3: CLOUDS (Sorting Order = -3, Kecepatan sedang = 1.0f)
        GameObject cloudsGo = new GameObject("BG_Clouds");
        cloudsGo.transform.SetParent(root.transform, false);

        GameObject c1 = new GameObject("Clouds_Part1");
        c1.transform.SetParent(cloudsGo.transform, false);
        c1.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        SpriteRenderer c1Sr = c1.AddComponent<SpriteRenderer>();
        c1Sr.sprite = clouds1Sprite != null ? clouds1Sprite : clouds2Sprite;
        c1Sr.sortingOrder = -3;

        GameObject c2 = new GameObject("Clouds_Part2");
        c2.transform.SetParent(cloudsGo.transform, false);
        c2.transform.localPosition = new Vector3(layerWidth, 1.5f, 0f);
        SpriteRenderer c2Sr = c2.AddComponent<SpriteRenderer>();
        c2Sr.sprite = clouds2Sprite != null ? clouds2Sprite : clouds1Sprite;
        c2Sr.sortingOrder = -3;

        ParallaxScroller cloudsScroller = cloudsGo.AddComponent<ParallaxScroller>();
        cloudsScroller.scrollSpeed = 1.0f;
        cloudsScroller.spriteWidth = layerWidth;
        cloudsScroller.sprite1 = c1.transform;
        cloudsScroller.sprite2 = c2.transform;

        // LAYER 4: BUSH (Sorting Order = -2, Kecepatan cepat = 2.0f)
        GameObject bushGo = new GameObject("BG_Bush");
        bushGo.transform.SetParent(root.transform, false);

        GameObject bSet1 = new GameObject("BushSet_1");
        bSet1.transform.SetParent(bushGo.transform, false);
        bSet1.transform.localPosition = Vector3.zero;
        CreateBushProp(bSet1.transform, bush1Sprite, -6f, -3.15f, -2);
        CreateBushProp(bSet1.transform, bush2Sprite, 0f, -3.15f, -2);
        CreateBushProp(bSet1.transform, bush3Sprite, 6f, -3.15f, -2);

        GameObject bSet2 = new GameObject("BushSet_2");
        bSet2.transform.SetParent(bushGo.transform, false);
        bSet2.transform.localPosition = new Vector3(layerWidth, 0f, 0f);
        CreateBushProp(bSet2.transform, bush1Sprite, -6f, -3.15f, -2);
        CreateBushProp(bSet2.transform, bush2Sprite, 0f, -3.15f, -2);
        CreateBushProp(bSet2.transform, bush3Sprite, 6f, -3.15f, -2);

        ParallaxScroller bushScroller = bushGo.AddComponent<ParallaxScroller>();
        bushScroller.scrollSpeed = 2.0f;
        bushScroller.spriteWidth = layerWidth;
        bushScroller.sprite1 = bSet1.transform;
        bushScroller.sprite2 = bSet2.transform;

        // LAYER 5: GROUND (Sorting Order = -1, Kecepatan pipa = 3.0f)
        GameObject groundGo = new GameObject("BG_Ground");
        groundGo.transform.SetParent(root.transform, false);

        GameObject g1 = new GameObject("Ground_Part1");
        g1.transform.SetParent(groundGo.transform, false);
        g1.transform.localPosition = new Vector3(0f, -4.37f, 0f);
        SpriteRenderer g1Sr = g1.AddComponent<SpriteRenderer>();
        g1Sr.sprite = groundSprite;
        g1Sr.sortingOrder = -1;
        BoxCollider2D g1Col = g1.AddComponent<BoxCollider2D>();
        g1Col.size = new Vector2(layerWidth, 2.1f);
        g1Col.offset = Vector2.zero;

        GameObject g2 = new GameObject("Ground_Part2");
        g2.transform.SetParent(groundGo.transform, false);
        g2.transform.localPosition = new Vector3(layerWidth, -4.37f, 0f);
        SpriteRenderer g2Sr = g2.AddComponent<SpriteRenderer>();
        g2Sr.sprite = groundSprite;
        g2Sr.sortingOrder = -1;
        BoxCollider2D g2Col = g2.AddComponent<BoxCollider2D>();
        g2Col.size = new Vector2(layerWidth, 2.1f);
        g2Col.offset = Vector2.zero;

        ParallaxScroller groundScroller = groundGo.AddComponent<ParallaxScroller>();
        groundScroller.scrollSpeed = 3.0f;
        groundScroller.spriteWidth = layerWidth;
        groundScroller.sprite1 = g1.transform;
        groundScroller.sprite2 = g2.transform;

        // Tandai scene dirty dan pilih root di hierarchy
        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = root;

        Debug.Log("[AutoSetupBackground] Sukses! Parallax Background 5 layer telah dibuat di Scene.");
        EditorUtility.DisplayDialog(
            "Parallax Background Selesai",
            "5 Layer Background (Sky, Mountains, Clouds, Bush, Ground) berhasil dibuat dengan efek Parallax Looping otomatis!\n\nTekan Play untuk menikmati pemandangan bergulir.",
            "OK"
        );
    }

    private static Sprite LoadSprite(string path, string filenameWithoutExt)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;

        // Fallback search jika path sedikit berbeda
        string[] guids = AssetDatabase.FindAssets(filenameWithoutExt + " t:Texture2D");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.Contains("Background"))
            {
                Sprite found = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (found != null) return found;
            }
        }
        return null;
    }

    private static void CreateBushProp(Transform parent, Sprite sprite, float localX, float localY, int sortingOrder)
    {
        if (sprite == null) return;
        GameObject prop = new GameObject(sprite.name);
        prop.transform.SetParent(parent, false);
        prop.transform.localPosition = new Vector3(localX, localY, 0f);

        SpriteRenderer sr = prop.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
    }
}
