using UnityEngine;

public class Moneda : MonoBehaviour
{
    public int valor = 1;

    void OnTriggerEnter(Collider otro)
    {
        Jugador jugador = otro.GetComponent<Jugador>();

        if (jugador != null)
        {
            bool guardada = jugador.AgregarMoneda(valor);

            if (guardada)
            {
                Destroy(gameObject);
            }
        }
    }
}