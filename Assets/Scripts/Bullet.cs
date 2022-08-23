using System;
using UnityEngine;
using Flee.PublicTypes;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private Func<double, double> trajectoryEquation;
    private bool wasFired = false;
    [SerializeField]
    private float xAxisSwipeSpeed = 10000.0f;
    private float initialX;
    private float initialY;
    private ulong playerId;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (IsClient && IsOwner)
        {
            if (wasFired)
            {
                if (GameManager.Instance.gamePhase.Value != GameManager.GamePhase.SHOOTING)
                {
                    // destroy the bullet if it is not in the shooting phase
                    Destroy(gameObject);
                }
                else
                {
                    float newX = transform.position.x + xAxisSwipeSpeed * Time.deltaTime;
                    transform.position = new Vector3(newX, (float)trajectoryEquation(newX - initialX) + initialY, transform.position.z);
                }
            }
        }
    }

    public void CreateBullet(Vector3 position, Func<double, double> equation, ulong id)
    {
        playerId = id;
        transform.position = position;
        trajectoryEquation = equation;
        initialX = position.x;
        initialY = position.y;
    }

    public void FireBullet()
    {
        if (!wasFired)
        {
            wasFired = true;
        }
    }

    /// <summary>
    /// Creates a function that returns the y-coordinate of the bullet's trajectory at a given x-coordinate,
    /// given the equation written as f(x) in a string.
    /// </summary>
    /// <param name="equation">string containing the equation in x</param>
    /// <returns></returns>
    public static Func<double, double> CreateEquation(string equation)
    {
        ExpressionContext context = new ExpressionContext();
        context.Variables["x"] = 0.0;
        context.Imports.AddType(typeof(Math));

        // catch exceptions
        try
        {
            IGenericExpression<double> e = context.CompileGeneric<double>(equation);
            return (double x) =>
            {
                context.Variables["x"] = x;
                return e.Evaluate();
            };
        }
        catch (Exception ex)
        {
            Debug.LogError("[Equation Parser Error] " + ex.Message);
            return null;
        }
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.tag == "Player")
        {
            GameManager.Instance.BulletHitPlayerServerRpc(other.GetComponent<Player>().playerId);
        }
        else if (other.tag == "Walls")
        {
            GameManager.Instance.BulletHitWallServerRpc();
        }
    }
}
