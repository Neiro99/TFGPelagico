    using UnityEngine;

    public class IdleLogic : MonoBehaviour
    {
        public Animator animator;

        float idleTimer;
        float idleRepeatTimer;

        public float idleStartDelay;
        public float idleRepeatDelay;


        void IdleChange()
        {
            animator.SetBool("IdleChange", false);
        }

    void Update()
    {
        bool up = InputManager.MoveUp;
        bool down = InputManager.MoveDown;
        bool left = InputManager.MoveLeft;
        bool right = InputManager.MoveRight;

        // 1) Cancelaciones (opuestos)
        bool vertical = up ^ down;       // true si solo uno está activo
        bool horizontal = left ^ right;  // true si solo uno está activo

        // 2) Prioridad: vertical > horizontal
        bool moveUp = vertical && up;
        bool moveDown = vertical && down;

        bool moveLeft = !vertical && horizontal && left;
        bool moveRight = !vertical && horizontal && right;

        // 3) Enviar al Animator: SOLO una dirección verdadera
        animator.SetBool("MoveUp", moveUp);
        animator.SetBool("MoveDown", moveDown);
        animator.SetBool("MoveLeft", moveLeft);
        animator.SetBool("MoveRight", moveRight);

        bool isMoving = moveUp || moveDown || moveLeft || moveRight;

        if (isMoving)
        {
            idleTimer = 0f;
            idleRepeatTimer = 0f;
            animator.SetBool("IdleChange", false);
        }
        else
        {
            idleTimer += Time.deltaTime;

            // Aún no llegó a los 6 segundos
            if (idleTimer < idleStartDelay)
            {
                animator.SetBool("IdleChange", false);
            }
            else
            {
                // Ya pasó el primer trigger (6s)
                idleRepeatTimer += Time.deltaTime;

                if (idleRepeatTimer >= idleRepeatDelay)
                {
                    animator.SetBool("IdleChange", true);
                    idleRepeatTimer = 0f;
                }
            }
        }


    }

}
