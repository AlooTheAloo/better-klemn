using System;
using UnityEngine;

public class AlleyContainer : MonoBehaviour
{
    [SerializeField] internal AlleyArray[] alleesJoueur = new AlleyArray[Constantes.NOMBRE_JOUEURS];


    private void Update()
    {
        foreach (var joueur in alleesJoueur) // 2
        {
            foreach (var allee in joueur.alleesPourJoueur) // 4
            {
                 allee.UpdateNotes(Constantes.AR_MULTIPLICATEUR * GameManager.Instance.gameNoteData.map.metadonnees.ar, TimeManager.i.tempsSecondes);
            }
        }
    }

}

[Serializable]
internal struct AlleyArray
{
    [SerializeField] internal Alley[] alleesPourJoueur;
}
