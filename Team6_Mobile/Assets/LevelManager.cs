using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        Debug.Log("Loaded level");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
