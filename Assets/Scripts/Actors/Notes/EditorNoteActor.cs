using Nova;
using UnityEngine;

namespace Chroma.Editor.Actors.Notes
{
    public abstract class EditorNoteActor : MonoBehaviour
    {
        internal NoteData notedata;

        internal UIBlock2D acteur;
        public abstract EditorNoteActor Initialiser(NoteData data);
    }
}