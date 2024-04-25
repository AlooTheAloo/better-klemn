using System.Drawing;
using Color = UnityEngine.Color;
using Rectangle = Shapes.Rectangle;

public class ClickNoteActor : GameNoteActor 
{
    public override void OnMiss(Alley alley , float vitesseMovement)
    {
        FadeOutNotes fadeOutNotes = gameObject.GetComponent<FadeOutNotes>();
        fadeOutNotes.thisAlley = alley;
        fadeOutNotes.vitesseMovement = vitesseMovement;
        
        fadeOutNotes.BeginFade(false);

    }

    internal void Finish()
    {

    }

}
