using UnityEngine;

/// <summary>
/// DESCRIPCIÓN:
/// Fecha:
/// Autor:
/// </summary>
public class MouseManager : MonoBehaviour
{
    //*************************************************************************************************************
    #region 1 Definicion de variables


    [Header("CONTENIDO => MIRILLA")]
    public RectTransform mirillaLimite;
    public RectTransform mirilla;

    #endregion 1
    //*************************************************************************************************************
    #region 2 Funciones de Unity

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void Update()
    {
        MoverMirillaDentroLimites();
    }

    #endregion 2
    //*************************************************************************************************************
    #region 3 Mis funciones

    void MoverMirillaDentroLimites()
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mirillaLimite,
            Input.mousePosition,
            null,
            out localPoint
        );

        // Tamaño del área disponible (restando el tamaño del hijo para que no sobresalga)
        Vector2 halfSize = mirilla.rect.size * 0.5f;
        Vector2 minBounds = mirillaLimite.rect.min + halfSize;
        Vector2 maxBounds = mirillaLimite.rect.max - halfSize;

        // Clampeamos la posición dentro de los límites
        localPoint.x = Mathf.Clamp(localPoint.x, minBounds.x, maxBounds.x);
        localPoint.y = Mathf.Clamp(localPoint.y, minBounds.y, maxBounds.y);

        mirilla.anchoredPosition = localPoint;
    }

    #endregion 3
    //*************************************************************************************************************
}
