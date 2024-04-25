using System.Collections.Generic;
using System.Linq;

namespace Chroma.Editor
{
    public class AddNoteCommand : ICommand
    {
        private EditorAlleyManager _alleyManager;
        private float _tempsDebut;
        private ushort _joueur;
        private ushort _positionNote;
        private float _duree;
        private GroupeEffets _listeEffets;

        private NoteData previousNote;
        public AddNoteCommand(EditorAlleyManager alleyManager, float tempsDebut, ushort joueur, ushort positionNote, float duree,
            GroupeEffets listeEffets)
        {
            _alleyManager = alleyManager;
            _tempsDebut = tempsDebut;
            _joueur = joueur;
            _positionNote = positionNote;
            _duree = duree;
            _listeEffets = listeEffets;
        }

        public void Execute()
        {
            if (_duree == 0f)
            {
                _alleyManager._notes.Add( new ClickNoteData(_tempsDebut , _joueur , _positionNote , _listeEffets));
            }
            else
            {
                _alleyManager._notes.Add( new HoldNoteData(_tempsDebut , _joueur , _positionNote , _duree , _listeEffets));
            }

            previousNote = _alleyManager._notes.Last();
            _alleyManager.UpdateNotes();
        }

        public void Undo()
        {
            _alleyManager._notes.Remove(previousNote);
            _alleyManager.UpdateNotes();
        }
    }
}