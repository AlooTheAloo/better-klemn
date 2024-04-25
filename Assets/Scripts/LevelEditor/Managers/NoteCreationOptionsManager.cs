using System;
using NovaSamples.UIControls;
using UnityEngine;

public class NoteCreationOptionsManager: MonoBehaviour
{
    [SerializeField] private Button clickNoteButton;
    [SerializeField] private Button holdNoteButton;
    [SerializeField] private Button groupeEffectButton;
    [SerializeField] private Button cancelButton;
    
    public static event Action OnClickNoteSelected;
    public static event Action OnHoldNoteSelected;
    public static event Action OnGroupEffectSelected;
    
    private void OnEnable()
    {
        clickNoteButton.OnClicked.AddListener(ClickNoteButtonOnClick);
        holdNoteButton.OnClicked.AddListener(HoldNoteButtonOnClick);
        groupeEffectButton.OnClicked.AddListener(GroupEffectButtonOnClick);
        cancelButton.OnClicked.AddListener(CancelButtonOnClick);
    }
    
    private void OnDisable()
    {
        clickNoteButton.OnClicked.RemoveListener(ClickNoteButtonOnClick);
        holdNoteButton.OnClicked.RemoveListener(HoldNoteButtonOnClick);
        groupeEffectButton.OnClicked.RemoveListener(GroupEffectButtonOnClick);
        cancelButton.OnClicked.RemoveListener(CancelButtonOnClick);
    }
    
    private void ClickNoteButtonOnClick()
    {
        OnClickNoteSelected?.Invoke();
        gameObject.SetActive(false);
    }
    
    private void HoldNoteButtonOnClick()
    {
        OnHoldNoteSelected?.Invoke();
        gameObject.SetActive(false);
    }
    
    private void GroupEffectButtonOnClick()
    {
        OnGroupEffectSelected?.Invoke();
        gameObject.SetActive(false);
    }
    
    private void CancelButtonOnClick()
    {
        gameObject.SetActive(false);
    }
}