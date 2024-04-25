using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;
using System.Globalization;
using System.Linq;

public interface IXMLSerializable<T> 
{
    public XmlNode Serialize(XmlDocument document);
    public T Deserialize(XmlNode node);
}

[Serializable]
public struct Map : IXMLSerializable<Map>
{
    public EvenementsMap evenements;
    public Metadonnees metadonnees;
    public List<NoteData> Notes;
    public List<string> listeGroupes;
    public List<LightObjectInitiaialisePacket> ListeObjetLumieres;
    public List<GroupeEffets> ListeEffets;

    public Map Deserialize(XmlNode node)
    {
        throw new NotImplementedException(); // trol
    }

    public XmlNode Serialize(XmlDocument mapDocument)
    {
        XmlNode donneesNiveauRoot = mapDocument.CreateElement(XmlConstants.ROOT_NODE);
        XmlNode metadonneesNode = mapDocument.CreateElement(XmlConstants.METADONNEES_NODE);

        List<XmlNode> listMapNodes = new List<XmlNode>();

        listMapNodes.Add(evenements.Serialize(mapDocument));
        
        // METADONNEES SERIALIZATION

        List<XmlNode> listMetadonneesNode = new List<XmlNode>();

        // titre node
        XmlNode titreNode = mapDocument.CreateElement("Titre");
        XmlAttribute titreAttr = mapDocument.CreateAttribute("value");
        titreAttr.Value = metadonnees.titre;
        titreNode.Attributes.Append(titreAttr);
        listMetadonneesNode.Add(titreNode);

        // artiste node
        XmlNode artisteNode = mapDocument.CreateElement("Artiste");
        XmlAttribute artisteAttr = mapDocument.CreateAttribute("value");
        artisteAttr.Value = metadonnees.artiste;
        artisteNode.Attributes.Append(artisteAttr);
        listMetadonneesNode.Add(artisteNode);

        // mappeur node
        XmlNode mappeurNode = mapDocument.CreateElement("Mappeur");
        XmlAttribute mappeurAttr = mapDocument.CreateAttribute("value");
        mappeurAttr.Value = metadonnees.mappeur;
        mappeurNode.Attributes.Append(mappeurAttr);
        listMetadonneesNode.Add(mappeurNode);

        // tempo node
        XmlNode tempoNode = mapDocument.CreateElement("Tempo");
        XmlAttribute tempoAttr = mapDocument.CreateAttribute("bpm");
        tempoAttr.Value = metadonnees.bpm.ToString();
        tempoNode.Attributes.Append(tempoAttr);
        listMetadonneesNode.Add(tempoNode);

        // duree node
        XmlNode dureeNode = mapDocument.CreateElement("Duree");

        XmlAttribute dureeDebutAttr = mapDocument.CreateAttribute("debut");
        dureeDebutAttr.Value = metadonnees.duree.debut.ToString();

        XmlAttribute dureeFinAttr = mapDocument.CreateAttribute("fin");
        dureeFinAttr.Value = metadonnees.duree.fin.ToString();

        dureeNode.Attributes.Append(dureeDebutAttr);
        dureeNode.Attributes.Append(dureeFinAttr);
        listMetadonneesNode.Add(dureeNode);

        // audiopreview node
        XmlNode audioPrvNode = mapDocument.CreateElement("AudioPreview");
        XmlAttribute audioPrvAttr = mapDocument.CreateAttribute("offset");
        audioPrvAttr.Value = metadonnees.audioPreview.ToString();
        audioPrvNode.Attributes.Append(audioPrvAttr);
        listMetadonneesNode.Add(audioPrvNode);

        // fichiers node
        XmlNode fichiersNode = mapDocument.CreateElement("Fichiers");

        XmlAttribute fichiersChansonAttr = mapDocument.CreateAttribute("chanson");
        fichiersChansonAttr.Value = Path.GetFileName(metadonnees.fichiers.audio);

        XmlAttribute fichiersCouvertureAttr = mapDocument.CreateAttribute("couverture");
        fichiersCouvertureAttr.Value = Path.GetFileName(metadonnees.fichiers.imageCouverture);

        XmlAttribute fichiersBgAttr = mapDocument.CreateAttribute("bg");
        fichiersBgAttr.Value = Path.GetFileName(metadonnees.fichiers.arrierePlan);

        // video optional
        XmlAttribute fichiersVideoAttr = mapDocument.CreateAttribute("video");
        fichiersVideoAttr.Value = Path.GetFileName(metadonnees.fichiers.video ?? "");

        fichiersNode.Attributes.Append(fichiersChansonAttr);
        fichiersNode.Attributes.Append(fichiersCouvertureAttr);
        fichiersNode.Attributes.Append(fichiersBgAttr);
        fichiersNode.Attributes.Append(fichiersVideoAttr);
        listMetadonneesNode.Add(fichiersNode);

        // ar node
        XmlNode arNode = mapDocument.CreateElement("AR");
        XmlAttribute arAttr = mapDocument.CreateAttribute("value");
        arAttr.Value = metadonnees.ar.ToString();
        arNode.Attributes.Append(arAttr);
        listMetadonneesNode.Add(arNode);

        // od node
        XmlNode odNode = mapDocument.CreateElement("OD");
        XmlAttribute odAttr = mapDocument.CreateAttribute("value");
        odAttr.Value = metadonnees.od.ToString();
        odNode.Attributes.Append(odAttr);
        listMetadonneesNode.Add(odNode);

        // chansonId node
        XmlNode chansonIdNode = mapDocument.CreateElement("ChansonID");
        XmlAttribute chansonIdAttr = mapDocument.CreateAttribute("value");
        chansonIdAttr.Value = metadonnees.chansonId.ToString();
        chansonIdNode.Attributes.Append(chansonIdAttr);
        listMetadonneesNode.Add(chansonIdNode);

        // hp node
        XmlNode hpNode = mapDocument.CreateElement("HP");
        XmlAttribute hpNodeAttr = mapDocument.CreateAttribute("value");
        hpNodeAttr.Value = metadonnees.hp.ToString();
        hpNode.Attributes.Append(hpNodeAttr);
        listMetadonneesNode.Add(hpNode);

        // diff node
        XmlNode diffNode = mapDocument.CreateElement("Difficulte");
        XmlAttribute diffAttr = mapDocument.CreateAttribute("value");
        diffAttr.Value = metadonnees.difficulte.ToString();
        diffNode.Attributes.Append(diffAttr);
        listMetadonneesNode.Add(diffNode);

        // adding all nodes to metadonnees node
        foreach (XmlNode node in listMetadonneesNode)
        {
            metadonneesNode.AppendChild(node);
        }
        listMapNodes.Add(metadonneesNode);

        // LISTE GROUPES SERIALIZATION
        XmlNode listeGroupesNodes = mapDocument.CreateElement(XmlConstants.LISTE_GROUPE_NODE);
        foreach (string groupe in listeGroupes)
        {
            XmlNode node = mapDocument.CreateElement("Groupe");
            XmlAttribute attr = mapDocument.CreateAttribute("nom");
            attr.Value = groupe;
            node.Attributes.Append(attr);
            listeGroupesNodes.AppendChild(node);
        }
        listMapNodes.Add(listeGroupesNodes);

        // LISTE OBJ LUMIERES SERIALIZATION
        XmlNode listeObjetsLumieresNodes = mapDocument.CreateElement(
            XmlConstants.LISTE_OBJ_LUMIERE_NODE
        );
        foreach (LightObjectInitiaialisePacket objetLumiere in ListeObjetLumieres)
        {
            XmlNode objetLumiereNode = mapDocument.CreateElement("ObjetLumiere"); // TODO : CONSTANTE

            // attributs
            XmlAttribute attrType = mapDocument.CreateAttribute("type");
            switch (objetLumiere.lumiereData.type)
            {
                case ShapeActorType.ELLIPSE:
                    attrType.Value = XmlConstants.ELLIPSE_OBJET_NOM;
                    break;
                case ShapeActorType.RECTANGLE:
                    attrType.Value = XmlConstants.RECTANGLE_OBJET_NOM;
                    break;
            }
            objetLumiereNode.Attributes.Append(attrType);

            // groupes
            objetLumiereNode.Attributes.Append(
                SerializeGroupes(objetLumiere.lumiereData.groupes, "groupes", mapDocument)
            );

            XmlAttribute attrProjecteur = mapDocument.CreateAttribute("projecteur");
            attrProjecteur.Value = objetLumiere.lumiereData.projectorID.ToString();
            objetLumiereNode.Attributes.Append(attrProjecteur);

            XmlAttribute attrNom = mapDocument.CreateAttribute("nom");
            attrNom.Value = objetLumiere.lumiereData.nomLumiere;
            objetLumiereNode.Attributes.Append(attrNom);

            // État objet
            XmlNode etatObjetNode = SerializeEtatObjet(
                objetLumiere.initialState.listeEtats,
                mapDocument
            );
            objetLumiereNode.AppendChild(etatObjetNode);

            listeObjetsLumieresNodes.AppendChild(objetLumiereNode);
        }
        listMapNodes.Add(listeObjetsLumieresNodes);

        // Liste effets
        XmlNode listeEffetLumineux = SerializeListeEffets(ListeEffets, mapDocument);
        listMapNodes.Add(listeEffetLumineux);
        // NOTES TIMES
        XmlNode listeNotesNode = mapDocument.CreateElement(XmlConstants.LISTE_NOTES_NODE);
        foreach (NoteData noteData in Notes)
        {
            XmlNode node = mapDocument.CreateElement(XmlConstants.NOTE_NODE);

            XmlAttribute joueurAttr = mapDocument.CreateAttribute("joueur");
            joueurAttr.Value = (noteData.joueur + 1).ToString();
            node.Attributes.Append(joueurAttr);

            XmlAttribute positionAttr = mapDocument.CreateAttribute("position");
            positionAttr.Value = (noteData.positionNote + 1).ToString();
            node.Attributes.Append(positionAttr);

            XmlAttribute tempsDebutAttr = mapDocument.CreateAttribute("tempsDebut");
            tempsDebutAttr.Value = noteData.tempsDebut.ToString(CultureInfo.InvariantCulture);
            node.Attributes.Append(tempsDebutAttr);

            XmlAttribute debutAttr = mapDocument.CreateAttribute("duree");
            if (noteData is ClickNoteData)
                debutAttr.Value = "0";
            else
            {
                debutAttr.Value = (noteData as HoldNoteData).duree.ToString(
                    CultureInfo.InvariantCulture
                );
            }
            node.Attributes.Append(debutAttr);

            if (noteData.listeEffets.EffetsLumineux.Count != 0)
            {
                XmlNode listeEffetsNotesNode = SerializeListeEffets(
                    new List<GroupeEffets>() { noteData.listeEffets },
                    mapDocument
                );
                node.AppendChild(listeEffetsNotesNode);
            }

            listeNotesNode.AppendChild(node);
        }
        listMapNodes.Add(listeNotesNode);

        // adding all nodes to root node
        foreach (XmlNode node in listMapNodes)
        {
            donneesNiveauRoot.AppendChild(node);
        }
        
        return donneesNiveauRoot;
    }

    private XmlNode SerializeListeEffets(List<GroupeEffets> ListeGroupes, XmlDocument doc)
    {
        XmlNode listeEffets = doc.CreateElement(XmlConstants.LISTE_EFFETS_NODE);

        foreach (GroupeEffets groupe in ListeGroupes)
        {
            XmlNode nodeGroupeEffet = doc.CreateElement(XmlConstants.GROUPES_NODE);
            XmlAttribute tempsattr = doc.CreateAttribute("tempsDebut");
            tempsattr.Value = groupe.TempsDebut.ToString(CultureInfo.InvariantCulture);

            nodeGroupeEffet.Attributes.Append(tempsattr);

            foreach (EffetLumineux effet in groupe.EffetsLumineux)
            {
                XmlNode nodeEffet = doc.CreateElement(XmlConstants.EFFET_NODE);

                XmlAttribute orderAttr = doc.CreateAttribute("order");
                orderAttr.Value = effet.Order.ToString();
                nodeEffet.Attributes.Append(orderAttr);

                XmlAttribute decalageAttr = doc.CreateAttribute("decalage");
                decalageAttr.Value = effet.Decalage.ToString(CultureInfo.InvariantCulture);
                nodeEffet.Attributes.Append(decalageAttr);

                XmlAttribute groupeCibleAttr = SerializeGroupes(effet.GroupesCible, "groupeCible", doc);
                nodeEffet.Attributes.Append(groupeCibleAttr);

                XmlAttribute dureeAttr = doc.CreateAttribute("duree");
                dureeAttr.Value = effet.Duree.ToString(CultureInfo.InvariantCulture);
                nodeEffet.Attributes.Append(dureeAttr);

                XmlAttribute transAttr = doc.CreateAttribute("transition");
                transAttr.Value = effet.CourbeAnimation.ToString();
                nodeEffet.Attributes.Append(transAttr);

                XmlNode etatObjetNode = SerializeEtatObjet(effet.State.listeEtats, doc);
                nodeEffet.AppendChild(etatObjetNode);

                nodeGroupeEffet.AppendChild(nodeEffet);
            }
            listeEffets.AppendChild(nodeGroupeEffet);
        }

        return listeEffets;
    }

    private XmlAttribute SerializeGroupes(string[] groupes, string attrName, XmlDocument doc)
    {
        XmlAttribute attrGroupes = doc.CreateAttribute(attrName);
        string groupesTemp = "";
        int interval = 0;
        foreach (string groupe in groupes)
        {
            if (interval != groupes.Length - 1)
            {
                groupesTemp += groupe + XmlConstants.GROUPS_SEPARATOR;
            }
            else
            {
                groupesTemp += groupe;
            }
            interval++;
        }
        attrGroupes.Value = groupesTemp;

        return attrGroupes;
    }

    private XmlNode SerializeEtatObjet(LightState[] listeEtats, XmlDocument doc)
    {
        XmlNode node = doc.CreateElement(XmlConstants.ETAT_OBJET_NODE);
        foreach (LightState state in listeEtats)
        {
            if (state.actif)
            {
                XmlNode nodeTemp = null;
                switch (state)
                {
                    case PositionLightState:
                        nodeTemp = doc.CreateElement(XmlConstants.ETAT_POS_NODE);
                        XmlAttribute attrXPos = doc.CreateAttribute("x");
                        attrXPos.Value = (state as PositionLightState).targetPosition.x.ToString(CultureInfo.InvariantCulture);
                        XmlAttribute attrYPos = doc.CreateAttribute("y");
                        attrYPos.Value = (state as PositionLightState).targetPosition.y.ToString(CultureInfo.InvariantCulture);
                        nodeTemp.Attributes.Append(attrXPos);
                        nodeTemp.Attributes.Append(attrYPos);
                        break;

                    case ScaleLightState:
                        nodeTemp = doc.CreateElement(XmlConstants.ETAT_TAILLE_NODE);
                        XmlAttribute attrXScale = doc.CreateAttribute("x");
                        attrXScale.Value = (state as ScaleLightState).taille.x.ToString(CultureInfo.InvariantCulture);
                        XmlAttribute attrYScale = doc.CreateAttribute("y");
                        attrYScale.Value = (state as ScaleLightState).taille.y.ToString(CultureInfo.InvariantCulture);
                        nodeTemp.Attributes.Append(attrXScale);
                        nodeTemp.Attributes.Append(attrYScale);
                        break;

                    case ColorLightState:
                        nodeTemp = doc.CreateElement(XmlConstants.ETAT_COULEUR_NODE);
                        XmlAttribute attrColor = doc.CreateAttribute("couleur");
                        attrColor.Value =
                            "#"
                            + ColorUtility.ToHtmlStringRGB((state as ColorLightState).lightColor);
                        nodeTemp.Attributes.Append(attrColor);
                        break;

                    case OpacityLightState:
                        nodeTemp = doc.CreateElement(XmlConstants.ETAT_OPACITE_NODE);
                        XmlAttribute attrOpacite = doc.CreateAttribute("opacite");
                        attrOpacite.Value = (state as OpacityLightState).opacity.ToString(CultureInfo.InvariantCulture);
                        nodeTemp.Attributes.Append(attrOpacite);
                        break;
                }
                if (state.mesure == StateMeasurement.RELATIVE)
                {
                    XmlAttribute mesureAttr = doc.CreateAttribute("mesure");
                    mesureAttr.Value = "relatif";
                    nodeTemp.Attributes.Append(mesureAttr);
                } else
                {
                    XmlAttribute mesureAttr = doc.CreateAttribute("mesure");
                    mesureAttr.Value = "absolu";
                    nodeTemp.Attributes.Append(mesureAttr);
                }
                node.AppendChild(nodeTemp);
            }
        }
        return node;
    }
}

[Serializable]
public struct Metadonnees
{
    public string titre;
    public string artiste;
    public string mappeur;
    public float audioPreview;
    public float bpm;
    public DureeMap duree;
    public FichiersMap fichiers;
    public int ar; // approach rate
    public int od; // occupation double
    public int chansonId;
    public int hp; // health points
    public int difficulte;

    public void Init()
    {
        // TODO : Ajouter param (et ping vers serveur) pour chansonID
        ar = 1;
        od = 1;
        hp = 1;
        difficulte = 1;
    }
}

[Serializable]
public struct DureeMap
{
    public float debut;
    public float fin;
    public readonly float dureeTemps
    {
        get { return fin - debut; }
    }

    public DureeMap(float debut, float fin)
    {
        this.debut = debut;
        this.fin = fin;
    }
}

public struct EvenementsMap : IXMLSerializable<EvenementsMap>
{
    public List<Evenement> evenements;

    public EvenementsMap(List<Evenement> evenements)
    {
        this.evenements = evenements;
    }
    
    public XmlNode Serialize(XmlDocument document)
    { 
        XmlNode evenementsNode = document.CreateElement("Evenements");


        if (evenements != null)
        {
            foreach (var evenement in evenements)
            {
                evenementsNode.AppendChild(evenement.Serialize(document));
            } 
        }

        return evenementsNode;
    }

    public EvenementsMap Deserialize(XmlNode node)
    {
        evenements = new List<Evenement>();
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.Name == "Kiai")
            {
                Debug.Log("Creating kiai");
                evenements.Add(new KiaiEvenement().Deserialize(child));
            }
            else if (child.Name == "Blackout")
            {
                evenements.Add(new BlackoutEvenement().Deserialize(child));                
            }
        }

        evenements = evenements.OrderBy(x => x.start).ToList();

        return this;
    }
}

public abstract class Evenement : IXMLSerializable<Evenement>
{
    public float start;
    public float end;

    public abstract void Execute();
    public abstract void Cancel();
    
    public void SerializeElements(ref XmlNode node, XmlDocument document)
    {
        XmlAttribute startAttr = document.CreateAttribute("Start");
        startAttr.Value = start.ToString(CultureInfo.InvariantCulture);
        
        XmlAttribute endAttr  = document.CreateAttribute("End");
        endAttr.Value = end.ToString(CultureInfo.InvariantCulture);

        if (node.Attributes == null) return;
        node.Attributes.Append(startAttr);
        node.Attributes.Append(endAttr);
    }
    
    public void DeserializeElements(XmlNode node)
    {
        Debug.Log("Funny is " + node.Name);
        
        start = float.Parse(node.Attributes["Start"].Value, CultureInfo.InvariantCulture);
        end = float.Parse(node.Attributes["End"].Value, CultureInfo.InvariantCulture);
    }

    public abstract XmlNode Serialize(XmlDocument document);
    public abstract Evenement Deserialize(XmlNode node);
}

public class KiaiEvenement : Evenement
{
    private ushort color;
    private ushort pattern;

    public override void Execute()
    {
        Debug.Log("Executing DMX :)");
        LightsClient.ActiverDMX(color, pattern);
    }

    public override void Cancel()
    {
        if(MapEventsManager.i.evenementsEnCours.Where(x => x.GetType() == typeof(KiaiEvenement)).Count() <= 1){
            LightsClient.DesactiverDMX();
        }
    }

    public override XmlNode Serialize(XmlDocument document)
    {
        XmlNode node = document.CreateElement("Kiai");
        base.SerializeElements(ref node, document);
        var colorAttr = document.CreateAttribute("Color");
        colorAttr.Value = color.ToString();
        var patternAttr = document.CreateAttribute("Pattern");
        patternAttr.Value = pattern.ToString();
        node.Attributes.Append(colorAttr);
        node.Attributes.Append(patternAttr);
        return node;
    }

    public override Evenement Deserialize(XmlNode node)
    {
        base.DeserializeElements(node);
        color = ushort.Parse(node.Attributes["Color"].Value);
        pattern = ushort.Parse(node.Attributes["Pattern"].Value);
        return this;
    }
}

public class BlackoutEvenement : Evenement 
{
    public static event Action onEventStart;
    public static event Action onEventEnd;

    public override void Execute()
    {
        Debug.Log("Oneventstart");
        onEventStart?.Invoke();
    }

    public override void Cancel()
    {
        Debug.Log("Oneventend");
        onEventEnd?.Invoke();
    }

    public override XmlNode Serialize(XmlDocument document)
    {
        XmlNode node = document.CreateElement("Blackout");
        base.SerializeElements(ref node, document);
        return node;
    }

    public override Evenement Deserialize(XmlNode node)
    {
        base.DeserializeElements(node);
        return this;
    }
}


[Serializable]
public struct FichiersMap
{
    public string audio;
    public string imageCouverture;
    public string arrierePlan;
    public string video;
}

public class XmlConstants
{
    public static readonly string GROUPS_SEPARATOR = "~";
    internal static readonly string ROOT_NODE = "DonneesNiveau";
    internal static readonly string METADONNEES_NODE = "Metadonnees";
    internal static readonly string LISTE_GROUPE_NODE = "ListeGroupes";
    internal static readonly string LISTE_OBJ_LUMIERE_NODE = "ListeObjetLumiere";
    internal static readonly string LISTE_EFFETS_NODE = "ListeEffets";
    internal static readonly string GROUPES_NODE = "GroupeEffets";
    internal static readonly string EFFET_NODE = "Effet";
    internal static readonly string LISTE_NOTES_NODE = "Notes";
    internal static readonly string NOTE_NODE = "Note";
    internal static readonly string CHANSON_ID_NODE = "ChansonID";

    internal static readonly string ETAT_OBJET_NODE = "EtatObjet";
    internal const string ETAT_COULEUR_NODE = "EtatCouleur";
    internal const string ETAT_OPACITE_NODE = "EtatOpacite";
    internal const string ETAT_POS_NODE = "EtatPosition";
    internal const string ETAT_TAILLE_NODE = "EtatTaille";
    internal const string ELLIPSE_OBJET_NOM = "Circle";
    internal const string RECTANGLE_OBJET_NOM = "Rectangle";

    public static readonly string XML_FILE = "data.xml";
}

public class XmlUtil
{
    public static Metadonnees GetMetadonneesFromNode(XmlNode node)
    {
        Metadonnees metadonnees = new();
        try
        {
            XmlNode donneesNiveau = node.SelectSingleNode(XmlConstants.ROOT_NODE);

            XmlNode titre = donneesNiveau.SelectSingleNode("Metadonnees/Titre");
            metadonnees.titre = titre.Attributes["value"].Value;

            XmlNode artiste = donneesNiveau.SelectSingleNode("Metadonnees/Artiste");
            metadonnees.artiste = artiste.Attributes["value"].Value;

            XmlNode mappeur = donneesNiveau.SelectSingleNode("Metadonnees/Mappeur");
            metadonnees.mappeur = mappeur.Attributes["value"].Value;

            XmlNode bpm = donneesNiveau.SelectSingleNode("Metadonnees/Tempo");
            metadonnees.bpm = int.Parse(bpm.Attributes["bpm"].Value);

            XmlNode duree = donneesNiveau.SelectSingleNode("Metadonnees/Duree");
            metadonnees.duree.debut = float.Parse(
                duree.Attributes["debut"].Value,
                CultureInfo.InvariantCulture
            );
            metadonnees.duree.fin = float.Parse(
                duree.Attributes["fin"].Value,
                CultureInfo.InvariantCulture
            );

            XmlNode audioPreview = donneesNiveau.SelectSingleNode("Metadonnees/AudioPreview");
            metadonnees.audioPreview = float.Parse(audioPreview.Attributes["offset"].Value);

            XmlNode fichiers = donneesNiveau.SelectSingleNode("Metadonnees/Fichiers");
            metadonnees.fichiers.audio = fichiers.Attributes["chanson"].Value;
            metadonnees.fichiers.imageCouverture = fichiers.Attributes["couverture"].Value;
            metadonnees.fichiers.arrierePlan = fichiers.Attributes["bg"].Value;
            metadonnees.fichiers.video = fichiers.Attributes["video"].Value;

            XmlNode ar = donneesNiveau.SelectSingleNode("Metadonnees/AR");
            metadonnees.ar = int.Parse(ar.Attributes["value"].Value, CultureInfo.InvariantCulture);

            XmlNode od = donneesNiveau.SelectSingleNode("Metadonnees/OD");
            metadonnees.od = int.Parse(od.Attributes["value"].Value, CultureInfo.InvariantCulture);

            XmlNode chansonid = donneesNiveau.SelectSingleNode("Metadonnees/ChansonID");
            metadonnees.chansonId = int.Parse(chansonid.Attributes["value"].Value);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to read metadonnees from path : " + e);
            Debug.LogError(e.StackTrace);
            return new Metadonnees();
        }

        return metadonnees;
    }

    public static Metadonnees? getMetadonneesFromMapName(string mapName)
    {
        string pathToMapXML =
            MainMenuUtil.getMapsDirectory()
            + mapName
            + Path.DirectorySeparatorChar
            + XmlConstants.XML_FILE;

        string pathToMapDirectory =
            MainMenuUtil.getMapsDirectory() + mapName + Path.DirectorySeparatorChar;
        Metadonnees metadonnees = new Metadonnees();
        XmlDocument doc;
        doc = new XmlDocument();
        try
        {
            doc.Load(pathToMapXML);
            XmlNode donneesNiveau = doc.SelectSingleNode(XmlConstants.ROOT_NODE);

           

            
            XmlNode titre = donneesNiveau.SelectSingleNode("Metadonnees/Titre");
            metadonnees.titre = titre.Attributes["value"].Value;

            XmlNode artiste = donneesNiveau.SelectSingleNode("Metadonnees/Artiste");
            metadonnees.artiste = artiste.Attributes["value"].Value;

            XmlNode mappeur = donneesNiveau.SelectSingleNode("Metadonnees/Mappeur");
            metadonnees.mappeur = mappeur.Attributes["value"].Value;

            XmlNode bpm = donneesNiveau.SelectSingleNode("Metadonnees/Tempo");
            metadonnees.bpm = float.Parse(
                bpm.Attributes["bpm"].Value,
                CultureInfo.InvariantCulture
            );

            XmlNode duree = donneesNiveau.SelectSingleNode("Metadonnees/Duree");
            metadonnees.duree.debut = float.Parse(
                duree.Attributes["debut"].Value,
                CultureInfo.InvariantCulture
            );
            metadonnees.duree.fin = float.Parse(
                duree.Attributes["fin"].Value,
                CultureInfo.InvariantCulture
            );

            XmlNode audioPreview = donneesNiveau.SelectSingleNode("Metadonnees/AudioPreview");
            metadonnees.audioPreview = float.Parse(audioPreview.Attributes["offset"].Value);

            XmlNode fichiers = donneesNiveau.SelectSingleNode("Metadonnees/Fichiers");
            string absoluteAudioPath = pathToMapDirectory + fichiers.Attributes["chanson"].Value;
            Uri audioUri = new(absoluteAudioPath);
            metadonnees.fichiers.audio = audioUri.ToString();
            metadonnees.fichiers.imageCouverture =
                pathToMapDirectory + fichiers.Attributes["couverture"].Value;
            metadonnees.fichiers.arrierePlan = pathToMapDirectory + fichiers.Attributes["bg"].Value;
            if (fichiers.Attributes["video"].Value != "")
            {
                string absoluteVideoPath = pathToMapDirectory + fichiers.Attributes["video"].Value;
                Uri videoUri = new(absoluteVideoPath);
                metadonnees.fichiers.video = videoUri.ToString();
            } else
            {
                metadonnees.fichiers.video = "";
            }
            

            XmlNode ar = donneesNiveau.SelectSingleNode("Metadonnees/AR");
            metadonnees.ar = int.Parse(ar.Attributes["value"].Value, CultureInfo.InvariantCulture);

            XmlNode od = donneesNiveau.SelectSingleNode("Metadonnees/OD");
            metadonnees.od = int.Parse(od.Attributes["value"].Value, CultureInfo.InvariantCulture);

            XmlNode chansonid = donneesNiveau.SelectSingleNode("Metadonnees/ChansonID");
            if (chansonid == null)
                throw new ArgumentException("Fichier ne contient pas d'ID de chanson");
            metadonnees.chansonId = int.Parse(chansonid.Attributes["value"].Value);

            XmlNode hp = donneesNiveau.SelectSingleNode("Metadonnees/HP");
            if (hp == null)
                throw new ArgumentException("Fichier ne contient pas d'HP de chanson");
            metadonnees.hp = int.Parse(hp.Attributes["value"].Value);
            XmlNode difficulte = donneesNiveau.SelectSingleNode("Metadonnees/Difficulte");
            if (difficulte == null)
                throw new ArgumentException("Fichier ne contient pas de difficulté de chanson");
            metadonnees.difficulte = int.Parse(difficulte.Attributes["value"].Value);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to read metadonnees from path : " + pathToMapXML);
            Debug.LogError(e.StackTrace);
            return null;
        }

        return metadonnees;
    }

    public static Map CreateMapFromName(string mapName)
    {
        string pathToMapXML =
            MainMenuUtil.getMapsDirectory()
            + mapName
            + Path.DirectorySeparatorChar
            + XmlConstants.XML_FILE;

        
        XmlDocument doc;
        Map map = new();
        try
        {
            doc = new XmlDocument();
            doc.Load(pathToMapXML);
            XmlNode donnesNiveau = doc.SelectSingleNode(XmlConstants.ROOT_NODE);

            
            Metadonnees? metaNullable = getMetadonneesFromMapName(mapName);

            XmlNode nodeEvenements = donnesNiveau.SelectSingleNode("Evenements");
            
            if (nodeEvenements == null)
            {
                Debug.Log("was unll");

                map.evenements = new EvenementsMap(new List<Evenement>());
            }
            else
            {
                Debug.Log("not null");
                map.evenements = new EvenementsMap().Deserialize(nodeEvenements);
                Debug.Log("len : " + map.evenements.evenements.Count);
                foreach(var ev in map.evenements.evenements)
                {

                    Debug.Log(ev.GetType() + "has start of " + ev.start + " and end of " + ev.end);
                }
            }

            
            if (metaNullable == null)
            {
                throw new InvalidOperationException(
                    "Impossible de créer map à partir de metadata invalide"
                );
            }
            map.metadonnees = (Metadonnees)metaNullable;

            // Ajout des listes de notes
            map.Notes = GetNotesFromMap(donnesNiveau);

            // Ajout des groupes
            List<string> listeGroupes = new List<string>();
            XmlNode listeGroupesNode = donnesNiveau.SelectSingleNode(
                XmlConstants.LISTE_GROUPE_NODE
            );
            foreach (XmlNode groupe in listeGroupesNode.ChildNodes)
            {
                listeGroupes.Add(groupe.Attributes["nom"].Value);
            }
            map.listeGroupes = listeGroupes;

            // Ajout de la liste des objets lumières
            List<LightObjectInitiaialisePacket> listeObjetLumiere =
                new List<LightObjectInitiaialisePacket>();
            XmlNode listeObjetLumiereNode = donnesNiveau.SelectSingleNode(
                XmlConstants.LISTE_OBJ_LUMIERE_NODE
            );
            string lumiereName = string.Empty;
            int projecteurID;
            string objLumiereType;
            List<string> objLumiereGroupes;
            string[] objLumiereGroupesXML;
            foreach (XmlNode objLumiereNode in listeObjetLumiereNode.ChildNodes)
            {
                projecteurID = Convert.ToInt32(
                    objLumiereNode.Attributes["projecteur"].Value ?? "0"
                );
                objLumiereType = objLumiereNode.Attributes["type"].Value;

                string groupes = objLumiereNode.Attributes["groupes"].Value;
                
                objLumiereGroupesXML = groupes == "" ? Array.Empty<string>() : groupes.Split(XmlConstants.GROUPS_SEPARATOR);
                objLumiereGroupes = new List<string>();
                lumiereName = objLumiereNode.Attributes["nom"].Value;
                ShapeActorType type;

                switch (objLumiereType)
                {
                    case XmlConstants.RECTANGLE_OBJET_NOM:
                        type = ShapeActorType.RECTANGLE;
                        break;
                    case XmlConstants.ELLIPSE_OBJET_NOM:
                        type = ShapeActorType.ELLIPSE;
                        break;
                    default:
                        throw new ArgumentException(
                            "Type " + objLumiereType + " de lumière inconnu."
                        );
                }

                XmlNode etatObjetNode = objLumiereNode.SelectSingleNode(
                    XmlConstants.ETAT_OBJET_NODE
                );
                listeObjetLumiere.Add(
                    new LightObjectInitiaialisePacket(
                        GetEtatObjetFromNode(etatObjetNode),
                        new ObjetLumiereData(
                            type,
                            objLumiereGroupesXML,
                            (ushort)projecteurID,
                            lumiereName
                        )
                    )
                );
            }
            map.ListeObjetLumieres = listeObjetLumiere;

            // Ajout de la liste des effets
            XmlNode listeEffetsNode = donnesNiveau.SelectSingleNode(XmlConstants.LISTE_EFFETS_NODE);
            List<GroupeEffets> listeEffets = getEffetsFromNode(listeEffetsNode);
            map.ListeEffets = listeEffets;
        }
        catch (Exception ex)
        {
            Debug.Log("Erreur de lecture de la map au path de : " + pathToMapXML);
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            return new Map();
        }

        return map;
    }

    public static List<NoteData> GetNotesFromMap(XmlNode donnesNiveau)
    {
        List<NoteData> listeNotes = new List<NoteData>();
        XmlNode notes = donnesNiveau.SelectSingleNode(XmlConstants.LISTE_NOTES_NODE);
        float tempsDebut;
        ushort joueur;
        ushort position;
        float duree;
        foreach (XmlNode note in notes.ChildNodes)
        {
            tempsDebut = float.Parse(
                note.Attributes["tempsDebut"].Value,
                CultureInfo.InvariantCulture
            );
            joueur = ushort.Parse(note.Attributes["joueur"].Value);
            position = ushort.Parse(note.Attributes["position"].Value);
            duree = float.Parse(note.Attributes["duree"].Value, CultureInfo.InvariantCulture);

            List<GroupeEffets> effetsNotes = new();

            if (duree > 0) // Note longue
            {
                if (note.HasChildNodes) // A des effets
                {
                    effetsNotes = getEffetsFromNode(
                        note.ChildNodes
                            .Cast<XmlNode>()
                            .First(x => x.Name == XmlConstants.LISTE_EFFETS_NODE)
                    );

                    Debug.Log("Il y a " + effetsNotes.Count + " effet(s) dans cette note longue");

                    listeNotes.Add(
                        new HoldNoteData(tempsDebut, joueur, position, duree, effetsNotes.First())
                    );
                }
                else
                {
                    listeNotes.Add(new HoldNoteData(tempsDebut, joueur, position, duree));
                }
            }
            else // Note click
            {
                if (note.HasChildNodes)
                {
                    Debug.Log(note.OuterXml);
                    Debug.Log("cnt : " + note.ChildNodes.Count);
                    Debug.Log("name " + note.ChildNodes.Item(0).Name);

                    effetsNotes = getEffetsFromNode(
                        note.ChildNodes
                            .Cast<XmlNode>()
                            .First(x => x.Name == XmlConstants.LISTE_EFFETS_NODE)
                    );

                    Debug.Log("Il y a " + effetsNotes.Count + " effet(s) dans cette note courte");

                    listeNotes.Add(
                        new ClickNoteData(tempsDebut, joueur, position, effetsNotes.First())
                    );
                }
                else
                {
                    listeNotes.Add(new ClickNoteData(tempsDebut, joueur, position));
                }
            }
        }
        return listeNotes.OrderBy(x => x.tempsDebut).ToList();
    }

    public static List<GroupeEffets> getEffetsFromNode(XmlNode node)
    {
        List<GroupeEffets> listeGroupes = new();

        foreach (XmlNode child in node.ChildNodes)
        {
            float tempsDebut = float.Parse(
                child.Attributes["tempsDebut"].Value,
                CultureInfo.InvariantCulture
            );
            List<EffetLumineux> listeEffets = new();

            float decalage;
            string groupeCible;
            float dureeEffet;
            string transitionEffet;
            ushort order = 0;

            foreach (XmlNode effet in child.ChildNodes)
            {
                decalage = float.Parse(
                    effet.Attributes["decalage"].Value,
                    CultureInfo.InvariantCulture
                );
                groupeCible = effet.Attributes["groupeCible"].Value;
                dureeEffet = float.Parse(
                    effet.Attributes["duree"].Value,
                    CultureInfo.InvariantCulture
                );
                transitionEffet = effet.Attributes["transition"].Value;
                XmlAttribute orderAttribute = effet.Attributes["order"];
                if (orderAttribute != null)
                {
                    ushort.TryParse(orderAttribute.Value, out order);
                }

                listeEffets.Add(
                    new EffetLumineux
                    {
                        Decalage = decalage,
                        GroupesCible = groupeCible == "" ? Array.Empty<string>() : groupeCible.Split(XmlConstants.GROUPS_SEPARATOR),
                        Duree = dureeEffet,
                        CourbeAnimation = transitionEffet,
                        State = GetEtatObjetFromNode(
                            effet.ChildNodes.Cast<XmlNode>().First(x => x.Name == "EtatObjet")
                        ),
                        Order = order
                    }
                );
            }

            listeGroupes.Add(new GroupeEffets(listeEffets, tempsDebut));
        }

        return listeGroupes;
    }

    public static LightObjectState GetEtatObjetFromNode(XmlNode node)
    {
        LightState[] listeEtats = new LightState[]
        {
            new PositionLightState(),
            new ScaleLightState(),
            new ColorLightState(),
            new OpacityLightState()
        };

        foreach (XmlNode child in node.ChildNodes)
        {
            StateMeasurement mesure;

            if (child.Attributes["mesure"] == null)
            {
                mesure = StateMeasurement.ABSOLUTE;
            }
            else
                switch (child.Attributes["mesure"]?.Value)
                {
                    case "absolu":
                        mesure = (StateMeasurement.ABSOLUTE);
                        break;
                    case "relatif":
                        mesure = (StateMeasurement.RELATIVE);
                        break;
                    default:
                        throw new ArgumentException("XML contient une mesure inconnue");
                }

            switch (child.Name)
            {
                case XmlConstants.ETAT_COULEUR_NODE:
                    listeEtats[(int)LightStates.COLOR] = new ColorLightState(
                        child.Attributes["couleur"].Value,
                        mesure
                    );
                    break;
                case XmlConstants.ETAT_OPACITE_NODE:
                    listeEtats[(int)LightStates.OPACITY] = new OpacityLightState(
                        float.Parse(
                            child.Attributes["opacite"].Value,
                            CultureInfo.InvariantCulture
                        ),
                        mesure
                    );
                    break;
                case XmlConstants.ETAT_POS_NODE:
                    listeEtats[(int)LightStates.POSITION] = new PositionLightState(
                        getVectorFromNode(child),
                        mesure
                    );
                    break;
                case XmlConstants.ETAT_TAILLE_NODE:
                    listeEtats[(int)LightStates.SCALE] = new ScaleLightState(
                        getVectorFromNode(child),
                        mesure
                    );
                    break;
                default:
                    throw new ArgumentException("XML contient un etat inconnu");
            }
        }

        return new LightObjectState(listeEtats);
    }

    private static Vector2? getVectorFromNode(XmlNode node)
    {
        XmlAttribute x = node.Attributes["x"];
        XmlAttribute y = node.Attributes["y"];

        if (x == null || y == null)
        {
            return null;
        }

        return new Vector2(float.Parse(x.Value), float.Parse(y.Value));
    }
}
