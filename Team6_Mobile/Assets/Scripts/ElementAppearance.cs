using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementAppearance : MonoBehaviour
{
    public enum AppearanceType
    {
        Fox,
        Frog,
        Rabbit,
        Deer,
        Count,
    }

    [System.Serializable]
    public struct SpriteAppearance
    {
        public AppearanceType appearance;
        public Sprite sprite;
    }

    public SpriteAppearance[] spriteAppearances;

    private AppearanceType appearance;

    public AppearanceType Appearance
    {
        get { return appearance; }
        set { SetAppearance(value); }
    }

    public int AppearancesCount
    {
        get { return spriteAppearances.Length; }
    }

    private SpriteRenderer spriteRendererRef;

    private Dictionary<AppearanceType, Sprite> spriteAppearancesDictionary;

    void Awake()
    {
        spriteRendererRef = transform.GetComponentInParent<SpriteRenderer>();
        spriteAppearancesDictionary = new Dictionary<AppearanceType, Sprite>();

        // Map the SpriteAppearances array to a dictionary (KEY: Appearance Type | VALUE: Sprite)
        for (int i = 0; i < spriteAppearances.Length; i++)
        {
            if (!spriteAppearancesDictionary.ContainsKey(spriteAppearances[i].appearance))
            {
                spriteAppearancesDictionary.Add(spriteAppearances[i].appearance, 
                    spriteAppearances[i].sprite);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAppearance(AppearanceType newAppearance)
    {
        appearance = newAppearance;
        if (spriteAppearancesDictionary.ContainsKey(newAppearance))
        {
            spriteRendererRef.sprite = spriteAppearancesDictionary[newAppearance];
        }
    }
}
