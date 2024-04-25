using Chroma.Editor;
using Nova;
using NovaSamples.UIControls;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct BPMKeyPress
{
    public float time;
    public BPMKeyPress(float time)
    {
        this.time = time;
    }
}

[Serializable]
public struct BPMFinderPanels
{
    public UIBlock2D panneauAvant;
    public UIBlock2D penneauDurant;
    public UIBlock2D penneauApres;
}

public class MapBpmFinder : MonoBehaviour
{
    const int NOMBRE_MINIMAL_CLICKS = 5;
    private const KeyCode BPM_FINDING_KEY = KeyCode.Space;
    private const KeyCode BPM_EXIT_KEY = KeyCode.Escape;

    [Header("Refs")]
    public BPMFinderPanels panneaux;
    public AudioSource source;
    public TextBlock panneauDurantInstruction;
    [SerializeField] internal TextBlock bpmBlock;


    [HideInInspector] public List<BPMKeyPress> keyPresses = new List<BPMKeyPress>();

    internal float BPM;
    
    



    private BPMFinderState etatActuel;

    private void Awake()
    {
        SetEtat(new AttenteBPMFinderState(this));   
    }

    private void Update()
    {
        if (Input.GetKeyDown(BPM_FINDING_KEY))
        {
            etatActuel.onMainPressed();
        }

        if (Input.GetKeyDown(BPM_EXIT_KEY))
        {
            etatActuel.onExitPressed();
        }
    }

    public void SetEtat(BPMFinderState etat)
    {
        etatActuel = etat;
        etatActuel.onStart();
    }

    public void AjouterKeyPress(BPMKeyPress e)
    {
        keyPresses.Add(e);
        CalculerBPM();
        if(keyPresses.Count < NOMBRE_MINIMAL_CLICKS)
        {
            panneauDurantInstruction.Text = $"Encore {NOMBRE_MINIMAL_CLICKS - keyPresses.Count} fois !";
        }
        else panneauDurantInstruction.Text = $"{Mathf.Round(BPM * 100) / 100} BPM";
    }

    private void CalculerBPM()
    {
        if (keyPresses.Count < NOMBRE_MINIMAL_CLICKS) { return; }
        float tempsEcoule = keyPresses.Last().time - keyPresses.First().time;
        float nombreNotes = keyPresses.Count - 1; // On ignore la première note car t = 0 => f(t) != ~E
        BPM = Constantes.SECONDES_DANS_MINUTE * nombreNotes / tempsEcoule;
    }

    public void SetPaneauActif(UIBlock2D panneau)
    {
        typeof(BPMFinderPanels).GetFields().ForEach(x =>
        {
            ((UIBlock2D)x.GetValue(this.panneaux)).gameObject.SetActive(false);
        });
        panneau.gameObject.SetActive(true);
    }

    internal void afficherResultats()
    {
        bpmBlock.Text = Mathf.Round(BPM).ToString() + " BPM";
    }
}

public abstract class BPMFinderState
{
    protected MapBpmFinder instance;
    protected BPMFinderState(MapBpmFinder instance)
    {
        this.instance = instance;
    }
    public abstract void onMainPressed();
    public abstract void onExitPressed();
    public abstract void onStart();
    
    
}

internal class AttenteBPMFinderState : BPMFinderState
{
    public AttenteBPMFinderState(MapBpmFinder instance) : base(instance) {}

    public override void onExitPressed()
    {
        
    }

    public override void onMainPressed()
    {
        instance.SetEtat(new EnCoursBPMFinderState(instance));
    }

    public override void onStart()
    {
        instance.SetPaneauActif(instance.panneaux.panneauAvant);
        instance.keyPresses.Clear();
    }
}

internal class EnCoursBPMFinderState : BPMFinderState
{
    public EnCoursBPMFinderState(MapBpmFinder instance) : base(instance) { }

    public override void onExitPressed()
    {
        instance.SetEtat(new TermineBPMFinderState(instance));
    }

    public override void onMainPressed()
    {
        instance.AjouterKeyPress(new BPMKeyPress(instance.source.time));
    }

    public override void onStart()
    {
        instance.SetPaneauActif(instance.panneaux.penneauDurant);
        instance.source.Play();
    }
}

public class TermineBPMFinderState : BPMFinderState
{
    public static event Action<float> OnBpmFound; 
    public TermineBPMFinderState(MapBpmFinder instance) : base(instance) { }

    public override void onExitPressed()
    {
        instance.SetEtat(new AttenteBPMFinderState(instance));
    }

    public override void onMainPressed()
    {
        OnBpmFound?.Invoke(instance.BPM);
        instance.gameObject.SetActive(false);
    }

    public override void onStart()
    {
        instance.SetPaneauActif(instance.panneaux.penneauApres);
        instance.source.Stop();
        instance.afficherResultats();
    }
}