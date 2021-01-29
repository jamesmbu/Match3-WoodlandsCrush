using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearableElement : MonoBehaviour
{
    public AnimationClip clearAnimation;

    private bool bIsBeingCleared;

    public bool IsBeingCleared
    {
        get { return bIsBeingCleared; }
    }

    protected GameElement element;

    void Awake()
    {
        element = GetComponent<GameElement>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clear(float preDestructionTime)
    {
        bIsBeingCleared = true;
        StartCoroutine(ClearCoroutine(preDestructionTime));
    }
    private IEnumerator ClearCoroutine(float preDestructionTime)
    {
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            //animator.Play(clearAnimation.name);
            yield return new WaitForSeconds(clearAnimation.length);

            Destroy(gameObject);
        }
        else Destroy(gameObject,preDestructionTime);
        
    }
}
