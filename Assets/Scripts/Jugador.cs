using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    public Vector3 posicionPrimeraPersona = new Vector3(0f, 0.6f, 0f);
    public Vector3 posicionTerceraPersona = new Vector3(0.6f, 0.4f, -3f);

    public int[] inventario = new int[5];

    private Rigidbody rb;
    private float rotacionX = 0f;
    private float tiempoUltimoDisparo = -100f;
    private bool enSuelo = false;
    private bool terceraPersona = false;
    private int monedasGuardadas = 0;

    private float vidaInicial;
    private float estaminaInicial;
    private int balasIniciales;
    private Vector3 posicionInicial;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        vidaInicial = vida;
        estaminaInicial = estamina;
        balasIniciales = balas;
        posicionInicial = transform.position;

        ActualizarCamara();
    }

    void Update()
    {
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            MostrarInventario();
        }

        if (vida <= 0)
        {
            vida = 0;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                Reaparecer();
            }

            return;
        }

        Mirar();
        CambiarCamara();
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

        ActualizarCamara();
    }

    void CambiarCamara()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            terceraPersona = !terceraPersona;
            ActualizarCamara();
        }
    }

    void ActualizarCamara()
    {
        Quaternion rotacion = Quaternion.Euler(rotacionX, 0, 0);
        camara.transform.localRotation = rotacion;

        if (terceraPersona)
        {
            camara.transform.localPosition = posicionPrimeraPersona + rotacion * posicionTerceraPersona;
        }
        else
        {
            camara.transform.localPosition = posicionPrimeraPersona;
        }
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
        enSuelo = Physics.Raycast(transform.position, Vector3.down, 1.1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
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
            float adelanto = Vector3.Dot(transform.position - rayo.origin, rayo.direction);
            adelanto = Mathf.Max(adelanto, 0f);
            Vector3 origen = rayo.origin + rayo.direction * adelanto;

            int capaOriginal = gameObject.layer;
            gameObject.layer = 2;

            RaycastHit impacto;
            bool acierto = Physics.Raycast(origen, rayo.direction, out impacto, rango, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            gameObject.layer = capaOriginal;

            if (acierto)
            {
                Enemigo enemigo = impacto.collider.GetComponent<Enemigo>();

                if (enemigo != null)
                {
                    enemigo.RecibirDano(dano);
                }
            }
        }
    }

    void Reaparecer()
    {
        vida = vidaInicial;
        estamina = estaminaInicial;
        balas = balasIniciales;
        transform.position = posicionInicial;
        rb.linearVelocity = Vector3.zero;
    }

    void MostrarInventario()
    {
        Debug.Log("MONEDAS TOTALES EN INVENTARIO: " + monedasGuardadas);
    }

    public bool AgregarMoneda(int valor)
    {
        if (monedasGuardadas >= inventario.Length)
        {
            return false;
        }

        inventario[monedasGuardadas] = valor;
        monedasGuardadas++;
        return true;
    }

    public void SumarBalas(int cantidad)
    {
        balas += cantidad;
        Debug.Log("Balas: " + balas);
    }

    public void RecibirDano(float cantidad)
    {
        vida = Mathf.Max(vida - cantidad, 0f);
        Debug.Log("Vida del jugador: " + vida);
    }
}