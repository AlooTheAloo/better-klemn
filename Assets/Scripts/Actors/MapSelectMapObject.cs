using Nova;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class MapSelectMapObject : MonoBehaviour
{
    const float SAFE_MARGIN = 5000f;
    public const float SIZE_SELECTED = 900f;
    float SIZE_UNSELECTED;
    [SerializeField] AnimationCurve courbeAnimation;
    [SerializeField] float tempsAnimation;

    [SerializeField] UIBlock2D parent;
    [SerializeField] UIBlock2D background;
    [SerializeField] private UIBlock2D defaultMap;
    
    [ReadOnly]
    public int myIndex;

    bool estSelectionne = false;


    Coroutine changerTailleRoutine;
    Coroutine changerCouleurRoutine;
    Coroutine changerBorderRoutine;
    Coroutine changerParentOpaciteRoutine;

    private void Awake()
    {
        background.Color = Color.white;
        SIZE_UNSELECTED = parent.Size.X.Value;
    }

    private void OnEnable()
    {
        SongSelectManager.onMapSelectedChanged += onMapSelectChanged;
    }
    private void OnDisable()
    {
        SongSelectManager.onMapSelectedChanged -= onMapSelectChanged;
    }


    private void onMapSelectChanged(int index)
    {
        if (myIndex == index)
        {
            estSelectionne = true;
            if(index == MapCollection.i.mapsAffichees.Count - 1 && MapCollection.i.mapsAffichees.Count != 1)
            {
                parent.Margin.Right.Value = SAFE_MARGIN;
            }
            if(changerTailleRoutine != null)
            {
                StopCoroutine(changerTailleRoutine);
            }
            if (changerCouleurRoutine != null)
            {
                StopCoroutine(changerCouleurRoutine);
            }
            if (changerBorderRoutine != null)
            {
                StopCoroutine(changerBorderRoutine);
            }

            changerTailleRoutine = StartCoroutine(changerTaille(SIZE_SELECTED));
            changerCouleurRoutine = StartCoroutine(changerCouleur(Color.gray));
            changerBorderRoutine = StartCoroutine(changerBorder(1));
            changerParentOpaciteRoutine = StartCoroutine(changerParentOpacite(1));
        }
        else if (estSelectionne)
        {
            estSelectionne = false;
            if (changerTailleRoutine != null)
            {
                StopCoroutine(changerTailleRoutine);
            }
            if (changerCouleurRoutine != null)
            {
                StopCoroutine(changerCouleurRoutine);
            }
            if (changerBorderRoutine != null)
            {
                StopCoroutine(changerBorderRoutine);
            }

            changerTailleRoutine = StartCoroutine(changerTaille(SIZE_UNSELECTED));
            changerCouleurRoutine = StartCoroutine(changerCouleur(Color.white));
            changerBorderRoutine = StartCoroutine(changerBorder(0));
            changerParentOpaciteRoutine = StartCoroutine(changerParentOpacite(0));
        }

    }

    public IEnumerator changerParentOpacite(float targetOpacite)
    {
        float opaciteOriginale = defaultMap.Color.a;
        float timer = 0;

        while(timer < tempsAnimation)
        {
            float opac = Mathf.Lerp(opaciteOriginale, targetOpacite, courbeAnimation.Evaluate(timer / tempsAnimation));

            ChangeChildrenOpacity(defaultMap, opac);
            
            yield return null;
            timer += Time.deltaTime;
        }
        ChangeChildrenOpacity(defaultMap, targetOpacite);

    }

    public void ChangeChildrenOpacity(UIBlock parent, float opacity)
    {
        Color color = parent.Color;
        color.a = opacity;
        parent.Color = color;
        if (parent.ChildCount == 0) return; // Return condition

        for (int i = 0; i < parent.ChildCount; i++)
        {
            ChangeChildrenOpacity(parent.GetChild(i), opacity); 
        }
    }
    
    
    public IEnumerator changerCouleur(Color couleur)
    {
        Color couleurInitiale = background.Color;
        float timer = 0;

        while(timer < tempsAnimation)
        {
            background.Color = Color.Lerp(couleurInitiale, couleur, courbeAnimation.Evaluate(timer / tempsAnimation));

            yield return null;
            timer += Time.deltaTime;
        }
        background.Color = couleur;
    }

    public IEnumerator changerBorder(float targetOpac)
    {
        float opacInitiale = background.Border.Color.a;
        float timer = 0;

        while(timer < tempsAnimation)
        {
            background.Border.Color.a = Mathf.Lerp(opacInitiale, targetOpac, courbeAnimation.Evaluate(timer / tempsAnimation));
            yield return null;
            timer += Time.deltaTime;
        }
        
        background.Border.Color.a = targetOpac;
    }
    

    public IEnumerator changerTaille(float target)
    {
        float tailleInitiale = parent.Size.X.Value;
        float timer = 0;
        while (timer < tempsAnimation) 
        {
            parent.Size.X.Value = Mathf.Lerp(tailleInitiale, target, courbeAnimation.Evaluate(timer / tempsAnimation));
            yield return null;
            timer += Time.deltaTime;
        }
        parent.Size.X.Value = target;
    }

}
