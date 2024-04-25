using UnityEngine;

public class PrefabContainer : MonoBehaviour
{
    public static PrefabContainer i;
    private void Awake()
    {
        if (i) Destroy(this);
        else i = this;
    }
}
