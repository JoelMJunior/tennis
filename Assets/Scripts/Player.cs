using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id = 0, team = 0;
    
    // Movement
    public float velocity;
    public int acceleration, maxVelocity;
    public float[] direction = new float[2];
    
    // Positions
    public float[] initialPosition = new float[2]; // Inspector's reference
    public float[] servePosition = new float[2]; // Inspector's reference
    public float[] defensePosition = new float[2];
    public float[] positioningPosition = new float[2];
    public float[] positionCourt = new float[2];

    // Jump
    public int jumpUp, jumpDown;
    public float jumpVelocity = 0.85f, maxJumpSize = 1.1f;

    // Serve
    public int serving = 0;
    public float serveMaxDistance = 2.0f;
    public float[] serveFinalPos = new float[2];

    // Ball
    public float[] ballFuturePosition = new float[2];

    // Dimensions
    public float deltaDistance = 0.05f;
    public float originalScale;

    // Triggers
    public int jumpTrigger, serveTrigger, defenseTrigger, positioningTrigger, 
        attackTrigger; // New trigger, update DisableAllTriggers()

    // Transforms
    public Transform player;
    public Transform playerVs; // Inspector's reference

    // Classes
    public GameController gameController; // Inspector's reference
    public Ball ball; // Inspector's reference


    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.transform;
        acceleration = 3; maxVelocity = 5;
        
        positionCourt[0] = transform.position.x;
        positionCourt[1] = transform.position.y;

        initialPosition[0] = transform.position.x;
        initialPosition[1] = transform.position.y;

        if (team == 0)
        {
            positioningPosition = new float[] { -6.40f, 0 };
        }
        else
        {
            positioningPosition = new float[] { 6.40f, 0 };
        }

        originalScale = transform.localScale.x;

        gameController = gameController.GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpTrigger = 1;
            jumpUp = 1; jumpDown = 0;
        }
        if (jumpTrigger == 1)
        {
            Jump();
        }

        // Serve
        if (Input.GetKeyDown(KeyCode.S) && serving == 1)
        {
            StartServe();
        }
        if (serveTrigger == 1)
        {
            Serve();
        }

        // Defense
        if (defenseTrigger == 1)
        {
            Defense();
        }

        if (positioningTrigger == 1)
        {
            Positioning();
        }
    }

    void Move(float[] direction)
    {
        positionCourt = new float[] { player.position.x, player.position.y }; 
        float varTime = Time.deltaTime;

        velocity += (acceleration * Time.deltaTime);
        velocity = Mathf.Clamp(velocity, 0, maxVelocity);

        float add = (velocity * varTime + 0.5f * acceleration * varTime * varTime);

        positionCourt[0] += direction[0] * add;
        positionCourt[1] += direction[1] * add;

        player.position = new Vector3(positionCourt[0], positionCourt[1], 0);
    }

    public void StartPositioning()
    {
        positioningTrigger = 1;
        float[] pos = CalcPosition(team, id);
        direction = CalcAngle(pos);
    }
    void Positioning()
    {
        float[] pos = CalcPosition(team, id);
        float distanceToPos = CalcDistance(pos);
        
        if (distanceToPos > deltaDistance)
        {
            Move(direction);
        } 
        else
        {
            positioningTrigger = 0;
            velocity = 0;
        }
    }

    void Jump()
    {
        int direction;
        Vector3 jumpChange;
        if (jumpUp == 1)
        {
            direction = 1;
            float i = direction * jumpVelocity * Time.deltaTime;
            jumpChange = new Vector3(i, i);
            player.localScale += jumpChange;
            if (player.localScale.x >= maxJumpSize)
            {
                jumpUp = 0;
                jumpDown = 1;
                if (attackTrigger == 1)
                {
                    float ang = AngAttack();
                    Attack(25, ang); // ~~ Ajusta o valor 25
                }
            }
        }
        else if (jumpDown == 1)
        {
            direction = -1;
            float i = direction * jumpVelocity * Time.deltaTime;
            jumpChange = new Vector3(i, i);
            player.localScale += jumpChange;
            if (player.localScale.x <= 0.85f)
            {
                jumpUp = 1;
                jumpDown = 0;
                jumpTrigger = 0;
                attackTrigger = 0;
                player.localScale = new Vector3(originalScale, originalScale, 1);
            }
        }
    }

    public void StartServe()
    {
        jumpTrigger = 1; jumpUp = 1; jumpDown = 0;
        serveTrigger = 1; attackTrigger = 1;

        if(team == 0)
        {
            serveFinalPos[0] = servePosition[0] + serveMaxDistance;
            serveFinalPos[1] = transform.position.y;
        }
        else
        {
            serveFinalPos[0] = servePosition[0] - serveMaxDistance;
            serveFinalPos[1] = transform.position.y;
        }

        float angDirection;
        if (team == 0)
        {
            angDirection = 0;
        }
        else
        {
            angDirection = Mathf.PI;
        }
        direction = new float[] { Mathf.Cos(angDirection), Mathf.Sin(angDirection) };

        serving = 0;
    }
    void Serve()
    {
        float serveDistance = CalcDistance(serveFinalPos);
        if (serveDistance > deltaDistance)
        {
            Move(direction);
        }
        else
        {
            serveTrigger = 0;
            StartPositioning();
        }
    }

    public void StartDefense(float[] ballFutPos)
    {
        ballFuturePosition = ballFutPos;
        Debug.Log(ballFutPos[0] +" "+ ballFutPos[1]);
        DisableAllTriggers();

        direction = CalcAngle(ballFutPos);

        int aux = BallComeToMySide(ballFutPos);

        Debug.Log(aux);
        defenseTrigger = aux;
    }
    void Defense()
    {
        float distanceToFutureBall = CalcDistance(ballFuturePosition);

        if (distanceToFutureBall > deltaDistance)
        {
            Move(direction);
        } 
        else
        {
            float distanceToPosBall = CalcDistance(ball.ballPosition);

            if (distanceToPosBall <= deltaDistance)
            {
                float ang = AngAttack();
                Attack(15, ang); // ~~ Ajusta o valor 15
                StartPositioning(); 
                defenseTrigger = 0;
                velocity = 0;
            }
        }
    }

    void Attack(float force, float ang)
    {
        if (gameController.pointInGame == 1)
        {
            ball.StartMove(force, ang); 
            gameController.lastTouch = new int[] { team, id };
            float[] ballFutPos = ball.finalPosition;
            playerVs.GetComponent<Player>().StartDefense(ballFutPos);
        }
    }
    
    void ServePosition()
    {
        if(serving == 1)
        {
            player.position = new Vector3(servePosition[0], servePosition[1], 0);
        }
        else
        {
            player.position = new Vector3(initialPosition[0], initialPosition[1], 0);
        }
    }

    public void DisableAllTriggers()
    {
        jumpTrigger = 0; serveTrigger = 0; defenseTrigger = 0; positioningTrigger = 0; attackTrigger = 0;
    }
    public void Reset()
    {
        velocity = 0;
        DisableAllTriggers();
        ServePosition();
    }

    float AngAttack()
    {
        float posX = transform.position.x;
        float posY = transform.position.y;
        
        float angMin = Mathf.Atan((gameController.courtDimensions[1]/2 + posY) / posX); ;
        float angMax = Mathf.Atan(-(gameController.courtDimensions[1]/2 - posY) / posX); ;

        if (team == 1)
        {
            angMin += Mathf.PI;
            angMax += Mathf.PI;
        }

        float ang = Random.Range(angMin, angMax);
        return ang;
    }
    
    float CalcDistance(float[] finalPos)
    {
        float posX = finalPos[0] - player.position.x;
        float posY = finalPos[1] - player.position.y;
        float distance = Mathf.Sqrt(posX * posX + posY * posY);

        return distance;
    } 
    float[] CalcAngle(float[] finalPos)
    {
        float posX = finalPos[0] - player.position.x;
        float posY = finalPos[1] - player.position.y;
        float distance = Mathf.Sqrt(posX * posX + posY * posY);

        float[] direction = new float[] { (posX / distance), (posY / distance) };
        return direction;
    }
    float[] CalcPosition(int team, int id)
    {
        // ~~ Improve to 2 players in team
        float[] newPosition = positioningPosition;
        return newPosition;
    }

    int BallComeToMySide(float[] ballFutPos)
    {
        if (team == 0)
        {
            if (ballFutPos[0] > 0 || ballFutPos[0] < -(gameController.courtDimensions[0] + ball.transform.localScale.x / 2))
            {
                return 0;
            }
            else if (Mathf.Abs(ballFutPos[1]) > (gameController.courtDimensions[1]/2 + ball.transform.localScale.y / 2))
            {
                return 0;
            }
        }
        else
        {
            if (ballFutPos[0] < 0 || ballFutPos[0] > (gameController.courtDimensions[0] + ball.transform.localScale.x / 2))
            {
                return 0;
            }
            else if (Mathf.Abs(ballFutPos[1]) > (gameController.courtDimensions[1]/2 + ball.transform.localScale.y / 2))
            {
                return 0;
            }
        }
        return 1;
    }
}
