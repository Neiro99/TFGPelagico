using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float moveSpeed;

    Rigidbody rb;
    Vector3 inputDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (InputManager.MoveUp) v += 1f;
        if (InputManager.MoveDown) v -= 1f;
        if (InputManager.MoveRight) h += 1f;
        if (InputManager.MoveLeft) h -= 1f;

        inputDirection = new Vector3(h, 0f, v).normalized;
    }
    void FixedUpdate()
    {
        Vector3 move = inputDirection * moveSpeed * Time.fixedDeltaTime;
        Vector3 targetPos = rb.position + move;

        rb.MovePosition(targetPos);
    }
}
