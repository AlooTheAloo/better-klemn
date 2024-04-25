using System.Xml;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class TestsGameplay
{
    [Test]
    public void Test_Precision_Rate()
    {
        StaticStats.Reset();
        StaticStats.nombreNotesRatees[0] = 10;
        StaticStats.nombreNotesParfait[0] = 10;
        StaticStats.nombreNotesTotal[0] = 20;

        Assert.AreEqual(StaticStats.precisions[0], 50);
    }

    [Test]
    public void Test_Precision_Bien()
    {
        StaticStats.Reset();
        StaticStats.nombreNotesBien[0] = 10;
        StaticStats.nombreNotesTotal[0] = 10;
        Assert.AreEqual(StaticStats.precisions[0], Constantes.POINTS_PRECISION[(int) Precision.BIEN]);
    }

    [Test]
    public void Test_Precision_OK()
    {
        StaticStats.Reset();
        StaticStats.nombreNotesOk[0] = 10;
        StaticStats.nombreNotesTotal[0] = 10;
        Assert.AreEqual(StaticStats.precisions[0], Constantes.POINTS_PRECISION[(int)Precision.OK]);
    }
}
