using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProgressDisplay : MonoBehaviour
{
    public int Element;

    private int CurrentMatches;

    private int RequiredMatches;

    private ProgressionTracker progTracker;
    // Start is called before the first frame update
    void Start()
    {
        progTracker = LevelManager.FindObjectOfType<ProgressionTracker>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
