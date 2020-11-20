using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    // types of elements which can fill the board
    public enum ElementType
    {
        Empty,
        Normal,
        Obstacle,
        Count,
    }
    // grid dimensions
    public int width, height;

    // time for the grid to fill up with elements
    public float fillTime = 0.5f;

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
    
    public GameElement[,] elements;

    private Vector2 screenBounds;

    private float tileWidth;

    private float tileHeight;

    private int topRowIndex;

    private bool inverse = false;
    private Vector3 _position;

    // Start is called before the first frame update
    void Start()
    {
        topRowIndex = height - 1;
        // set initial location before anything is done
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
        elements = new GameElement[width,height]; // the game object array is instantiated with the same dimensions as the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Fill the board with empty prefabs initially. The actual animals will fall onto the board soon after.
                SpawnElement(x, y, ElementType.Empty);
                /*GameObject newElement = (GameObject)Instantiate(elementPrefabDictionary[ElementType.Normal],
                    Vector3.zero, Quaternion.identity, transform);

                newElement.name = "Element (" + x + "," + y + ")";

                elements[x, y] = newElement.GetComponent<GameElement>();
                elements[x, y].Init(x, y, this, ElementType.Normal);

                // Move the animal element locally to the board
                if (elements[x, y].isMovable())
                {
                    elements[x, y].MovableComponent.Move(x, y);
                }
                // Set appearance of animal
                if (elements[x, y].appearanceIsSet())
                {
                    elements[x, y].AppearanceComponent.SetAppearance((ElementAppearance.AppearanceType)Random.Range(0, elements[x, y].AppearanceComponent.AppearancesCount));
                }*/
            }
        }
        Destroy(elements[3, 3].gameObject);
        SpawnElement(3, 3, ElementType.Obstacle);
        AlignBoard();
        
        
        StartCoroutine(Fill());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Fill()
    {
        // Debug.Log("Starts here (IENUM)");
        while (FillStep())
        {
            /* Function is called until it returns false */
            inverse = !inverse;
            yield return new WaitForSeconds(fillTime);
        }

    }

    public bool FillStep()
    {
        bool bMovedPiece = false;
        // looping from bottom to top of board
        for (int y = 1; y < height; y++) // starts at index 1, as for index 0 there is nothing below it.
        {
            for (int loopX = 0; loopX < width; loopX++)
            {
                int x = loopX;
                if (inverse)
                {
                    x = width - 1 - loopX; // makes loop go from end -> start
                }

                GameElement element = elements[x, y];
                if (element.isMovable())
                {
                    // Downwards Movement
                    GameElement elementBelow = elements[x, y - 1];
                    if (elementBelow.Type == ElementType.Empty)
                    {
                        Destroy(elementBelow.gameObject);
                        element.MovableComponent.Move(x, y - 1, fillTime); 
                        elements[x, y - 1] = element; // bring the element to its new space below
                        SpawnElement(x, y, ElementType.Empty); // spawn empty element where the element just moved from
                        bMovedPiece = true;
                    }
                    // Diagonal Movement
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++) // -1: left; +1: right
                        {
                            if (diag == 0) continue; 
                            // yield the index which will denote the position either to the left or right
                            int diagX = x + diag;

                            if (inverse)
                            {
                                diagX = x - diag;
                            }

                            if (diagX < 0 || diagX >= width) continue; // check if the value is within the bounds of the board

                            GameElement diagonalPiece = elements[diagX, y - 1]; // get ref to the element below diagonally to the element

                            if (diagonalPiece.Type != ElementType.Empty) continue; // check if the value is empty; if so, it needs filling

                            bool bHasPieceAbove = true;

                            for (int aboveY = y; aboveY >= 0; aboveY--) // loop through whatever is above the diagonal subject
                            {
                                GameElement pieceAbove = elements[diagX, aboveY];

                                if (pieceAbove.isMovable()) // if there is a movable element above
                                {
                                    break; // end the loop; the movable element above the diagonal subject element will fill the space instead
                                }
                                else if (!pieceAbove.isMovable() && pieceAbove.Type != ElementType.Empty) // if the element above cannot move and isn't empty
                                {
                                    bHasPieceAbove = false;
                                    break;
                                }
                            }

                            if (!bHasPieceAbove)
                            {   // then diagonal movement commences
                                Destroy(diagonalPiece.gameObject);
                                element.MovableComponent.Move(diagX, y - 1, fillTime);
                                elements[diagX, y - 1] = element;
                                SpawnElement(x, y, ElementType.Empty);
                                bMovedPiece = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
        // check the top row for empty pieces
        for (int x = 0; x < width; x++)
        {
            
            GameElement elementBelow = elements[x, topRowIndex];

            if (elementBelow.Type == ElementType.Empty)
            {
                Destroy(elementBelow.gameObject);
                
                GameObject newElement = (GameObject) Instantiate(elementPrefabDictionary[ElementType.Normal],
                    Vector3.zero, Quaternion.identity, transform);
                
                if (newElement.GetComponent<MovableElement>())
                {
                    newElement.GetComponent<MovableElement>().Move(x,topRowIndex+1,0);
                }
                elements[x, topRowIndex] = newElement.GetComponent<GameElement>();
                elements[x, topRowIndex].Init(x, -1, this, ElementType.Normal);
                elements[x, topRowIndex].MovableComponent.Move(x, topRowIndex, fillTime);
                elements[x, topRowIndex].AppearanceComponent.SetAppearance((ElementAppearance.AppearanceType)
                    Random.Range(0, elements[x, topRowIndex].AppearanceComponent.AppearancesCount));
                bMovedPiece = true;
            }
        }
        /*// looping from bottom to top of board, as it makes sense logically
        for (int y = 1; y <height; y++) // -2 means that the bottom row isn't checked- there is nothing below it.
        {
            for (int x = 0; x < width; x++)
            {
                
                GameElement element = elements[x, y];
                if (element.isMovable())
                {
                    GameElement elementBelow = elements[x, y - 1]; 
                    if (elementBelow.Type == ElementType.Empty)
                    {
                        element.MovableComponent.Move(x, y - 1, fillTime); 
                        elements[x, y - 1] = element; // bring the element to its new space below
                        SpawnElement(x, y, ElementType.Empty); // spawn empty element where the element just moved from
                        bMovedPiece = true;
                    }
                }
            }
        }
        // check the top row for empty pieces
        for (int x = 0; x < width; x++)
        {
            GameElement elementBelow = elements[x, height-1];

            if (elementBelow.Type == ElementType.Empty)
            {
                GameObject newElement = (GameObject) Instantiate(elementPrefabDictionary[ElementType.Normal],
                    new Vector3(x, -1), Quaternion.identity, transform);

                elements[x, height-1] = newElement.GetComponent<GameElement>();
                elements[x, height-1].Init(x, height+1, this, ElementType.Normal);
                elements[x, height-1].MovableComponent.Move(x, height, fillTime);
                elements[x, height-1].AppearanceComponent.SetAppearance((ElementAppearance.AppearanceType)
                    Random.Range(0, elements[x, height-1].AppearanceComponent.AppearancesCount));
                bMovedPiece = true;
            }
        }*/
        return bMovedPiece;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - width / 2.0f + x,
            transform.position.y + height / 2.0f - y);
    }

    public GameElement SpawnElement(int x, int y, ElementType type)
    { 
        GameObject newElement = (GameObject) Instantiate(elementPrefabDictionary[type],
            new Vector3(x, y, 0), Quaternion.identity, transform);

        elements[x, y] = newElement.GetComponent<GameElement>();
        elements[x, y].Init(x, y, this, type);

        return elements[x, y];
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
        //Debug.Log("Tile Width: " + tileWidth);
        
        // get screen width (the desired width of the board for a perfect fit)
        float screenWidth = screenBounds.x * 2.0f;
        //Debug.Log("Screen Width: " + screenWidth);
        
        // get width of the board (width of all tiles in a line added up) -> (the actual board width)
        float boardWidth = tileWidth * width;
        //Debug.Log("Board Width: " + boardWidth);
        
        // get the multiplier which brings the actual width to the desired width
        float desiredWidthFactor = screenWidth / boardWidth;
        //Debug.Log("Desired Width Multiplier: " + desiredWidthFactor);

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
        //Debug.Log("Board Height: " + boardHeight);

        // re-scale the board
        transform.localScale *= desiredWidthFactor*0.95f;
        

        // align board to center side of screen
        transform.position = _position;
        _position.x = 0.0f - (boardWidth / 2.0f) + (tileWidth / 2);//(tileWidth / 2) + (screenBounds.x * -1); // half-width plus left-most bound of the screen
        _position.y = 0.0f - (boardHeight / 2.0f) + (tileWidth / 2);//(tileWidth / 2) + ((screenBounds.y * -1));
        transform.position = _position;
    }
}
