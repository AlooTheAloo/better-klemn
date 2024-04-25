using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma.Editor.Commands
{
    public class MoveNoteCommand : ICommand
    {
        private EditorAlleyManager _alleyManager;
        private List<NoteData> _noteDatas;
        private List<float> _startTimes;
        private float _deltaTemps;
        private List<AlleeData?> _newAlleys;
        private List<AlleeData?> _originalAlleys;

        public MoveNoteCommand(EditorAlleyManager alleymanager, 
             List<NoteData> noteDatas, List<float> startTimes , float delta, List<AlleeData?> originalAlleys , List<AlleeData?> newAlleys)
        {
            _alleyManager = alleymanager;
            _noteDatas = noteDatas;
            _startTimes = startTimes;
            _deltaTemps = delta;
            Debug.Log(delta);
            _newAlleys = newAlleys;
           _originalAlleys = originalAlleys;

        }

        public void Execute()
        {
            
            for (int i = 0; i < _noteDatas.Count; i++)
            {
                _noteDatas[i].tempsDebut = _startTimes[i] + _deltaTemps;
                _noteDatas[i].joueur = _newAlleys[i].Value.Joueur;
                _noteDatas[i].positionNote = (ushort)_newAlleys[i].Value.Position;
            }
            
            _alleyManager.UpdateNotes();
        }

        public void Undo()
        {
            GameManager.Instance.StartCoroutine(longUndo());

        }

        private IEnumerator longUndo()
        {
            for (int i = 0; i < _noteDatas.Count; i++)
            {
                _noteDatas[i].tempsDebut = _startTimes[i];
            }
            _alleyManager.UpdateNotes();

            yield return null;
            
            for (int i = 0; i < _noteDatas.Count; i++)
            {
                _noteDatas[i].joueur = _originalAlleys[i].Value.Joueur;
                _noteDatas[i].positionNote = (ushort)_originalAlleys[i].Value.Position;
            }
            _alleyManager.UpdateNotes();
        }
    }
}