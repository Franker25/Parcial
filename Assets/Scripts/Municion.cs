using UnityEngine;

public class Municion : MonoBehaviour
{
    public int cantidad = 5;

    void OnTriggerEnter(Collider otro)
    {
        Jugador jugador = otro.GetComponent<Jugador>();

        if (jugador != null)
        {
            jugador.SumarBalas(cantidad);
            Destroy(gameObject);
        }
    }
}
