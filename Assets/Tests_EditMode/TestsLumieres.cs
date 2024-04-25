using System.Xml;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class TestsLumieres
{
    [Test]
    public void EtatPositionCalculePositionRelativeCorrectement()
    {
        var pls = new PositionLightState();
        pls.actif = true;
        pls.originalPosition = new Vector3(1, 1);
        pls.targetPosition = new Vector3(10, 10);
        pls.mesure = StateMeasurement.RELATIVE;
        Vector2 delta = pls.getDeltaAtFrame(1, 1, 2, AnimationCurve.Linear(0, 0, 1, 1));
        Assert.AreEqual(delta, new Vector2(5, 5));
    }

    [Test]
    public void EtatPositionCalculePositionAbsolueCorrectement()
    {
        var pls = new PositionLightState();
        pls.actif = true;
        pls.originalPosition = new Vector3(1, 1);
        pls.targetPosition = new Vector3(10, 10);
        pls.mesure = StateMeasurement.ABSOLUTE;
        Vector2 delta = pls.getDeltaAtFrame(1, 1, 2, AnimationCurve.Linear(0, 0, 1, 1));
        Assert.AreEqual(delta, new Vector2(5.5f, 5.5f));
    }


    [Test]
    public void EtatTailleCalculeTailleRelativeCorrectement()
    {
        var sls = new ScaleLightState();
        sls.actif = true;
        sls.tailleOriginal = new Vector3(1, 1);
        sls.taille = new Vector3(10, 1);
        sls.mesure = StateMeasurement.RELATIVE;
        Vector2 delta = sls.getDeltaAtFrame(1, 1, 2, AnimationCurve.Linear(0, 0, 1, 1));
        Assert.AreEqual(delta, new Vector2(5.0f, 0.5f));
    }


    [Test]
    public void EtatTailleCalculeTailleAbsolueCorrectement()
    {
        var sls = new ScaleLightState();
        sls.actif = true;
        sls.tailleOriginal = new Vector3(1, 1);
        sls.taille = new Vector3(10, 1);
        sls.mesure = StateMeasurement.ABSOLUTE;
        Vector2 delta = sls.getDeltaAtFrame(1, 1, 2, AnimationCurve.Linear(0, 0, 1, 1));
        Assert.AreEqual(delta, new Vector2(5.5f, 1f));
    }

    [Test]
    public void EtatCouleurCalculeCouleur()
    {
        var cls = new ColorLightState();
        cls.actif = true;
        cls.initColor = new Color(0, 0, 0);
        cls.lightColor = new Color(100, 100, 100);
        cls.mesure = StateMeasurement.ABSOLUTE;
        Color couleur = cls.getDeltaAtFrame(1, 2, AnimationCurve.Linear(0, 0, 1, 1));
        Assert.AreEqual(couleur, new Color(50, 50, 50));
    }

    [Test]
    public void EtatOpaciteCalculeOpaciteAbsolueCorrectement()
    {
        var ols = new OpacityLightState();
        ols.actif = true;
        ols.originalOpacity = 0;
        ols.opacity = 1;
        ols.mesure = StateMeasurement.ABSOLUTE;
        float opac = ols.getDeltaAtFrame(1, 1, 2, AnimationCurve.Linear(0, 0, 1, 1));
        Assert.AreEqual(opac, 0.5f);
    }

    [Test]
    public void EtatOpaciteCalculeOpaciteRelativeCorrectement()
    {
        var ols = new OpacityLightState();
        ols.actif = true;
        ols.originalOpacity = 0.2f;
        ols.opacity = 0.4f;
        ols.mesure = StateMeasurement.RELATIVE;
        float opac = ols.getDeltaAtFrame(1, 1, 2, AnimationCurve.Linear(0, 0, 1, 1));
        Assert.AreEqual(opac, 0.2f);
    }
}
