using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableElement : MonoBehaviour
{
    private GameElement gameElement;
    private IEnumerator movementCoroutine;

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

    public void Move(int toX, int toY, float time)
    {
        /*gameElement.X = toX;
        gameElement.Y = toY;

        gameElement.transform.localPosition = new Vector3(toX,toY,0);*/
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = MovementCoroutine(toX, toY, time);
        StartCoroutine(movementCoroutine);
    }

    private IEnumerator MovementCoroutine(int toX, int toY, float time) // interpolate between the start and end position
    {
        gameElement.X = toX;
        gameElement.Y = toY;

        Vector3 startPos = gameElement.transform.localPosition;
        Vector3 endPos = gameElement.transform.localPosition = new Vector3(toX, toY, 0);

        Debug.Log("START: " + startPos);
        Debug.Log("END: " + endPos);
        for (float t = 0.0f; t <= 1.0f * time; t += Time.deltaTime)
        {
            gameElement.transform.localPosition = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0;
        }

        gameElement.transform.localPosition = endPos;
    }
}
