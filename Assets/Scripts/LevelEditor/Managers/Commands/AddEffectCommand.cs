using System.Collections;
using System.Collections.Generic;
using Chroma.Editor;
using UnityEngine;

namespace Chroma.Editor
{
    public class AddEffectCommand : ICommand
    {
        private EditorAlleyManager _alleyManager;
        private GroupeEffets _groupeEffets;
        public AddEffectCommand(EditorAlleyManager alleyManager , GroupeEffets groupeEffets )
        {
            _alleyManager = alleyManager;
            _groupeEffets = groupeEffets;
        }
        
        public void Execute()
        {
            _alleyManager._mapGroupeEffets.Add(_groupeEffets);
            _alleyManager.UpdateNotes();
        }
    
        public void Undo()
        {
            _alleyManager._mapGroupeEffets.Remove(_groupeEffets);
            _alleyManager.UpdateNotes();
        }
    }
}

