using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionTracker : MonoBehaviour
{
    
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
                elementTypeCount[appearanceNameKey]+=1;
                Debug.Log(""+appearanceNameKey+ " "+ elementTypeCount[appearanceNameKey]);
            }
        }
        
        Score += matchedElements.Count;
    }
}
