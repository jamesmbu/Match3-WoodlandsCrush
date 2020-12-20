using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionTracker : MonoBehaviour
{
    [System.Serializable]
    public struct WinConditions
    {
        public ElementAppearance.AppearanceType type;
        public int WinRequirement;
    }

    public WinConditions[] LevelWinConditions;

    [SerializeField]
    private int Score = 0;

    public Dictionary<ElementAppearance.AppearanceType, int> elementTypeCount = new Dictionary<ElementAppearance.AppearanceType, int>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateProgress(List<GameElement> matchedElements)
    {
        /* Count elements by type */
        for (int i = 0; i < matchedElements.Count; i++)
        {
            ElementAppearance.AppearanceType appearanceNameKey = matchedElements[i].AppearanceComponent.Appearance;
            if (!elementTypeCount.ContainsKey(appearanceNameKey)) // if the key is not in the dictionary yet
            {
                //Debug.Log("Doesn't contain Key: " + appearanceName);
                elementTypeCount.Add(appearanceNameKey, 1); // initialize the animal in the dictionary, first count of this animal
            }
            else // key is in the dictionary
            {
                elementTypeCount[appearanceNameKey]+=1; // increment the counter for this animal
                //Debug.Log(""+appearanceNameKey+ " "+ elementTypeCount[appearanceNameKey]); // out latest element count
            }
        }
        /* Determine distance from win condition(s) */
        for (int i = 0; i < LevelWinConditions.Length; i++)
        {
            if (LevelWinConditions[i].WinRequirement > 0) // 0 means win isn't dependent on the specific element, only look to those above 0
            {
                if (elementTypeCount.ContainsKey(LevelWinConditions[i].type))
                {
                    // access the key, then do other stuff
                }
            }
        }


        /* Increment score */
        Score += matchedElements.Count;
    }
}
