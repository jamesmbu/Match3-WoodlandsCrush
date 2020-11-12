using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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

    
    // Start is called before the first frame update
    void Start()
    {
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
                GameObject tile = (GameObject) Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
            }
        }

        // instantiate elements
        elements = new GameObject[width,height]; // the game object array is instantiated with the same dimensions as the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                elements[x,y] = (GameObject)Instantiate(elementPrefabDictionary[ElementType.NORMAL], new Vector3(x, y, 0), Quaternion.identity);
                elements[x, y].name = "Element (" + x + "," + y + ")";
                elements[x, y].transform.parent = transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
