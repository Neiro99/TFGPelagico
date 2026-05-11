using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMove : MonoBehaviour
{
    public static PlayerMove Instance { get; private set; }

    [SerializeField] float moveSpeed;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float skin;

    Rigidbody rb;
    CapsuleCollider col;
    Vector3 inputDirection;

    // Cuando es true, el inputDirection lo controla un script externo
    // (por ejemplo una cinemática), y Update() ya no lo sobreescribe con WASD.
    bool scriptedWalk;

    void Awake()
    {
        Instance = this;
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
        // Durante un walk scriptado, no leemos input: la dirección la pone la corrutina.
        if (scriptedWalk) return;

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

    // ---------------------------------------------------------------------
    // Movimiento scriptado (para cinemáticas).
    // ---------------------------------------------------------------------

    /// <summary>
    /// Mueve a Aster automáticamente hasta <paramref name="worldTarget"/> simulando
    /// que pulsa WASD: usa el mismo Rigidbody y las mismas colisiones que el
    /// movimiento normal. Llama <paramref name="onArrived"/> cuando llega
    /// (o cuando se cumple <paramref name="timeoutSeconds"/>).
    /// </summary>
    /// <param name="worldTarget">Posición destino en mundo (la Y se ignora).</param>
    /// <param name="arrivalDistance">Distancia a la que se considera que ha llegado.</param>
    /// <param name="onArrived">Callback al llegar (o al hacer timeout). Puede ser null.</param>
    /// <param name="timeoutSeconds">Tiempo máximo de seguridad para no bloquear la cinemática si nunca llega.</param>
    public void WalkTo(Vector3 worldTarget, float arrivalDistance, Action onArrived, float timeoutSeconds = 5f)
    {
        StopAllCoroutines();
        StartCoroutine(WalkToRoutine(worldTarget, arrivalDistance, onArrived, timeoutSeconds));
    }

    /// <summary>
    /// Interrumpe cualquier walk scriptado en curso y deja a Aster quieto.
    /// </summary>
    public void StopScriptedWalk()
    {
        StopAllCoroutines();
        scriptedWalk = false;
        inputDirection = Vector3.zero;
        InputManager.ClearScriptedMovement();
    }

    private IEnumerator WalkToRoutine(Vector3 worldTarget, float arrivalDistance, Action onArrived, float timeoutSeconds)
    {
        scriptedWalk = true;

        float arrivalSqr = Mathf.Max(arrivalDistance * arrivalDistance, 0.0001f);
        float elapsed = 0f;

        // Umbral para decidir si una componente cuenta como "dirección activa".
        // Por encima, fijamos el bool correspondiente en InputManager.
        const float dirThreshold = 0.1f;

        while (true)
        {
            Vector3 to = worldTarget - rb.position;
            to.y = 0f;

            if (to.sqrMagnitude <= arrivalSqr) break;
            if (elapsed >= timeoutSeconds) break;

            Vector3 dir = to.normalized;
            inputDirection = dir;

            // Replicamos en InputManager el equivalente WASD de esta dirección,
            // para que IdleLogic (y cualquier otro que escuche esos bools)
            // pinte la animación de caminar como si el jugador estuviera
            // pulsando las teclas correspondientes.
            bool up    = dir.z >  dirThreshold;
            bool down  = dir.z < -dirThreshold;
            bool right = dir.x >  dirThreshold;
            bool left  = dir.x < -dirThreshold;
            InputManager.SetScriptedMovement(up, down, left, right);

            elapsed += Time.deltaTime;
            yield return null;
        }

        inputDirection = Vector3.zero;
        scriptedWalk = false;
        InputManager.ClearScriptedMovement();
        onArrived?.Invoke();
    }
}
