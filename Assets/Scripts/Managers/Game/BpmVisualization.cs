using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BpmVisualization : MonoBehaviour
{

    [SerializeField] List<Rectangle> ligneBpm;
    private float initYLigne;
    private int nombreLignesEnCours = 0;
    private float tempsLigneDescendre;
    private float vitesse = 0f;
    private const float POSITION_MINIMALE_LIGNE = -6.53f;
    [SerializeField] private Rectangle ligneJugement;    
    
    private void Start() {
        initYLigne = ligneBpm[0].transform.localPosition.y;
        nombreLignesEnCours = 0;
        vitesse = Constantes.AR_MULTIPLICATEUR * GameManager.Instance.gameNoteData.map.metadonnees.ar;
        tempsLigneDescendre = (initYLigne - ligneJugement.transform.position.y) / vitesse;
        CalculerProchainSpawn();
    }

    private float prochainSpawn;
    private int compteurSpawn;
    
    void CalculerProchainSpawn()
    {
        prochainSpawn = (Constantes.SECONDES_DANS_MINUTE * 4 / GameManager.mapSelectionnee.mapMetadonnees.bpm) * compteurSpawn- tempsLigneDescendre;
        
        compteurSpawn++;
    }
    
    
    void Update()
    {
        if (TimeManager.i.tempsSecondes > prochainSpawn)
        {
            if (nombreLignesEnCours >= ligneBpm.Count - 1) {
                nombreLignesEnCours = 0;
            }
            ligneBpm[nombreLignesEnCours].transform.localPosition = new Vector2(0, initYLigne);
            ligneBpm[nombreLignesEnCours].gameObject.SetActive(true);

            StartCoroutine(animerLigneBpm(prochainSpawn));
            nombreLignesEnCours++;
            CalculerProchainSpawn();
        }
    }

    
    private IEnumerator animerLigneBpm(float tempsDebut) {

        int indexLigne = nombreLignesEnCours;
        bool aFini = false;
        float lastTime = TimeManager.i.tempsSecondes;
        Vector2 posOrig = new Vector2(ligneBpm[indexLigne].transform.position.x, ligneBpm[indexLigne].transform.position.y - vitesse * (lastTime - tempsDebut));
        ligneBpm[indexLigne].transform.position = posOrig;

        while (!aFini) {
            float delta = TimeManager.i.tempsSecondes - lastTime;
            lastTime = TimeManager.i.tempsSecondes;
            Vector2 newPos = new Vector2(ligneBpm[indexLigne].transform.position.x, ligneBpm[indexLigne].transform.position.y - vitesse * delta);
            ligneBpm[indexLigne].transform.position = newPos;
            
            
            if (newPos.y < POSITION_MINIMALE_LIGNE) {
                ligneBpm[indexLigne].gameObject.SetActive(false);
                aFini = true;
            }
            yield return new WaitForEndOfFrame();
        }
        
        
    }

}
