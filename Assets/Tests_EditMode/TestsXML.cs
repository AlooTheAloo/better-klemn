using System.Xml;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class TestsXML
{
    [Test]
    public void Test_XML_Util_Convertit_Metadonnes_Node_En_Metadonnees()
    {
        XmlDocument doc = new XmlDocument();

        string XMLNodeSampleString = @"
        <DonneesNiveau>
            <Metadonnees>
                <Titre value=""KillerBeast"" />
                <Artiste value=""Camellia"" />
                <Mappeur value=""727-apps"" />
                <Tempo bpm=""155"" />
                <Duree debut=""0"" fin=""727"" />
                <AudioPreview offset=""40""/>
                <Fichiers chanson=""audio.wav"" couverture=""cover.png"" bg=""bg.png"" video=""video.mp4"" />
                <AR value=""9"" />
                <OD value=""3"" />
                <ChansonID value=""1"" />
            </Metadonnees>
        </DonneesNiveau>
        ";
        doc.LoadXml(XMLNodeSampleString);
        XmlNode node = doc.DocumentElement;

        Metadonnees metadonnees = XmlUtil.GetMetadonneesFromNode(node.SelectSingleNode("/"));

        Assert.AreEqual(metadonnees.titre, "KillerBeast");
        Assert.AreEqual(metadonnees.artiste, "Camellia");
        Assert.AreEqual(metadonnees.mappeur, "727-apps");
        Assert.AreEqual(metadonnees.bpm, 155f);
        Assert.AreEqual(metadonnees.duree.debut, 0f);
        Assert.AreEqual(metadonnees.duree.fin, 727f);
        Assert.AreEqual(metadonnees.audioPreview, 40f);
        Assert.AreEqual(metadonnees.fichiers.audio, "audio.wav");
        Assert.AreEqual(metadonnees.fichiers.imageCouverture, "cover.png");
        Assert.AreEqual(metadonnees.fichiers.arrierePlan, "bg.png");
        Assert.AreEqual(metadonnees.fichiers.video, "video.mp4");
        Assert.AreEqual(metadonnees.ar, 9f);
        Assert.AreEqual(metadonnees.od, 3f);
        Assert.AreEqual(metadonnees.chansonId, 1f);

    }

    [Test]
    public void Test_XML_Util_Convertit_ClickNote_Node_En_NoteData()
    {
        XmlDocument doc = new XmlDocument();

        string XMLNodeSampleString = @"
            <DonnesNiveau>
            <Notes>
            <Note joueur=""1"" position=""4"" tempsDebut=""5.82"" duree=""0"" />
            </Notes>
            </DonnesNiveau>
        ";
        doc.LoadXml(XMLNodeSampleString);
        XmlNode node = doc.DocumentElement;

        List<NoteData> notes = XmlUtil.GetNotesFromMap(node);

        Assert.AreEqual(notes[0].joueur, 0);
        Assert.AreEqual(notes[0].positionNote, 3);
        Assert.AreEqual(notes[0].tempsDebut, 5.82f);
        Assert.True(notes[0] is ClickNoteData);
    }

    [Test]
    public void Test_XML_Util_Convertit_HoldNote_Node_En_NoteData()
    {
        XmlDocument doc = new XmlDocument();

        string XMLNodeSampleString = @"
            <DonnesNiveau>
            <Notes>
            <Note joueur=""1"" position=""4"" tempsDebut=""5.82"" duree=""1"" />
            </Notes>
            </DonnesNiveau>
        ";
        doc.LoadXml(XMLNodeSampleString);
        XmlNode node = doc.DocumentElement;

        List<NoteData> notes = XmlUtil.GetNotesFromMap(node);

        Assert.AreEqual(notes[0].joueur, 0);
        Assert.AreEqual(notes[0].positionNote, 3);
        Assert.AreEqual(notes[0].tempsDebut, 5.82f);
        Assert.True(notes[0] is HoldNoteData);
    }


    [Test]
    public void Test_XML_Etat_A_Partir_De_Node()
    {
        XmlDocument doc = new XmlDocument();

        string XMLNodeSampleString = @"
            <EtatObjet>
                <EtatCouleur couleur=""#FFFFFF""/>
                <EtatOpacite opacite=""0""/>
                <EtatPosition x=""12"" y=""5""/>
                <EtatTaille x=""2"" y=""2""/>
            </EtatObjet>
        ";
        doc.LoadXml(XMLNodeSampleString);
        XmlNode node = doc.DocumentElement;

        var etat = XmlUtil.GetEtatObjetFromNode(node);

        Assert.AreEqual(((PositionLightState) etat.listeEtats[(int)LightStates.POSITION]).targetPosition, new Vector2(12, 5));
        Assert.AreEqual(((ScaleLightState)etat.listeEtats[(int) LightStates.SCALE]).taille, new Vector2(2, 2));
        Assert.AreEqual(((ColorLightState)etat.listeEtats[(int) LightStates.COLOR]).lightColor, new Color(1, 1, 1, 1));
    }
}
