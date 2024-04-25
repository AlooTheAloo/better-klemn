using UnityEngine;

public class InputData
{
    public bool PrimaryButtonDown = false;
    public bool SecondaryButtonDown = false;

    public bool AnyButtonPressed => PrimaryButtonDown || SecondaryButtonDown;
}

public abstract class EditorInputManager : MonoBehaviour
{
    public abstract bool TryGetRay(uint controlID, out Ray ray);
}