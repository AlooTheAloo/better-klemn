using System.Collections.Generic;
using UnityEngine;

public class ClickNoteData : NoteData
{
    public ClickNoteData(float tempsDebut, ushort joueur, ushort positionNote, GroupeEffets listeEffets)
        : base()
    {
        this.positionNote = (ushort)(positionNote - 1);
        this.tempsDebut = tempsDebut;
        this.joueur = joueur - 1;
        this.listeEffets = listeEffets;
    }

    public ClickNoteData(float tempsDebut, ushort joueur, ushort positionNote)
        : base()
    {
        this.positionNote = (ushort)(positionNote - 1);
        this.tempsDebut = tempsDebut;
        this.joueur = joueur - 1;
        this.listeEffets = new GroupeEffets();
    }

    

    public override void OnClick() { }
}
