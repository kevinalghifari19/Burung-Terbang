using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class BirdController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Kekuatan lompatan burung ke atas")]
    [SerializeField] private float jumpForce = 8f;

    [Tooltip("Gravitasi fisik burung")]
    [SerializeField] private float gravityScale = 3f;

    [Header("Rotation (Tilt)")]
    [Tooltip("Sudut rotasi maksimal ke atas saat melompat (derajat)")]
    [SerializeField] private float upwardAngle = 25f;

    [Tooltip("Sudut rotasi maksimal menukik ke bawah saat jatuh (derajat)")]
    [SerializeField] private float downwardAngle = -85f;

    [Tooltip("Kecepatan transisi rotasi")]
    [SerializeField] private float rotationSpeed = 6f;

    [Header("Boundaries & Death")]
    [Tooltip("Batas bawah Y (tanah/jatuh ke luar layar) yang menyebabkan Game Over")]
    [SerializeField] private float bottomDeathY = -5.5f;

    [Tooltip("Batas atas Y (langit-langit) yang menyebabkan Game Over")]
    [SerializeField] private float topDeathY = 6.5f;

    private Rigidbody2D rb;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    private void Update()
    {
        // Jika burung sudah mati atau game over, hentikan input dan logika
        if (isDead || (GameManager.Instance != null && GameManager.Instance.IsGameOver))
        {
            return;
        }

        // Cek jika burung jatuh ke bawah layar atau menabrak batas atas
        if (transform.position.y < bottomDeathY || transform.position.y > topDeathY)
        {
            Die();
            return;
        }

        // Deteksi input lompat (Spasi atau Klik Mouse Kiri)
        if (IsJumpTriggered())
        {
            Jump();
        }

        // Terapkan rotasi berdasarkan kecepatan vertikal burung
        HandleRotation();
    }

    private void Jump()
    {
        // Unity 6 linearVelocity
        rb.linearVelocity = new Vector2(0f, jumpForce);
    }

    private void HandleRotation()
    {
        float t = Mathf.InverseLerp(-8f, 5f, rb.linearVelocity.y);
        float targetZ = Mathf.Lerp(downwardAngle, upwardAngle, t);

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Tabrakan fisik dengan pipa atau rintangan lain
        Die();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Jika trigger bukan untuk score trigger, anggap sebagai rintangan maut
        if (!collider.CompareTag("ScoreTrigger"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    private bool IsJumpTriggered()
    {
#if ENABLE_INPUT_SYSTEM
        // New Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        // Legacy Input Manager
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) return true;
#endif

        return false;
    }
}
