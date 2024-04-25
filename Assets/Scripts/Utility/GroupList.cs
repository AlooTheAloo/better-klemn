using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnduplicatableList
{
    public List<UnduplicatableItem> list {
        get; private set;
    } = new List<UnduplicatableItem>();

    public void Add(string itemName)
    {
        UnduplicatableItem itemObj = new();
        itemObj.name = itemName;
        itemObj.id = list.FindAll(s => s.name.Equals(itemName)).Count + 1;
        list.Add(itemObj);
    }

    public void Remove(string itemName)
    {
        list.Remove(list.Find(s => s.ToHumanReadable() == itemName));
    }

    public void Modify(string oldname, string newname)
    {
       UnduplicatableItem item = list.Find(s => s.ToHumanReadable() == oldname);
       int dupeIndex = list.FindIndex(s => s.ToHumanReadable().Equals(newname));
       int lastSpace = newname.ToCharArray().ToList().FindLastIndex(s => s.Equals(' '));
       if (dupeIndex != -1 && lastSpace > 0)
       {
           
          
           if (int.TryParse(newname.Substring(lastSpace) , out int result))
           {
               item.name = newname.Substring(0, lastSpace);
               item.id = result + 1;
           }
           else
           {
               item.name = newname;
               item.id = 2;
               
           }
       }
       else
       {
           item.name = newname;
           item.id = 1; //aucune duplique
       }

       if (list.FindAll(s => s.ToHumanReadable().Contains(oldname)).Count == 1)
       {
           list.Find(s => s.ToHumanReadable().Contains(oldname)).id = 1;
       }
       
    }


}

public class UnduplicatableItem : IHumanReadable
{
    public int id;
    public string name;
    public string ToHumanReadable()
    {
        return _ = id == 1 ? name : name + " " + id;
    }
}

public interface IHumanReadable
{
    string ToHumanReadable();
}