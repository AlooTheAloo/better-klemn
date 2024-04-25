using System;
using System.Collections;
using System.Collections.Generic;
using Nova;
using UnityEngine;

public class MapSelectDots : MonoBehaviour
{
    [SerializeField] private GameObject dotPrefab;
    private List<UIBlock2D> dots = new List<UIBlock2D>();
    private int previouslySelectedDot = -1;
    
    private void Start()
    {
        int cnt = MapCollection.i.mapsAffichees.Count;
        for (int i = 0; i < cnt; i++)
        {
            dots.Add(Instantiate(dotPrefab, gameObject.transform).GetComponent<UIBlock2D>());
        }
        print("Dots len : " + dots.Count);
        SelectDot(0);
    }

    private void OnEnable()
    {
        SongSelectManager.onMapSelectedChanged += SelectDot;
    }
    
    private void OnDisable()
    {
        SongSelectManager.onMapSelectedChanged -= SelectDot;
    }

    private const float SELECTED_DOT_OPAC = 1;
    private const float DESELECTED_DOT_OPAC = 0.6f;

    private const float DESELECTED_DOT_SIZE = 15;
    private const float SELECTED_DOT_SiZE = 20;

    private void SelectDot(int index)
    {
        if (dots.Count == 0) return;
        Color c = Color.white;

        if (previouslySelectedDot != -1)
        {
            c.a = DESELECTED_DOT_OPAC;
            dots[previouslySelectedDot].Color = c;
            dots[previouslySelectedDot].Size.X.Value = DESELECTED_DOT_SIZE;
            dots[previouslySelectedDot].Size.Y.Value = DESELECTED_DOT_SIZE;
        }

        c.a = SELECTED_DOT_OPAC;
        dots[index].Color = c;
        dots[index].Size.X.Value = SELECTED_DOT_SiZE;
        dots[index].Size.Y.Value = SELECTED_DOT_SiZE;
        previouslySelectedDot = index;
        
    }

}
