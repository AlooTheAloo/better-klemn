using System;
using System.Collections;
using UnityEngine;

public struct GameNoteData
{
    public Map map;

    public GameNoteData(Map map)
    {
        this.map = map;
    }
}

public class GameNotePreloader : Preloader<GameNoteData>, IPreloader
{
    internal Map map;
    public int noteCount;

    public override IPreloader CommencerChargement(Action<GameNoteData> onComplete)
    {
        GameManager.Instance.StartCoroutine(LoadNotesRoutine(onComplete));
        return this;
    }

    public IEnumerator LoadNotesRoutine(Action<GameNoteData> onComplete)
    {
        string nomMap = GameManager.mapSelectionnee.nomMap;
        map = XmlUtil.CreateMapFromName(nomMap);
        noteCount = map.Notes.Count; 
        GameNoteData data = new GameNoteData(map);
        yield return new WaitForSeconds(1f);
        onComplete.Invoke(data);
    }
    
}
