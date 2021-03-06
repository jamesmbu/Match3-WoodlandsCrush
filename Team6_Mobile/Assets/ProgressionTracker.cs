using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionTracker : MonoBehaviour
{
    public GameObject Canvas;
    public GameObject WinPanel;
    public GameObject LosePanel;
    public Text DisplayTurnsText;
    public int Turns;
    private bool hasWon;

    // Public struct for setting conditions from the editor
    [System.Serializable]
    public struct WinConditions
    {
        public ElementAppearance.AppearanceType type;
        public int WinRequirement;
        public bool WinReached;
        public Text DisplayText;
        public Image DisplayImage;
    }



    public WinConditions[] LevelWinConditions;
    private int WinConditionsCount, ConditionsMet;
    
    [SerializeField]
    private int Score = 0;

    public Dictionary<ElementAppearance.AppearanceType, int> elementTypeCount = new Dictionary<ElementAppearance.AppearanceType, int>();
    // Start is called before the first frame update

    public 
    void Start()
    {
        Canvas = GameObject.Find("Canvas"); 
        WinPanel = GameObject.Find("Canvas/WinPanel");
        LosePanel = GameObject.Find("Canvas/LosePanel");
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
        DisplayTurnsText = GameObject.Find("TurnsText").GetComponent<Text>();
        hasWon = false;

        /* determine how many win conditions there are */
        for (int i = 0; i < LevelWinConditions.Length; i++)
        {
            // if there is a win condition set for the element
            if (LevelWinConditions[i].WinRequirement > 0) // 0 means win isn't dependent on the specific element, only look to those above 0
            {
                WinConditionsCount++;
            }
            /* set initial text displays to show the total required */
            if (LevelWinConditions[i].DisplayText)
            {
                LevelWinConditions[i].DisplayText.text =
                    "0" + "/" + LevelWinConditions[i].WinRequirement.ToString();
            }
        }

        if (DisplayTurnsText)
        {
            DisplayTurnsText.text = "Moves: " + Turns.ToString();
        }
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
        
        /* Display text */
        for (int i = 0; i < LevelWinConditions.Length; i++)
        {
            if (LevelWinConditions[i].DisplayText && elementTypeCount.ContainsKey(LevelWinConditions[i].type))
            {
                if (elementTypeCount[LevelWinConditions[i].type] >= LevelWinConditions[i].WinRequirement)
                {
                    LevelWinConditions[i].DisplayText.color = Color.green; // green text when goal is met
                }
                LevelWinConditions[i].DisplayText.text = 
                    elementTypeCount[LevelWinConditions[i].type].ToString() + "/" + 
                    LevelWinConditions[i].WinRequirement.ToString();
            }
        }

        /* Determine distance from win condition(s) */
        for (int i = 0; i < LevelWinConditions.Length; i++)
        {
            // if there is a win condition set for the element
            if (LevelWinConditions[i].WinRequirement > 0) // 0 means win isn't dependent on the specific element, only look to those above 0
            {
                
                if (elementTypeCount.ContainsKey(LevelWinConditions[i].type))
                {
                    // if the current count of the element is equivalent or exceeds the win requirement...
                    if (elementTypeCount[LevelWinConditions[i].type] >= LevelWinConditions[i].WinRequirement
                    && !LevelWinConditions[i].WinReached)// ... and if the condition hasn't already been met
                    {
                        LevelWinConditions[i].WinReached = true;
                        ConditionsMet++;
                        if (ConditionsMet >= WinConditionsCount)
                        {
                            hasWon=true;
                            Win();
                        }
                    }
                }
            }
        }

        /* Increment score */
        Score += matchedElements.Count;
    }

    void Win()
    {
        // enable the end-of-level UI
        WinPanel.SetActive(true);
        Canvas.GetComponent<Canvas>().sortingOrder = 5;
    }

    void Lose()
    {
        // enable the end-of-level UI
        LosePanel.SetActive(true);
        Canvas.GetComponent<Canvas>().sortingOrder = 5;
    }
    /* Notes that a turn has been made */
    public void UseTurn()
    {
        Turns--;
        DisplayTurnsText.text = "Moves: " + Turns.ToString();
        if (Turns <= 0 && !hasWon)
        {
            Lose();
        }
    }
    
}
