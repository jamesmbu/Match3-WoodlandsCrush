using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // types of elements which can fill the board
    public enum ElementType
    {
        NORMAL,
        COUNT,
    }
    // grid dimensions
    public int width, height;

    // struct to define each element as having a type and prefab
    [System.Serializable]
    public struct ElementPrefab
    {
        public ElementType type;
        public GameObject prefab;
    }

    // make an array of the struct for the editor
    public ElementPrefab[] elementPrefabs;
    public GameObject tilePrefab;
    // use a dictionary for internal logic. Dictionaries are not visible for the editor
    private Dictionary<ElementType, GameObject> elementPrefabDictionary;
    
    public GameObject[,] elements;

    private Vector2 screenBounds;

    private float tileWidth;

    private float tileHeight;
    private Vector3 _position;

    // Start is called before the first frame update
    void Start()
    {
        _position = transform.position;
        _position.x = 0.0f;
        _position.y = 0.0f;
        transform.position = _position;
        // load each value from the 'ElementPrefab' array to the dictionary
        elementPrefabDictionary = new Dictionary<ElementType, GameObject>();
        for (int i = 0; i < elementPrefabs.Length; i++)
        {
            /* if the dictionary does not contain a key (which, in context, denotes a specific type)
             the type and prefab of the current index is added to the dictionary */
            if (!elementPrefabDictionary.ContainsKey(elementPrefabs[i].type))
                elementPrefabDictionary.Add(elementPrefabs[i].type, elementPrefabs[i].prefab);
        }

        // instantiate each tile on each cell of the board
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = (GameObject) Instantiate(tilePrefab, new Vector3(x,y,0), Quaternion.identity);
                tile.transform.parent = transform; 
            }
        }

        // instantiate elements (animals)
        elements = new GameObject[width,height]; // the game object array is instantiated with the same dimensions as the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                elements[x,y] = (GameObject)Instantiate(elementPrefabDictionary[ElementType.NORMAL],
                    new Vector3(x, y, 0), Quaternion.identity);
                elements[x, y].name = "Element (" + x + "," + y + ")";
                elements[x, y].transform.parent = transform;
            }
        }

        AlignBoard();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector2 GetWorldPosition(int x, int y)
    {
        screenBounds =
            Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        
        return new Vector2(transform.position.x - tileWidth - (screenBounds.x*-1) + x,
            transform.position.y - height / 2.0f + y);
    }

    void AlignBoard()
    {
        //get screen bounds
        screenBounds =
            Camera.main.ScreenToWorldPoint(new Vector3(
                Screen.width, Screen.height, Camera.main.transform.position.z));
        
        /* ~~~~~~
         WIDTH CALCULATIONS
        ~~~~~~*/

        // get width of tile (the height is the same, 1:1)
        tileWidth = tilePrefab.transform.GetComponent<SpriteRenderer>().bounds.size.x; // tile width in the world
        Debug.Log("Tile Width: " + tileWidth);
        
        // get screen width (the desired width of the board for a perfect fit)
        float screenWidth = screenBounds.x * 2.0f;
        Debug.Log("Screen Width: " + screenWidth);
        
        // get width of the board (width of all tiles in a line added up) -> (the actual board width)
        float boardWidth = tileWidth * width;
        Debug.Log("Board Width: " + boardWidth);
        
        // get the multiplier which brings the actual width to the desired width
        float desiredWidthFactor = screenWidth / boardWidth;
        Debug.Log("Desired Width Multiplier: " + desiredWidthFactor);

        // set new width values
        tileWidth *= desiredWidthFactor*0.95f; // 0.95 gives a 5% margin of space on the width (should change hardcoded value to variable)
        boardWidth *= desiredWidthFactor*0.95f;

        /* ~~~~~~
         HEIGHT CALCULATIONS
        ~~~~~~*/

        // get screen height ONLY USEFUL FOR ADJUSTING SCALE WHICH FOR HEIGHT MAY NOT BE AS USEFUL
        //float screenHeight = screenBounds.y * 2.0f;
        //Debug.Log("Screen Height: " + screenHeight);
        
        // get height of the board (height of all tiles in a column added up) -> (the actual board height)
        float boardHeight = tileWidth * height; // tile width and height are 1:1
        Debug.Log("Board Height: " + boardHeight);

        // re-scale the board
        transform.localScale *= desiredWidthFactor*0.95f;
        

        // align board to center side of screen
        transform.position = _position;
        _position.x = 0.0f - (boardWidth / 2.0f) + (tileWidth / 2);//(tileWidth / 2) + (screenBounds.x * -1); // half-width plus left-most bound of the screen
        _position.y = 0.0f - (boardHeight / 2.0f) + (tileWidth / 2);//(tileWidth / 2) + ((screenBounds.y * -1));
        transform.position = _position;
    }
}
