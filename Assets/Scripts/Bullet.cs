using System;
using UnityEngine;
using Flee.PublicTypes;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private Func<float, float> trajectoryEquation;
    private bool wasFired = false;
    private float xAxisSwipeSpeed = 0.1f;

    public void Spawn(Vector3 position, string equation)
    {
        transform.position = position;
        trajectoryEquation = createEquation(equation);
    }

    public static Func<float, float> createEquation(string equation)
    {
        ExpressionContext context = new ExpressionContext();
        context.Variables["x"] = 0.0;

        IDynamicExpression e = context.CompileDynamic(equation);

        return (float x) =>
        {
            context.Variables["x"] = x;
            return (float)e.Evaluate();
        };
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (wasFired)
            {
                float newX = transform.position.x + xAxisSwipeSpeed * Time.deltaTime;
                transform.position = new Vector3(newX, trajectoryEquation(newX), transform.position.z);
            }
        }
    }
}
