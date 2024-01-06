using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public int pointInGame = 1;

    public int[] score = { 0, 0 };

    public int[] lastTouch = new int[2]; // team, player

    public float[] courtDimensions = new float[2];

    public float timeAwait = 1.5f, timeAwaiting;
    public int timeTrigger; 

    public Text score0Text, score1Text;
    public Transform[] playersInCourt = new Transform[2]; // Inspector's reference
    public Transform sideCourt; // Inspector's reference
    public Ball ball; // Inspector's reference


    // Start is called before the first frame update
    void Start()
    {
        courtDimensions[0] = sideCourt.lossyScale.x;
        courtDimensions[1] = sideCourt.lossyScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(timeTrigger == 1)
        {
            timeAwaiting += Time.deltaTime;

            if(timeAwaiting >= timeAwait)
            {
                AfterPoint();
                timeAwaiting = 0;
                timeTrigger = 0;
            }
        }
    }

    public void Point(float[] ballPosition)
    {
        float horizontalInside = courtDimensions[0] + (ball.transform.localScale.x) / 2;
        float verticalInside = courtDimensions[1] + (ball.transform.localScale.y) / 2;

        // ~~ Verificar caso a bola toque nas quinas da quadra. 
        if (Mathf.Abs(ballPosition[0]) < horizontalInside && Mathf.Abs(ballPosition[1]) < verticalInside)
        {
            if (ballPosition[0] > 0)
            {
                score[0]++;
            }
            else
            {
                score[1]++;
            }
        }
        else
        {
            if (lastTouch[0] == 0)
            {
                score[1]++;
            }
            else
            {
                score[0]++;
            }
        }

        pointInGame = 0;
        timeTrigger = 1;
    }
    public void AfterPoint()
    {
        float servePosX = 0, servePosY = 0;
        playersInCourt[0].GetComponent<Player>().serving = 1;

        for (int i = 0; i < playersInCourt.Length; i++)
        {
            playersInCourt[i].GetComponent<Player>().Reset();

            if (playersInCourt[i].GetComponent<Player>().serving == 1)
            {
                servePosX = playersInCourt[i].GetComponent<Player>().servePosition[0];
                servePosY = playersInCourt[i].GetComponent<Player>().servePosition[1];
            }
        }
        ball.ballPosition = new float[] { servePosX, servePosY, 0 };
        ball.transform.position = new Vector3(servePosX, servePosY, 0);

        pointInGame = 1;
    }
}

