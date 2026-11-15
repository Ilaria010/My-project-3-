
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;

    [Header("Slide Settings")]
    public float slideSpeed = 9f;
    public float slideAngle = 15f;

    [Header("Slope Jump Settings")]
    public float slopeJumpForce = 5f;
    public float minSlopeJumpAngle = 25f;
    public float slopeJumpCooldown = 0.3f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isDead;
    private bool isSliding;

    private Vector2 slopeDirection;
    private float lastSlopeJumpTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDead) return;

        float move = Input.GetAxisRaw("Horizontal");

        if (!isSliding)
        {
            rb.linearVelocity = new Vector2(
                move * speed,
                rb.linearVelocity.y
            );
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;
            isSliding = false;
        }
    }

    void FixedUpdate()
    {
        if (isDead || !isSliding) return;

        rb.linearVelocity = new Vector2(
            slopeDirection.x * slideSpeed,
            slopeDirection.y * slideSpeed
        );
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
            return;

        isGrounded = true;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            float angle = Vector2.Angle(
                contact.normal,
                Vector2.up
            );

            if (angle > slideAngle)
            {
                if (!isSliding &&
                    angle >= minSlopeJumpAngle &&
                    Time.time >= lastSlopeJumpTime + slopeJumpCooldown)
                {
                    rb.AddForce(
                        Vector2.up * slopeJumpForce,
                        ForceMode2D.Impulse
                    );

                    lastSlopeJumpTime = Time.time;
                }

                isSliding = true;

                slopeDirection = Vector2.Perpendicular(
                    contact.normal
                );

                if (slopeDirection.y > 0)
                {
                    slopeDirection = -slopeDirection;
                }

                break;
            }
            else
            {
                isSliding = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            isSliding = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Death"))
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.linearVelocity = Vector2.zero;

        Invoke("RestartLevel", 0.1f);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
