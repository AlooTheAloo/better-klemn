using System.Xml;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Chroma.Editor;

public class TestsMessage
{
    [Test]
    public void CreerFormAjouteFieldsCorrectement()
    {
        string error = "Object reference not set to the instance of an object";
        var form = EmileDisturber.CreateFormFromMessage(error);
        Assert.AreNotSame(form.data.Length, 0);
    }
}
