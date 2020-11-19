using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableElement : MonoBehaviour
{
    private GameElement gameElement;

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

    public void Move(int toX, int toY)
    {
        Debug.Log(toX);
        gameElement.X = toX;
        gameElement.Y = toY;

        gameElement.transform.localPosition = new Vector3(toX,toY,0);
    }
}
