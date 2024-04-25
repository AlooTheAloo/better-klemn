using System.Collections;
using System.Collections.Generic;
using Chroma.Editor;
using Lean.Pool;
using UnityEngine;

namespace Chroma.Editor
{
   public class DeleteCommand : ICommand
   {
       private EditorAlleyManager _alleyManager;

       private Dictionary<NoteData, EditorNoteActor> previousNotesSelected;
       private Dictionary<GroupeEffets, EditorMapLightEffectActor> previousGroupeEffets;

       public DeleteCommand(EditorAlleyManager alleyManager)
       {
           _alleyManager = alleyManager;
           previousNotesSelected = new Dictionary<NoteData, EditorNoteActor>(_alleyManager.NoteActorsSelected);
           previousGroupeEffets = new Dictionary<GroupeEffets, EditorMapLightEffectActor>(_alleyManager._effetsMapSelectionnes);
       }
       public void Execute()
       {
           
           _alleyManager._effetsMapSelectionnes.Clear();
           _alleyManager.DeselectAllNotes();
           
           foreach (var note in previousNotesSelected)
           {
               _alleyManager._notes.Remove(note.Key);
           }
           
           foreach (var effet in previousGroupeEffets)
           {
               _alleyManager._effetsMapCopies.Remove(effet.Key);
               _alleyManager._mapGroupeEffets.Remove(effet.Key);
           }

          

           _alleyManager.UpdateNotes();
       }
   
       public void Undo()
       {
           foreach (var note in previousNotesSelected)
           {
               _alleyManager._notes.Add(note.Key);
           }
           
           foreach (var effet in previousGroupeEffets)
           {
               _alleyManager._effetsMapCopies.Add(effet.Key, new CopiedEffetActor(effet.Value, 69));
               _alleyManager._mapGroupeEffets.Add(effet.Key);
           }
           _alleyManager.UpdateNotes();
       }
   } 
}

