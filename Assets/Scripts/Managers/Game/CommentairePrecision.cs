using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CommentairePrecision : MonoBehaviour
{
    [SerializeField]
    private float font_size = 16f;

    [Header("Textes précision")]
    [SerializeField]
    string parfaitText;

    [SerializeField]
    Color parfaitColor;

    [SerializeField]
    string bienText;

    [SerializeField]
    Color bienColor;

    [SerializeField]
    string okText;

    [SerializeField]
    Color okColor;

    [SerializeField]
    string manqueText;

    [SerializeField]
    Color manqueColor;

    private Dictionary<string, Color> textesPrecision;

    private TextMeshPro texteCommentaire;
    private Coroutine coroutineEnCours;

    [SerializeField]
    Player joueur;

    [SerializeField]
    private float VitesseAnimation;

    void Start()
    {
        texteCommentaire = GetComponent<TextMeshPro>();
        textesPrecision = new()
        {
            { parfaitText, parfaitColor },
            { bienText, bienColor },
            { okText, okColor },
            { manqueText, manqueColor },
        };
    }

    private void OnEnable()
    {
        Alley.onJugement += DeterminerTexte;
    }

    private void OnDisable()
    {
        Alley.onJugement -= DeterminerTexte;
    }

    private void DeterminerTexte(int joueur, Precision precision, Alley a)
    {
        if (joueur != this.joueur.playerID)
            return;

        AfficherTexte(
            textesPrecision.Keys.ElementAt((int)precision),
            textesPrecision.Values.ElementAt((int)precision)
        );
    }

    private void AfficherTexte(string texteAffiche, Color couleur)
    {
        texteCommentaire.text = texteAffiche;
        texteCommentaire.fontSize = font_size;
        texteCommentaire.color = couleur;

        if (coroutineEnCours != null)
        {
            StopCoroutine(coroutineEnCours);
        }
        coroutineEnCours = StartCoroutine(AnimationTexte());
    }

    private IEnumerator AnimationTexte()
    {
        //Animation du texte pour les commentaires
        float tempsDepuisAnimation = 0;
        while (texteCommentaire.fontSize > 8)
        {
            tempsDepuisAnimation += Time.deltaTime;
            texteCommentaire.fontSize -=
                VitesseAnimation * Time.deltaTime * (1 / Mathf.Sqrt(tempsDepuisAnimation));
            yield return new WaitForEndOfFrame();
        }

        //Le texte disparait en une seconde après l'animation
        float opacite = 1;
        while (opacite > 0)
        {
            opacite -= Time.deltaTime;
            texteCommentaire.color = new Color(
                texteCommentaire.color.r,
                texteCommentaire.color.g,
                texteCommentaire.color.b,
                opacite
            );
            yield return new WaitForEndOfFrame();
        }
    }
}
