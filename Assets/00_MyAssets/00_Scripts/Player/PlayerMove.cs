using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float skin;

    Rigidbody rb;
    CapsuleCollider col;
    Vector3 inputDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }
    private void Start()
    {
        moveSpeed = 9f;
        collisionMask = ~0;
        skin = 0.02f;
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
        if (move.sqrMagnitude < 0.000001f) return;

        Vector3 pos = rb.position;
        float fixedY = pos.y;

        GetCapsuleWorldPoints(out Vector3 p1, out Vector3 p2, out float radius);

        if (Physics.CapsuleCast(p1, p2, radius, move.normalized, out RaycastHit hit, move.magnitude + skin,
            collisionMask, QueryTriggerInteraction.Ignore))
        {
            float allowed = Mathf.Max(0f, hit.distance - skin);
            move = move.normalized * allowed;
        }

        Vector3 targetPos = rb.position + move;
        targetPos.y = fixedY;

        rb.MovePosition(targetPos);
    }

    void GetCapsuleWorldPoints(out Vector3 p1, out Vector3 p2, out float radius)
    {
        radius = col.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);

        float height = Mathf.Max(col.height * transform.lossyScale.y, radius * 2f);
        Vector3 center = transform.TransformPoint(col.center);

        float half = (height * 0.5f) - radius;
        Vector3 up = transform.up;

        p1 = center + up * half;
        p2 = center - up * half;
    }
}
    