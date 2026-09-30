using UnityEngine;
using UnityEngine.InputSystem;

public class Jugador : MonoBehaviour
{
    public float velocidad = 6f;
    public float fuerzaSalto = 6f;
    public float sensibilidadMouse = 0.1f;

    public float vida = 100f;
    public float estamina = 10f;
    public float costoSalto = 5f;

    public float rango = 20f;
    public float dano = 25f;
    public float cadencia = 1.5f;
    public int balas = 10;

    public Camera camara;

    private Rigidbody rb;
    private float rotacionX = 0f;
    private float tiempoUltimoDisparo = -100f;
    private bool enSuelo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (vida <= 0)
        {
            vida = 0;
            return;
        }

        Mirar();
        Mover();
        RevisarSuelo();
        Saltar();
        ControlarEstamina();
        ControlarFOV();
        Disparar();
    }

    void Mirar()
    {
        Vector2 mouse = Mouse.current.delta.ReadValue();

        float mouseX = mouse.x * sensibilidadMouse;
        float mouseY = mouse.y * sensibilidadMouse;

        transform.Rotate(0, mouseX, 0);

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);
        camara.transform.localRotation = Quaternion.Euler(rotacionX, 0, 0);
    }

    void Mover()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.dKey.isPressed) horizontal += 1f;
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.wKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed) vertical -= 1f;

        Vector3 direccion = transform.right * horizontal + transform.forward * vertical;
        direccion = direccion.normalized * velocidad;

        rb.linearVelocity = new Vector3(direccion.x, rb.linearVelocity.y, direccion.z);
    }

    void RevisarSuelo()
    {
        enSuelo = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    void Saltar()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && enSuelo && estamina >= costoSalto)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            estamina -= costoSalto;
        }
    }

    void ControlarEstamina()
    {
        estamina += 2f * Time.deltaTime;
        estamina = Mathf.Clamp(estamina, 0f, 10f);
    }

    void ControlarFOV()
    {
        if (Keyboard.current.tKey.isPressed)
        {
            camara.fieldOfView -= 30f * Time.deltaTime;
        }

        if (Keyboard.current.yKey.isPressed)
        {
            camara.fieldOfView += 30f * Time.deltaTime;
        }

        camara.fieldOfView = Mathf.Clamp(camara.fieldOfView, 50f, 120f);
    }

    void Disparar()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && balas > 0 && Time.time >= tiempoUltimoDisparo + cadencia)
        {
            tiempoUltimoDisparo = Time.time;
            balas--;

            Ray rayo = camara.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit impacto;

            if (Physics.Raycast(rayo, out impacto, rango))
            {
                Enemigo enemigo = impacto.collider.GetComponent<Enemigo>();

                if (enemigo != null)
                {
                    enemigo.RecibirDano(dano);
                }
            }
        }
    }

    public void RecibirDano(float cantidad)
    {
        vida -= cantidad;
        Debug.Log("Vida del jugador: " + vida);
    }

    public void SumarBalas(int cantidad)
    {
        balas += cantidad;
        Debug.Log("Balas: " + balas);
    }
}