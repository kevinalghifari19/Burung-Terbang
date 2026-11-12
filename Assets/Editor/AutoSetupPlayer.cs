using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class AutoSetupPlayer
{
    private const string SPRITE_PATH = "Assets/Jovial Games/Basic 2D Side Scroller Art Kit/Sprites/Player/Player.png";
    private const string ANIMATIONS_DIR = "Assets/Animations";
    private const string CLIP_PATH = "Assets/Animations/Fly.anim";
    private const string CONTROLLER_PATH = "Assets/Animations/PlayerController.controller";

    [MenuItem("Tools/Flappy Bird/Auto Setup Player", false, 1)]
    public static void SetupPlayer()
    {
        // 1. Cari sprite Player_0, Player_1, Player_2
        Sprite sprite0 = null;
        Sprite sprite1 = null;
        Sprite sprite2 = null;

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(SPRITE_PATH);
        if (assets != null && assets.Length > 0)
        {
            foreach (Object obj in assets)
            {
                if (obj is Sprite sprite)
                {
                    if (sprite.name == "Player_0") sprite0 = sprite;
                    else if (sprite.name == "Player_1") sprite1 = sprite;
                    else if (sprite.name == "Player_2") sprite2 = sprite;
                }
            }
        }

        // Fallback jika path spesifik tidak ditemukan
        if (sprite0 == null || sprite1 == null || sprite2 == null)
        {
            string[] guids = AssetDatabase.FindAssets("Player t:Texture2D");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains("Sprites/Player"))
                {
                    Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (Object obj in subAssets)
                    {
                        if (obj is Sprite s)
                        {
                            if (s.name == "Player_0") sprite0 = s;
                            else if (s.name == "Player_1") sprite1 = s;
                            else if (s.name == "Player_2") sprite2 = s;
                        }
                    }
                }
            }
        }

        if (sprite0 == null || sprite1 == null || sprite2 == null)
        {
            EditorUtility.DisplayDialog(
                "Error",
                "Sprite Player_0, Player_1, atau Player_2 tidak ditemukan di folder Sprites/Player!\nPastikan aset sudah diimpor.",
                "OK"
            );
            return;
        }

        // 2. Pastikan folder Assets/Animations tersedia
        if (!AssetDatabase.IsValidFolder(ANIMATIONS_DIR))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        // 3. Buat AnimationClip (Fly.anim) dengan loop time dan durasi 0.1s per frame
        AnimationClip clip = new AnimationClip
        {
            name = "Fly",
            frameRate = 10 // 10 FPS = 0.1s per frame
        };

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        // 3 frame berdurasi masing-masing 0.1s:
        // 0.0s -> Player_0, 0.1s -> Player_1, 0.2s -> Player_2, 0.3s -> Player_2 (menjaga frame ke-3 tetap aktif sampai 0.3s)
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[4];
        keyframes[0] = new ObjectReferenceKeyframe { time = 0.0f, value = sprite0 };
        keyframes[1] = new ObjectReferenceKeyframe { time = 0.1f, value = sprite1 };
        keyframes[2] = new ObjectReferenceKeyframe { time = 0.2f, value = sprite2 };
        keyframes[3] = new ObjectReferenceKeyframe { time = 0.3f, value = sprite2 };

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        // Aktifkan loop time
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        AssetDatabase.CreateAsset(clip, CLIP_PATH);

        // 4. Buat Animator Controller dan set Fly.anim sebagai default state
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH) != null)
        {
            AssetDatabase.DeleteAsset(CONTROLLER_PATH);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState flyState = stateMachine.AddState("Fly");
        flyState.motion = clip;
        stateMachine.defaultState = flyState;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 5. Cek apakah Player sudah ada di Scene
        GameObject existingPlayer = GameObject.Find("Player");
        if (existingPlayer != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "Player Sudah Ada",
                "GameObject 'Player' sudah ada di Scene aktif. Apakah Anda ingin me-replace?",
                "Ya, Ganti",
                "Batal"
            );

            if (!replace) return;
            Undo.DestroyObjectImmediate(existingPlayer);
        }

        // 6. Buat GameObject Player di Scene aktif pada koordinat (0, 0, 0)
        GameObject player = new GameObject("Player");
        Undo.RegisterCreatedObjectUndo(player, "Auto Setup Player");
        player.transform.position = Vector3.zero;

        // 7. Tambahkan SpriteRenderer
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = sprite0;
        sr.sortingOrder = 5;

        // 8. Tambahkan Animator
        Animator animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        // 9. Tambahkan Rigidbody2D dengan konfigurasi spesifik
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // 10. Tambahkan CircleCollider2D
        CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.65f;
        collider.offset = Vector2.zero;

        // 11. Tambahkan BirdController (script mandiri sebelumnya)
        player.AddComponent<BirdController>();

        // 12. Pilih objek di editor dan tandai scene dirty
        Selection.activeGameObject = player;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[AutoSetupPlayer] Sukses! GameObject 'Player' berhasil dibuat dengan Rigidbody2D, CircleCollider2D, Animator (Fly.anim loop), dan BirdController.");
        EditorUtility.DisplayDialog(
            "Setup Selesai",
            "GameObject 'Player' berhasil dibuat di koordinat (0,0,0) lengkap dengan Rigidbody2D, Collider, Animator, dan BirdController!",
            "OK"
        );
    }
}
