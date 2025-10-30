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
        moveSpeed = 5f;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector3(h, 0f, v).normalized;
    }
    private void OnEnable()
    {
        GameManager.ChangeScene += ChangeScene;
        GameManager.OnPlay += OnPlay;
    }

    private void OnDisable()
    {
        GameManager.ChangeScene -= ChangeScene;
    }

    void FixedUpdate()
    {
        Vector3 move = inputDirection * moveSpeed * Time.fixedDeltaTime;
        Vector3 targetPos = rb.position + move;

        rb.MovePosition(targetPos);
    }

    void ChangeScene()
    {
        moveSpeed = 0;
    }
    void OnPlay()
    {
        moveSpeed = 5f;
    }
}
