using Nova;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public struct MapCollectionMap
{
    public string nomMap;
    public Metadonnees mapMetadonnees;
    public Texture2D imageArriere;
    public Texture2D imageCover;
    public AudioClip audio;
    public UIBlock2D mapRepresentation;
    public bool selectionne;
    public Color couleur;
}


public class MapCollection : MonoBehaviour {

    private const int TUTORIAL_MAP_ID = -1;

    public static MapCollection i;
    private List<MapCollectionMap> maps = new List<MapCollectionMap>();
    public List<MapCollectionMap> mapsAffichees = new List<MapCollectionMap>();
    private void Awake() {
        i = this;
        DontDestroyOnLoad(this);
    }
    private void Start() {
        ChargerMaps();
    }

    public void ChargerMaps(Action onComplete = null) {
        maps.Clear();
        DirectoryInfo root = new DirectoryInfo(MainMenuUtil.getMapsDirectory());
        DirectoryInfo[] mapRoots = root.GetDirectories();
        int i = 0;
        foreach (var mapRoot in mapRoots) {
            Metadonnees? metadonneesNullable = XmlUtil.getMetadonneesFromMapName(mapRoot.Name);

            if (metadonneesNullable == null) i++;
            else {
                Metadonnees metadonnees = (Metadonnees)metadonneesNullable;
                new AudioLoader().LoadData(metadonnees.fichiers.audio)
                .onSuccess += (request) => {
                    i++;
                    MapCollectionMap nouvelleMap = new MapCollectionMap();
                    nouvelleMap.audio = request.clip;
                    nouvelleMap.nomMap = mapRoot.Name;
                    ImageLoaderImages images = new ImageLoader().LoadData(metadonnees);
                    nouvelleMap.imageCover = images.imageCover;
                    nouvelleMap.imageArriere = images.imageBG;
                    nouvelleMap.mapMetadonnees = metadonnees;
                    nouvelleMap.couleur = images.mainColor;
                    maps.Add(nouvelleMap);
                };
            }
            
        }
        

        StartCoroutine(SimpleRoutines.WaitUntil(() => mapRoots.Length == i, () =>
        {
            mapsAffichees = maps.
                OrderBy(x => x.mapMetadonnees.difficulte).
                Where(x => x.mapMetadonnees.chansonId != TUTORIAL_MAP_ID).ToList();
            onComplete?.Invoke();
        }));
    }

    public MapCollectionMap? Tutoriel() {

        MapCollectionMap? map = maps.FirstOrDefault(x => x.mapMetadonnees.chansonId == TUTORIAL_MAP_ID);

        print("La map avec id -1 est " + map.Value.nomMap + " proof : " + map.Value.mapMetadonnees.chansonId);

        if (!map.HasValue) {
            Debug.LogError("La map tutoriel n'a pas été trouvée");
            SceneManager.LoadScene((int)Scenes.SONG_SELECT);
            return null;
        }
        else return map.Value;
        
    }

}