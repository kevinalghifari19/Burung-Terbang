using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [Header("Scroll Speed")]
    [Tooltip("Kecepatan scroll layer ke arah kiri")]
    public float scrollSpeed = 0f;

    [Header("Looping Setup")]
    [Tooltip("Lebar satu sprite dalam unit")]
    public float spriteWidth = 19.2f;

    [Tooltip("Transform sprite pertama")]
    public Transform sprite1;

    [Tooltip("Transform sprite kedua")]
    public Transform sprite2;

    private void Start()
    {
        // Jika sprite1 dan sprite2 belum di-assign di Inspector, ambil dari 2 child pertama
        if (sprite1 == null && transform.childCount > 0)
        {
            sprite1 = transform.GetChild(0);
        }
        if (sprite2 == null && transform.childCount > 1)
        {
            sprite2 = transform.GetChild(1);
        }

        // Hitung lebar sprite otomatis dari SpriteRenderer jika spriteWidth belum diatur
        if (spriteWidth <= 0f && sprite1 != null)
        {
            SpriteRenderer sr = sprite1.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                spriteWidth = sr.bounds.size.x;
            }
        }
    }

    private void Update()
    {
        // Jika kecepatan 0 (seperti langit statis), tidak perlu bergerak
        if (scrollSpeed <= 0f) return;

        // Berhenti scroll jika Game Over agar pemandangan ikut membeku bersama player
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        float moveDistance = scrollSpeed * Time.deltaTime;

        // Logika looping menggunakan dua sprite berdampingan
        if (sprite1 != null && sprite2 != null)
        {
            // Gerakkan kedua sprite ke arah kiri
            sprite1.Translate(Vector3.left * moveDistance);
            sprite2.Translate(Vector3.left * moveDistance);

            // Jika sprite pertama sudah keluar layar kiri (position.x <= -spriteWidth)
            if (sprite1.position.x <= -spriteWidth)
            {
                // Pindahkan sprite pertama ke sebelah kanan sprite kedua
                sprite1.position = new Vector3(sprite2.position.x + spriteWidth, sprite1.position.y, sprite1.position.z);
            }

            // Jika sprite kedua sudah keluar layar kiri (position.x <= -spriteWidth)
            if (sprite2.position.x <= -spriteWidth)
            {
                // Pindahkan sprite kedua ke sebelah kanan sprite pertama
                sprite2.position = new Vector3(sprite1.position.x + spriteWidth, sprite2.position.y, sprite2.position.z);
            }
        }
        else
        {
            // Fallback pergerakan langsung transform jika layer hanya satu objek
            transform.Translate(Vector3.left * moveDistance);
        }
    }
}
