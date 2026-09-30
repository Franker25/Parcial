using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public float velocidad = 3f;
    public float vida = 100f;

    public float alcancePersecucion = 5f;
    public float distanciaMinima = 1.5f;

    public float rango = 5f;
    public float dano = 20f;
    public float cadencia = 2f;

    public Jugador jugador;

    private Rigidbody rb;
    private float tiempoUltimoDisparo = -100f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (vida <= 0)
        {
            vida = 0;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.transform.position);

        if (distancia <= alcancePersecucion)
        {
            Mirar();

            if (distancia > distanciaMinima)
            {
                Perseguir();
            }
            else
            {
                Detener();
            }

            Disparar();
        }
        else
        {
            Detener();
        }
    }

    void Mirar()
    {
        Vector3 objetivo = jugador.transform.position;
        objetivo.y = transform.position.y;
        transform.LookAt(objetivo);
    }

    void Perseguir()
    {
        Vector3 direccion = transform.forward * velocidad;
        rb.linearVelocity = new Vector3(direccion.x, rb.linearVelocity.y, direccion.z);
    }

    void Detener()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    void Disparar()
    {
        if (Time.time >= tiempoUltimoDisparo + cadencia)
        {
            tiempoUltimoDisparo = Time.time;

            Vector3 direccion = (jugador.transform.position - transform.position).normalized;
            RaycastHit impacto;

            if (Physics.Raycast(transform.position, direccion, out impacto, rango))
            {
                Jugador jugadorImpactado = impacto.collider.GetComponent<Jugador>();

                if (jugadorImpactado != null)
                {
                    jugadorImpactado.RecibirDano(dano);
                }
            }
        }
    }

    public void RecibirDano(float cantidad)
    {
        vida -= cantidad;
        Debug.Log("Vida del enemigo: " + vida);
    }
}