using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Chroma.Editor
{
    public class EditorMapManager : MonoBehaviour
    {
        [SerializeField] LumieresEditeur lumieresManager;

        public static EditorMap MapData
        {
            get {
                return _instance._map; 
            }
            private set
            {
                throw new InvalidOperationException("Il est impossible de changer le MapData sans passer directement par EditorMapManager");
            }
        }

        private EditorMap _map;


        private static EditorMapManager _instance;

        public static event Action<EditorMapSettings> OnWriteFiles;
        public static event Action<EditorMap> OnMapChanged;
        public static event Action<bool> OnEditeurBarreChanged;

        private void OnEnable()
        {
            SettingsManager.onSettingsChanged += OnSettingsChanged;
        }

        private void OnDisable()
        {
            SettingsManager.onSettingsChanged -= OnSettingsChanged;
            _instance = null;
        }

        private void OnSettingsChanged(EditorMapSettings anciensParametres, EditorMapSettings nouveauxParametres)
        {
            SetEditeurBarre(false);
            string title = nouveauxParametres.metadata.titre;
            string prefix = MainMenuUtil.getMapsDirectory() + Path.DirectorySeparatorChar;

            if (String.IsNullOrEmpty(anciensParametres.metadata.titre))
            {
                var r = nouveauxParametres.ressources;
                string mapPrefix = prefix + title + Path.DirectorySeparatorChar;
                
                Directory.CreateDirectory(prefix + title);
                File.Copy(r.cheminArrierePlan, mapPrefix + Path.GetFileName(r.cheminArrierePlan));
                File.Copy(r.cheminChanson, mapPrefix + Path.GetFileName(r.cheminChanson));
                File.Copy(r.cheminVideo, mapPrefix + Path.GetFileName(r.cheminVideo));

                if (!File.Exists(mapPrefix + Path.GetFileName(r.cheminCouverture)))
                {
                    File.Copy(r.cheminCouverture, mapPrefix + Path.GetFileName(r.cheminCouverture));
                }

                anciensParametres.ressources.cheminChanson = nouveauxParametres.ressources.cheminChanson;
                anciensParametres.ressources.cheminVideo = nouveauxParametres.ressources.cheminVideo;
                anciensParametres.ressources.cheminArrierePlan = nouveauxParametres.ressources.cheminArrierePlan;
                anciensParametres.ressources.cheminCouverture = nouveauxParametres.ressources.cheminCouverture;
                
                _map.MapVisualData.audio = new AudioLoader().LoadDataSync(nouveauxParametres.ressources.cheminChanson).clip;
                _map.MapVisualData.imageArriere = new ImageLoader().getImage(nouveauxParametres.ressources.cheminArrierePlan);
                _map.MapVisualData.imageCover = new ImageLoader().getImage(nouveauxParametres.ressources.cheminCouverture);
            }
            else if (anciensParametres.metadata.titre != title)
            {
                anciensParametres.ressources.cheminChanson = nouveauxParametres.ressources.cheminChanson;
                anciensParametres.ressources.cheminVideo = nouveauxParametres.ressources.cheminVideo;
                anciensParametres.ressources.cheminArrierePlan = nouveauxParametres.ressources.cheminArrierePlan;
                anciensParametres.ressources.cheminCouverture = nouveauxParametres.ressources.cheminCouverture;


                string mapPath = prefix + title + Path.DirectorySeparatorChar;
                nouveauxParametres.ressources.cheminChanson =
                    mapPath + Path.GetFileName(nouveauxParametres.ressources.cheminChanson);

                nouveauxParametres.ressources.cheminVideo =
                    mapPath + Path.GetFileName(nouveauxParametres.ressources.cheminVideo);

                nouveauxParametres.ressources.cheminArrierePlan =
                    mapPath + Path.GetFileName(nouveauxParametres.ressources.cheminArrierePlan);

                nouveauxParametres.ressources.cheminCouverture =
                    mapPath + Path.GetFileName(nouveauxParametres.ressources.cheminCouverture);

                CopyDirectory(anciensParametres, nouveauxParametres);
                _map.MapVisualData.audio = new AudioLoader().LoadDataSync(nouveauxParametres.ressources.cheminChanson).clip;
                _map.MapVisualData.imageArriere = new ImageLoader().getImage(nouveauxParametres.ressources.cheminArrierePlan);
                _map.MapVisualData.imageCover = new ImageLoader().getImage(nouveauxParametres.ressources.cheminCouverture);
            }
            else {
                RemplacerFichiers(anciensParametres, nouveauxParametres, title);
            }

            _map.ListeGroupes = lumieresManager.groupes;
            _map.ListeObjetLumieres = lumieresManager.objetsLumiere;
            _map.FromEditorSettings(nouveauxParametres);
            OnMapChanged?.Invoke(_map);
            OnWriteFiles?.Invoke(nouveauxParametres);

            XmlDocument document = new XmlDocument();
            var xmlNode = _map.ExportMap().Serialize(document);
            document.AppendChild(xmlNode);
            var xml = document.OuterXml;
            File.WriteAllText(prefix + title + Path.DirectorySeparatorChar + XmlConstants.XML_FILE, xml);
        }

        private void RemplacerFichiers(EditorMapSettings anciensParametres, EditorMapSettings nouveauxParametres, string title)
        {
            if (anciensParametres.ressources.cheminChanson != nouveauxParametres.ressources.cheminChanson)
            {
                FileCopy(nouveauxParametres.ressources.cheminChanson,
                    Path.GetFileName(anciensParametres.ressources.cheminChanson), title);
                _map.MapVisualData.audio = new AudioLoader().LoadDataSync(nouveauxParametres.ressources.cheminChanson).clip;
            }

            if (anciensParametres.ressources.cheminVideo != nouveauxParametres.ressources.cheminVideo)
            {
                FileCopy(nouveauxParametres.ressources.cheminVideo,
                    Path.GetFileName(anciensParametres.ressources.cheminVideo), title);
            }

            if (anciensParametres.ressources.cheminArrierePlan != nouveauxParametres.ressources.cheminArrierePlan)
            {
                FileCopy(nouveauxParametres.ressources.cheminArrierePlan,
                    Path.GetFileName(anciensParametres.ressources.cheminArrierePlan), title);
                _map.MapVisualData.imageArriere = new ImageLoader().getImage(nouveauxParametres.ressources.cheminArrierePlan);
            }

            if (anciensParametres.ressources.cheminCouverture != nouveauxParametres.ressources.cheminCouverture)
            {
                FileCopy(nouveauxParametres.ressources.cheminCouverture,
                    Path.GetFileName(anciensParametres.ressources.cheminCouverture), title);
                _map.MapVisualData.imageCover = new ImageLoader().getImage(nouveauxParametres.ressources.cheminCouverture);
            }
        }


        private void CopyDirectory(EditorMapSettings anciensParametres, EditorMapSettings nouveauxParametres)
        {
            string prefix = MainMenuUtil.getMapsDirectory() +
                Path.DirectorySeparatorChar;

            Directory.CreateDirectory(prefix + nouveauxParametres.metadata.titre);
            File.Copy(new Uri(anciensParametres.ressources.cheminChanson).LocalPath,
                nouveauxParametres.ressources.cheminChanson);
            File.Copy(new Uri(anciensParametres.ressources.cheminVideo).LocalPath,
                nouveauxParametres.ressources.cheminVideo);
            File.Copy(new Uri(anciensParametres.ressources.cheminArrierePlan).LocalPath,
                nouveauxParametres.ressources.cheminArrierePlan);

            if (!File.Exists(nouveauxParametres.ressources.cheminCouverture))
            {
                File.Copy(new Uri(anciensParametres.ressources.cheminCouverture).LocalPath,
                nouveauxParametres.ressources.cheminCouverture);
            }

            Directory.Delete(prefix + anciensParametres.metadata.titre, true);
        }

        private void FileCopy(string nouveauFilePath, string ancienFileName, string mapName)
        {
            string prefix = MainMenuUtil.getMapsDirectory() +
                Path.DirectorySeparatorChar + mapName +
                Path.DirectorySeparatorChar;

            if (File.Exists(prefix + ancienFileName)) 
            { 
                File.Delete(prefix + ancienFileName);
            }

            var pathFrom = nouveauFilePath;
            var pathTo = prefix + Path.GetFileName(nouveauFilePath);
            if (!File.Exists(pathTo))
            {
                File.Copy(pathFrom, pathTo);
            }
        }

        private void SetEditeurBarre(bool barre)
        {
            OnEditeurBarreChanged?.Invoke(barre);
        }

        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }

            if (EditorMapSelectManager.SelectedMap.HasValue)
            {
                _map = new EditorMap(EditorMapSelectManager.SelectedMap.Value,
                    ReadMap(EditorMapSelectManager.SelectedMap.Value.nomMap));
                SetEditeurBarre(false);
            }
            else
            {
                _map = new EditorMap();
                _map.Init();
                SetEditeurBarre(true);
            }
        }

        private void Start()
        {
            OnMapChanged?.Invoke(_map);
        }

        private Map ReadMap(string title)
        {
            return XmlUtil.CreateMapFromName(title);
        }

        internal static void SetNotes(List<NoteData> notes)
        {
            _instance._map.Notes = notes;
        }
    }

    public struct EditorMap
    {
        public MapCollectionMap MapVisualData;
        public List<NoteData> Notes;
        public List<string> ListeGroupes;
        public List<LightObjectInitiaialisePacket> ListeObjetLumieres;
        public List<GroupeEffets> ListeEffets;
        public EvenementsMap evenements;
        
        public void Init()
        {
            MapVisualData = new();
            Notes = new List<NoteData>();
            ListeGroupes = new List<string>();
            ListeObjetLumieres = new List<LightObjectInitiaialisePacket>();
            ListeEffets = new List<GroupeEffets>();
            MapVisualData.mapMetadonnees.Init();
        }

        public EditorMap(MapCollectionMap mapVisualData, Map mapSerializable)
        {
            MapVisualData = mapVisualData;
            Notes = mapSerializable.Notes;
            ListeGroupes = mapSerializable.listeGroupes;
            ListeObjetLumieres = mapSerializable.ListeObjetLumieres;
            ListeEffets = mapSerializable.ListeEffets;
            evenements = mapSerializable.evenements;
        }

        public void FromEditorSettings(EditorMapSettings settings)
        {
            var m = MapVisualData.mapMetadonnees;

            // Ressources
            m.fichiers.audio = Path.GetFileName(settings.ressources.cheminChanson);
            m.fichiers.arrierePlan = Path.GetFileName(settings.ressources.cheminArrierePlan);
            m.fichiers.imageCouverture = Path.GetFileName(settings.ressources.cheminCouverture);
            m.fichiers.video = Path.GetFileName(settings.ressources.cheminVideo);

            // Metadonn�es
            m.titre = settings.metadata.titre;
            m.artiste = settings.metadata.artiste;
            m.mappeur = settings.metadata.mappeur;
            m.duree = new DureeMap(settings.metadata.debut ?? 0, settings.metadata.fin ?? 0);
            m.bpm = settings.metadata.BPM ?? 727;
            m.audioPreview = settings.metadata.offsetMenu ?? 0;

            // Difficult�
            m.difficulte = settings.difficulte.diff_generale;
            m.ar = settings.difficulte.diff_velocite;
            m.od = settings.difficulte.diff_precision;
            m.hp = settings.difficulte.diff_vie;

            MapVisualData.mapMetadonnees = m;
        }

        public Map ExportMap()
        {
            var mapExport = new Map
            {
                Notes = Notes ?? new List<NoteData>(),
                listeGroupes = ListeGroupes ?? new List<string>(),
                ListeObjetLumieres = ListeObjetLumieres ?? new List<LightObjectInitiaialisePacket>(),
                ListeEffets = ListeEffets ?? new List<GroupeEffets>(),
                metadonnees = MapVisualData.mapMetadonnees
            };

            mapExport.evenements = evenements;
            return mapExport;
        }
    }
}
