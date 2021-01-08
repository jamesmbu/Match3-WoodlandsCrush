using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableElement : MonoBehaviour
{
    private GameElement gameElement;
    private IEnumerator movementCoroutine;
    private IEnumerator bounceBackCoroutine;

    void Awake()
    {
        gameElement = GetComponent<GameElement>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(int toX, int toY, float time, bool bounceBack)
    {
        /*gameElement.X = toX;
        gameElement.Y = toY;

        gameElement.transform.localPosition = new Vector3(toX,toY,0);*/
        int startPosX = (int)gameElement.transform.localPosition.x;
        int startPosY = (int)gameElement.transform.localPosition.y;
        Vector3 initialTransform = gameElement.transform.localPosition;
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = MovementCoroutine(toX, toY, time, bounceBack, initialTransform);
        StartCoroutine(movementCoroutine);
    }

    private IEnumerator MovementCoroutine(int toX, int toY, float time, bool BouncesBack, Vector3 InitialTransform) // interpolate between the start and end position
    {
        gameElement.X = toX;
        gameElement.Y = toY;
         
        Vector3 startPos = gameElement.transform.localPosition;
        Vector3 endPos = gameElement.transform.localPosition = new Vector3(toX, toY, 0);
        
        for (float t = 0.0f; t <= 1.0f * time; t += Time.deltaTime)
        {
            if (time > 0) // FOR LINEARLY INTERPOLATED MOVEMENTS
            {
                gameElement.transform.localPosition = Vector3.Lerp(startPos, endPos, t / time);
            }
            else // FOR INSTANT MOVEMENTS (divisions by zero cause errors)
            {
                gameElement.transform.localPosition = new Vector3(toX, toY, 0);
            }

            yield return 0;
        }

        gameElement.transform.localPosition = endPos;
        if ((int) gameElement.transform.localPosition.x == toX
            && (int) gameElement.transform.localPosition.y == toY && BouncesBack)
        {
            if (bounceBackCoroutine != null)
            {
                StopCoroutine(bounceBackCoroutine);
            }
            bounceBackCoroutine = MovementCoroutine((int)InitialTransform.x, (int)InitialTransform.y,time, false, InitialTransform);
            StartCoroutine(bounceBackCoroutine);
        }

    }
}
