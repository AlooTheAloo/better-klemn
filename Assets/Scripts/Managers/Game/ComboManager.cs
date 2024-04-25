using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] List<TextMeshPro> textesCombo;
    public ushort[] combos { get; private set; } = new ushort[Constantes.NOMBRE_JOUEURS];
    

    private void OnEnable()
    {
        Alley.onJugement += TraiterCombo;
    }

    private void OnDisable()
    {
        Alley.onJugement -= TraiterCombo;
    }

    private void TraiterCombo(int joueur, Precision precision, Alley a) {
        
        if (precision == Precision.RATE) {
            combos[joueur] = 0;
        }
        else {
            combos[joueur] += 1;
        }
    }

    private void OnGUI() { 
        for (int i = 0; i < Constantes.NOMBRE_JOUEURS; i++) {
            textesCombo[i].text = $"{combos[i]}x";
        }
    }
}
