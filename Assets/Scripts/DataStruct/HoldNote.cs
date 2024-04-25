using System.Collections.Generic;

public class HoldNoteData : NoteData
{
    internal float duree;

    public HoldNoteData(float tempsDebut, ushort joueur, ushort positionNote, float duree, GroupeEffets listeEffets)
        : base()
    {
        this.positionNote = (ushort)(positionNote - 1);
        this.tempsDebut = tempsDebut;
        this.joueur = joueur - 1;
        this.duree = duree;
        this.listeEffets = listeEffets;
    }

    public HoldNoteData(float tempsDebut, ushort joueur, ushort positionNote, float duree)
        : base()
    {
        this.positionNote = (ushort)(positionNote - 1);
        this.tempsDebut = tempsDebut;
        this.joueur = joueur - 1;
        this.duree = duree;
        listeEffets = new GroupeEffets() { };
    }

    public override void OnClick() { }
}
