using System.Xml;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Chroma.Editor;

public class TestsClonable
{
    [Test]
    public void CloneEffetLumineuxCreeEffetAvecMemeAttributs()
    {
        EffetLumineux effet = new EffetLumineux();
        effet.Duree = 7;
        effet.Decalage = 2;
        effet.Order = 7;
        effet.State = new LightObjectState().Init();
        effet.CourbeAnimation = "LINEAR";
        effet.GroupesCible = new string[] { "WYSI" };
        EffetLumineux clone = effet.Clone();
        
        Assert.AreEqual(clone.Duree, effet.Duree);
        Assert.AreEqual(clone.Decalage, effet.Decalage);
        Assert.AreEqual(clone.Order, effet.Order);
        Assert.AreEqual(clone.State, effet.State);
        Assert.AreEqual(clone.CourbeAnimation, effet.CourbeAnimation);
        Assert.AreEqual(clone.GroupesCible, effet.GroupesCible);
    }

    [Test]
    public void CloneObjectFrameCreeObjectFrameAvecMemeAttributs()
    {
        ObjectFrame frame = new ObjectFrame();
        frame.scale = new Vector2(7, 2);
        frame.opacity = 7;
        frame.position = new Vector2(2, 7);
        frame.color = Color.white;
        ObjectFrame clone = frame.Clone();
        Assert.AreEqual(clone.scale, frame.scale);
        Assert.AreEqual(clone.opacity, frame.opacity);
        Assert.AreEqual(clone.position, frame.position);
        Assert.AreEqual(clone.color, frame.color);
    }
}
