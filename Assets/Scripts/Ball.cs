using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Movement
    public int acceleration;
    public float velocity, velInitial;
    public float[] direction = new float[2];
    public float[] ballPosition = new float[2];
    public float[] finalPosition = new float[2];
    public float theta;
    
    // Triggers
    public int moveTrigger;

    // Transforms
    public Transform ball;
    
    // Classes
    public GameController gameController; // Inspector's reference


    // Start is called before the first frame update
    void Start()
    {
        ball = gameObject.transform;
        velInitial = 0;

        acceleration = 15; //5

        velInitial = 0;
        moveTrigger = 0;

        gameController = gameController.GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (moveTrigger == 1)
        {
            Move();
        }
    }

    public void StartMove(float vel, float ang)
    {
        velocity = vel;
        velInitial = vel;

        theta = ang;
        direction = new float[] { Mathf.Cos(theta), Mathf.Sin(theta) };
        EndPoint();
        moveTrigger = 1;
    }
    void Move()
    {
        ballPosition = new float[] { ball.position.x, ball.position.y };
        float varTime = Time.deltaTime;

        velocity += (-acceleration * varTime);
        velocity = Mathf.Clamp(velocity, 0, velInitial);

        if (velocity > 0)
        {
            float add = (velocity * varTime + 0.5f * acceleration * varTime * varTime);

            ballPosition[0] += direction[0] * add;
            ballPosition[1] += direction[1] * add;
        } 
        else
        {
            moveTrigger = 0;
            gameController.Point(ballPosition);
        }

        ball.position = new Vector3(ballPosition[0], ballPosition[1], -1);
    }

    void EndPoint()
    {
        float Sox = ball.position.x;
        float Soy = ball.position.y;
        float Vo = velocity;
        float a = acceleration;
        float Sx, Sy;

        if (a != 0)
        {
            Sx = (direction[0] * (Vo * Vo) / (2 * a)) + Sox;
            Sy = (direction[1] * (Vo * Vo) / (2 * a)) + Soy;
            finalPosition = new float[] { Sx, Sy };
        }
    }
}
