using Nova;
using System.Collections.Generic;

public abstract class NoteData
{
    public int joueur { get; internal set; }
    public ushort positionNote { get; internal set; }
    public float tempsDebut { get; internal set; }
    public GroupeEffets listeEffets { get; internal set; }

    public abstract void OnClick();
}