using UnityEngine;

public class PulsoInteractivo : MonoBehaviour
{
    [Header("Configuración del pulso")]
    [SerializeField] public float escalaMinima = 0.85f;  // Tamaño más pequeño (85% del original)
    [SerializeField] private float duracionPulso = 0.3f;  // Duración de cada pulso (encogerse + volver)
    [SerializeField] private float intervaloEntrePulsos = 1.5f; // Tiempo entre pulsos

    private Vector3 escalaOriginal;
    private float temporizador;
    private bool encogiendo = true;
    private float progreso = 0f;

    void Start()
    {
        // Guardar la escala original del objeto
        escalaOriginal = transform.localScale;
        temporizador = 0f;
    }

    void Update()
    {
        temporizador += Time.deltaTime;

        // Calcular el progreso del pulso actual (ciclo de 0 a 1)
        float cicloPulso = (temporizador % (duracionPulso + intervaloEntrePulsos));

        if (cicloPulso < duracionPulso)
        {
            // Durante el pulso, hacer ping-pong entre 1 y escalaMinima
            float t = cicloPulso / duracionPulso; // 0 a 1

            // Usar una curva suave (ease in-out)
            float tSuave = Mathf.SmoothStep(0f, 1f, t);

            // Primero encoger, luego volver
            float factorEscala;
            if (tSuave < 0.5f)
            {
                // Encoger: de 1 a escalaMinima
                factorEscala = 1f - (tSuave * 2f) * (1f - escalaMinima);
            }
            else
            {
                // Volver: de escalaMinima a 1
                float tVuelta = (tSuave - 0.5f) * 2f;
                factorEscala = escalaMinima + tVuelta * (1f - escalaMinima);
            }

            transform.localScale = escalaOriginal * factorEscala;
        }
        else
        {
            // Asegurar tamaño original durante el intervalo
            transform.localScale = escalaOriginal;
        }
    }
}