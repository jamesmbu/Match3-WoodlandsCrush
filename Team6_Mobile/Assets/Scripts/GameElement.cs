using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameElement : MonoBehaviour
{
    private int x;

    private int y;

    private BoardManager.ElementType type;

    private BoardManager boardManager;

    private MovableElement movableComponent; // handler for movement

    private ElementAppearance appearanceComponent; // handler for appearance

    public int X
    {
        get { return x; }
        set
        {
            if (isMovable())
            {
                x = value;
            }
        }
    }
    public int Y
    {
        get { return y; }
        set
        {
            if (isMovable())
            {
                y = value;
            }
        }
    }

    public BoardManager.ElementType Type
    {
        get { return type; }
    }
    public BoardManager BoardManagerRef
    {
        get { return boardManager; }
    }
    public MovableElement MovableComponent
    {
        get { return movableComponent; }
    }
    public ElementAppearance AppearanceComponent
    {
        get { return appearanceComponent; }
    }

    void Awake()
    {
        movableComponent = GetComponent<MovableElement>();
        appearanceComponent = GetComponent<ElementAppearance>();

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    public void DestroyMe()
    {
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(int _x, int _y, BoardManager _boardManager, BoardManager.ElementType _type)
    {
        x = _x;
        y = _y;
        boardManager = _boardManager;
        type = _type;
    }

    public bool isMovable()
    {
        return movableComponent != null;
    }
    public bool appearanceIsSet()
    {
        return appearanceComponent != null;
    }
    
}


