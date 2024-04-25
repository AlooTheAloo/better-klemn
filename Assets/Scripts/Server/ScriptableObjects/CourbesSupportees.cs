using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Courbes", menuName = "Chroma Assets/CourbesSupportees", order = 1)]
public class CourbesSupportees : SerializedScriptableObject
{
    [SerializeField] public List<CourbeLabel> courbeLabels;

    public AnimationCurve GetCourbeFromLabel(string label)
    {
        return courbeLabels.First(x => x.label == label).courbe;
    }
}

public struct CourbeLabel
{
    public string label;
    public AnimationCurve courbe;
}
