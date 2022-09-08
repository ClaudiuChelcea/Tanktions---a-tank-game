using System;
using UnityEngine;
using Flee.PublicTypes;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private Func<double, double> trajectoryEquation;
    private string trajectoryEquationString;
    private bool wasFired = false;
    private float xAxisSwipeSpeed = 1.5f;
    private float initialX;
    private float initialY;
    private float prevX;
    private float prevY;
    private ulong playerId;
    private bool stopped = false;
    private int baseDamage = 50;


    // Start is called before the first frame update
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            trajectoryEquationString = ArenaUIManager.Instance.GetEquation();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsClient && IsOwner && wasFired && !stopped)
        {
            if (IsServer)
            {
                float newX = transform.position.x + xAxisSwipeSpeed * Time.deltaTime;
                transform.position = new Vector3(newX, (float)trajectoryEquation(newX - initialX) + initialY, transform.position.z);
            }
            else
            {
                float newX = transform.position.x - xAxisSwipeSpeed * Time.deltaTime;
                transform.position = new Vector3(newX, -(float)trajectoryEquation(initialX - newX) + initialY, transform.position.z);
            }

            // rotate bullet to the angle determined by the old coordinates and the new coordinates
            float angle = Mathf.Atan2(transform.position.y - prevY, transform.position.x - prevX) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            prevX = transform.position.x;
            prevY = transform.position.y;
        }
    }

    public bool CreateBullet(Vector3 position, ulong id)
    {
        playerId = id;
        transform.position = position;
        trajectoryEquation = CreateEquation(trajectoryEquationString);
        if (trajectoryEquation == null)
        {
            return false;
        }

        initialX = position.x;
        if (IsServer)
        {
            initialY = position.y - (float)trajectoryEquation(0.0);
        }
        else
        {
            initialY = position.y + (float)trajectoryEquation(0.0);
        }

        // return false if initialY is not a number or is infinite
        if (float.IsNaN(initialY) || float.IsInfinity(initialY))
        {
            return false;
        }

        prevX = initialX;
        prevY = initialY;

        return true;
    }

    [ClientRpc]
    public void FireBulletClientRpc(Vector3 position, ulong id)
    {
        if (IsOwner)
        {
            if (!wasFired)
            {
                if (CreateBullet(position, id))
                {
                    wasFired = true;
                    Debug.Log("Fired bullet with equation: " + trajectoryEquationString);
                }
                else
                {
                    Debug.Log("Failed to fire bullet");
                    DespawnServerRpc();
                    GameManager.Instance.BulletFireFailedServerRpc();
                }
            }
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

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (stopped)
        {
            return;
        }

        GameObject other = collider.gameObject;
        if (other.tag == "Player")
        {
            GameManager.Instance.BulletHitPlayerServerRpc(other.GetComponent<Player>().playerId, baseDamage);
        }
        else if (other.tag == "Obstacle")
        {
            GameManager.Instance.BulletHitObstacleServerRpc();
        }
        else
        {
            return;
        }

        stopped = true;
        DespawnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void DespawnServerRpc()
    {
        Debug.Log("Destroying bullet");
        GetComponent<NetworkObject>().Despawn();
    }
}
