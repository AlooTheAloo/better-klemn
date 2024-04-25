using System.Xml;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Chroma.Editor;
using Riptide;

public class TestsSerialisation
{
    [Test]
    public void Test_BeatsToSecondes()
    {
        float secs = EditorAlleyManager.BeatsToSecondes(1, 1, 0);
        Assert.AreEqual(60, secs);
    }


    [Test]
    public void Test_SecondesToBeats()
    {
        float secs = EditorAlleyManager.SecondesToBeats(1, 60, 0);
        Assert.AreEqual(1, secs);
    }
}
