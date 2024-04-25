using System.Xml;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class TestsListeGroupes
{
    [Test]
    public void Test_Ajouter_Groupe_Simple()
    {
        UnduplicatableList list = new UnduplicatableList();
        list.Add("Hello");

        Assert.AreEqual(1, list.list.Count);
        Assert.AreEqual("Hello", list.list[0].ToHumanReadable());

        list.Add("HelloWorld");

        Assert.AreEqual(2, list.list.Count);
        Assert.AreEqual("HelloWorld", list.list[1].ToHumanReadable());
    }

    [Test]
    public void Test_Enlever_Groupe_Simple()
    {
        UnduplicatableList list = new UnduplicatableList();
        list.Add("Hello");
        Assert.AreEqual(1, list.list.Count);
        list.Remove("Hello");
        Assert.AreEqual(0, list.list.Count);
    }

    [Test]
    public void Test_Modifier_Simple()
    {
        UnduplicatableList list = new UnduplicatableList();
        list.Add("Hello");
        list.Modify("Hello", "Gamer");

        Assert.AreEqual("Gamer", list.list[0].ToHumanReadable());
    }

    [Test]
    public void Test_Ajouter_Duplique()
    {
        UnduplicatableList list = new UnduplicatableList();
        list.Add("Hello");
        list.Add("Hello");

        Assert.AreEqual(2, list.list.Count);
        Assert.AreEqual("Hello", list.list[0].ToHumanReadable());
        Assert.AreEqual("Hello 2", list.list[1].ToHumanReadable());
    }

    [Test]
    public void Test_Supprimer_Duplique()
    {
        UnduplicatableList list = new UnduplicatableList();
        list.Add("Hello");
        list.Add("Hello");
        list.Remove("Hello");
        list.Remove("Hello 2");

        Assert.AreEqual(0, list.list.Count);
    }

    [Test]
    public void Test_Modifier_Duplique()
    {
        UnduplicatableList list = new UnduplicatableList();
        list.Add("Hello");
        list.Add("Hello");
        list.Modify("Hello", "Gamer");

        Assert.AreEqual("Gamer", list.list[0].ToHumanReadable());
        Assert.AreEqual("Hello", list.list[1].ToHumanReadable());
    }

    [Test]
    public void Test_Modifier_Duplique_Edge_Case()
    {
        UnduplicatableList list = new UnduplicatableList();
        list.Add("Hello");
        list.Add("Hello");
        list.Modify("Hello", "Hello 2"); // Ne marche pas ! Se fait renommer "Hello 3"

        Assert.AreEqual("Hello 2", list.list[1].ToHumanReadable());
        Assert.AreEqual("Hello 3", list.list[0].ToHumanReadable());

        list.Modify("Hello 2", "Gamer");
        
        Assert.AreEqual("Gamer", list.list[1].ToHumanReadable());
    }



}
