using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    // Types of elements which can fill the board
    public enum ElementType
    {
        Empty,
        Normal,
        Obstacle,
        Count,
    }
    // Reference to the level manager
    private GameObject levelManagerRef;
    private ProgressionTracker progressionTracker;

    // Tracker for completion of initial generation
    private bool levelGenerated = false;
    // Grid dimensions
    public int width, height;

    // Struct to define each element as having a type and prefab
    [System.Serializable]
    public struct ElementPrefab
    {
        public ElementType type;
        public GameObject prefab;
    }

    // Make an array of the struct for the editor
    public ElementPrefab[] elementPrefabs;
    [System.Serializable]
    public struct ObstacleInsert
    {
        public int X, Y;
    }
    public ObstacleInsert[] ObstacleInserts;
    // Tile prefab
    public GameObject tilePrefab;

    // Use a dictionary for internal logic. Dictionaries are not visible for the editor
    private Dictionary<ElementType, GameObject> elementPrefabDictionary;
    
    // 2D array of all elements of the board
    public GameElement[,] elements;

    // Grid Alignment & Scaling
    private Vector2 screenBounds;
    private float tileWidth;
    private float tileHeight;
    
    // Player Interaction
    private GameElement pressedElement;
    private GameElement enteredElement;

    // Miscellaneous
    public float fillTime = 0.5f; // animation time
    private int topRowIndex;
    private bool inverse = false;
    private Vector3 _position;
    public float outerMargin = 0.95f;
    private bool boardIsRefilling;
    void Awake()
    {
        levelManagerRef = GameObject.Find("LevelManager");
        progressionTracker = levelManagerRef.gameObject.GetComponent<ProgressionTracker>();
    }

    // Start is called before the first frame update
    void Start()
    {
        boardIsRefilling = false;
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

        if (ObstacleInserts.Length > 0)
        {
            for (int i = 0; i < ObstacleInserts.Length; i++)
            {
                Destroy(elements[ObstacleInserts[i].X, ObstacleInserts[i].Y].gameObject);
                SpawnElement(ObstacleInserts[i].X, ObstacleInserts[i].Y, ElementType.Obstacle);
            }
        }

        AlignBoard();
        
        
        StartCoroutine(Fill());
    }

    // Update is called once per frame
    void Update()
    {
    }

    public IEnumerator Fill()
    {
        boardIsRefilling = true;
        bool bNeedsRefill = true;
        // Debug.Log("Starts here (IENUM)");
        while (bNeedsRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {
                
                /* Function is called until it returns false */
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }
            bNeedsRefill = ClearAllValidMatches();
        }

        levelGenerated = true; // marks the point when the board has been completely filled. Now, score will start to be tracked, obstacles can be cleared
        boardIsRefilling = false;
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
                        element.MovableComponent.Move(x, y - 1, fillTime,false); 
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
                                element.MovableComponent.Move(diagX, y - 1, fillTime,false);
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
                    newElement.GetComponent<MovableElement>().Move(x,topRowIndex+1,0,false);
                }
                elements[x, topRowIndex] = newElement.GetComponent<GameElement>();
                elements[x, topRowIndex].Init(x, -1, this, ElementType.Normal);
                elements[x, topRowIndex].MovableComponent.Move(x, topRowIndex, fillTime,false);
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
        tileWidth *= desiredWidthFactor*outerMargin; // 0.95 gives a 5% margin of space on the width (should change hardcoded value to variable)
        boardWidth *= desiredWidthFactor*outerMargin;

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
        transform.localScale *= desiredWidthFactor*outerMargin;
        

        // align board to center side of screen
        transform.position = _position;
        _position.x = 0.0f - (boardWidth / 2.0f) + (tileWidth / 2);//(tileWidth / 2) + (screenBounds.x * -1); // half-width plus left-most bound of the screen
        _position.y = 0.0f - (boardHeight / 2.0f) + (tileWidth / 2);//(tileWidth / 2) + ((screenBounds.y * -1));
        transform.position = _position;
    }

    public bool IsAdjacent(GameElement element1, GameElement element2) // Checking if two elements are next to each other (1 tile square radius)
    {
        return (element1.X == element2.X && (int) Mathf.Abs(element1.Y - element2.Y) == 1
                || element1.Y == element2.Y && (int)Mathf.Abs(element1.X - element2.X) == 1);
    }

    public void SwapElements(GameElement element1, GameElement element2) // Swap two elements positions
    { 
        if (element1.isMovable() && element2.isMovable()) // If both elements have movement capabilities
        {
            // Swap the elements in the array
            elements[element1.X, element1.Y] = element2;
            elements[element2.X, element2.Y] = element1;

            // Store one of the elements X and Y position to use as reference
            int element1X = element1.X;
            int element1Y = element1.Y;
            // Store one of the elements X and Y position to use as reference
            int element2X = element2.X;
            int element2Y = element2.Y;

            // Allow only matchable elements; check if they 
            if (GetMatch(element1, element2X, element2Y) != null ||
                GetMatch(element2, element1X, element1Y) != null)
            {
                /*// Store one of the elements X and Y position to use as reference
                int element1X = element1.X;
                int element1Y = element1.Y;*/

                // Facilitate the visual movement of the pair 
                element1.MovableComponent.Move(element2X, element2Y, fillTime,false);
                element2.MovableComponent.Move(element1X, element1Y, fillTime,false);

                // Clearing elements
                ClearAllValidMatches();
                StartCoroutine(Fill());
                progressionTracker.UseTurn();
            }
            else // Revert to original positions
            {
                elements[element1.X, element1.Y] = element1;
                elements[element2.X, element2.Y] = element2;
                // Facilitate the visual movement of the pair to indicate invalid move
                element1.MovableComponent.Move(element2X, element2Y, fillTime, true);
                element2.MovableComponent.Move(element1X, element1Y, fillTime, true);

                
            }
            
        }
    }

    public void PressElement(GameElement element)
    {
        if (!boardIsRefilling) pressedElement = element;
    }
    public void EnterElement(GameElement element)
    {
        if (element != pressedElement && pressedElement && Input.GetMouseButton(0))
        {
            enteredElement = element;
            ReleaseElement();
        }
        
    }

    public void ReleaseElement()
    {
        if (IsAdjacent(pressedElement, enteredElement)) // if they're next to each other
        { 
            SwapElements(pressedElement, enteredElement); // attempt a swap
        }

        pressedElement = null;
        enteredElement = null;
    }

    public List<GameElement> GetMatch(GameElement element, int newX, int newY)
    {
        if (element.appearanceIsSet())
        {
            ElementAppearance.AppearanceType appearance = element.AppearanceComponent.Appearance;
            
            // Initialize filler arrays
            List<GameElement> horizontalElements = new List<GameElement>();
            List<GameElement> verticalElements = new List<GameElement>();
            List<GameElement> matchingElements = new List<GameElement>();

            // First, check horizontally
            horizontalElements.Add(element);

            for (int dir = 0; dir <= 1; dir++) // directional determinant loop
            {
                for (int xOffset = 1; xOffset < width; xOffset++)
                {
                    int x;

                    if (dir == 0) x = newX - xOffset; // Left

                    else x = newX + xOffset; // Right

                    if (x < 0 || x >= width) break; // Out of bounds

                    // Check if the adjacent element is a match
                    if (elements[x, newY].appearanceIsSet() &&
                        elements[x, newY].AppearanceComponent.Appearance == appearance)
                    {
                        horizontalElements.Add(elements[x, newY]);
                    }
                    else break;
                }
            }

            if (horizontalElements.Count >= 3)
            {
                matchingElements.AddRange(horizontalElements);
            }

            // Traverse each element vertically if a match is found (L and T match checks)
            if (horizontalElements.Count >= 3)
            {
                for (int i = 0; i < horizontalElements.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int yOffset = 1; yOffset < height; yOffset++)
                        {
                            int y;

                            if (dir == 0) y = newY - yOffset; // Up
                            else y = newY + yOffset; // Down

                            if (y < 0 || y >= height) break; // Out of bounds 

                            if (elements[horizontalElements[i].X, y].appearanceIsSet() &&
                                elements[horizontalElements[i].X, y].AppearanceComponent.Appearance == appearance)
                            {
                                verticalElements.Add(elements[horizontalElements[i].X, y]);
                            }
                            else break;
                        }
                    }
                    if (verticalElements.Count < 2) verticalElements.Clear(); // clear the way for the next iteration
                    else
                    {
                        matchingElements.AddRange(verticalElements);
                        break;
                    }
                }
            }

            if (matchingElements.Count >= 3)
            {
                return matchingElements;
            }
            
            horizontalElements.Clear();
            verticalElements.Clear();
            matchingElements.Clear();
            // Secondly, check vertically (no matches found from horizontal traversal)
            verticalElements.Add(element);

            for (int dir = 0; dir <= 1; dir++) // directional determinant loop
            {
                for (int yOffset = 1; yOffset < height; yOffset++)
                {
                    int y;

                    if (dir == 0) y = newY - yOffset; // Up

                    else y = newY + yOffset; // Down


                    if (y < 0 || y >= height) break; // Out of bounds

                    // Check if the adjacent element is a match
                    if (elements[newX, y].appearanceIsSet() &&
                        elements[newX, y].AppearanceComponent.Appearance == appearance)
                    {
                        verticalElements.Add(elements[newX, y]);
                    }
                    else break;
                }
            }
            
            if (verticalElements.Count >= 3)
            {
                matchingElements.AddRange(verticalElements);
            }
            
            // Traverse each element horizontally if a match is found (L and T match checks)
            if (verticalElements.Count >= 3)
            {
                for (int i = 0; i < verticalElements.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int xOffset = 1; xOffset < width; xOffset++)
                        {
                            int x;

                            if (dir == 0) x = newX - xOffset; // Left
                            else x = newX + xOffset; // Right

                            if (x < 0 || x >= width) break; // Out of bounds 

                            if (elements[x, verticalElements[i].Y].appearanceIsSet() &&
                                elements[x, verticalElements[i].Y].AppearanceComponent.Appearance == appearance)
                            {
                                horizontalElements.Add(elements[x, verticalElements[i].Y]);
                            }
                            else break;
                        }
                    }
                    if (horizontalElements.Count < 2) horizontalElements.Clear(); // clear the way for the next iteration
                    else
                    {
                        matchingElements.AddRange(horizontalElements);
                        break;
                    }
                }
            }
            
            if (matchingElements.Count >= 3)
            {
                return matchingElements;
            }
        }

        return null;
    }

    public bool ClearAllValidMatches()
    {
        bool bNeedsRefill = false;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (elements[x, y].isClearable())
                {
                    List<GameElement> match = GetMatch(elements[x, y], x, y); // populate list with matched elements
                    

                    if (match != null) // if not empty (then matches were found)
                    {
                        if (levelGenerated) progressionTracker.UpdateProgress(match); //increment score if map is loaded properly
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (ClearElement(match[i].X, match[i].Y)) bNeedsRefill = true; // clear the element- if it worked, mark for refill
                        }
                    }
                }
            }
        }

        return bNeedsRefill;
    }

    public bool ClearElement(int x, int y)
    {
        if (elements[x, y].isClearable() && !elements[x, y].ClearableComponent.IsBeingCleared)
        {
            elements[x, y].ClearableComponent.Clear(fillTime);
            SpawnElement(x, y, ElementType.Empty); // Spawn an empty element in place of the removed
            ClearObstacles(x, y);
            //if (levelGenerated) progressionTracker.UpdateProgress(1);
            return true;
        }
        return false;
    }

    public void ClearObstacles(int x, int y) // checks adjacent elements to identify obstacles
    {
        if (!levelGenerated) return; // waits until the level has been fully generated. Otherwise, obstacles will be potentially cleared without user effort.
        for (int adjX = x - 1; adjX <= x + 1; adjX++)
        {
            if (adjX != x && adjX >= 0 && adjX < width) // only for adjacent elements
            {
                if (elements[adjX, y].Type == ElementType.Obstacle &&
                    elements[adjX, y].isClearable()) // if next to a clearable obstacle
                {
                    elements[adjX, y].health -= 1;
                    if (elements[adjX, y].health <= 0)
                    {
                        elements[adjX, y].ClearableComponent.Clear(fillTime); // removal of obstacle
                        SpawnElement(adjX, y, ElementType.Empty); // replace with empty element
                    }
                    
                }
                
            }

        }
        for (int adjY = y - 1; adjY <= y + 1; adjY++)
        {
            if (adjY != y && adjY >= 0 && adjY < height) // only for adjacent elements
            {
                if (elements[x, adjY].Type == ElementType.Obstacle &&
                    elements[x, adjY].isClearable()) // if next to a clearable obstacle
                {
                    elements[x, adjY].health -= 1;
                    if (elements[x, adjY].health <= 0)
                    {
                        elements[x, adjY].ClearableComponent.Clear(fillTime); // removal of obstacle
                        SpawnElement(x, adjY, ElementType.Empty); // replace with empty element
                    }

                }

            }

        }
    }
}
