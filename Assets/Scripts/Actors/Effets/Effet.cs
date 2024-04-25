using System.Collections.Generic;
using Riptide;

public interface ICloneable<T>
{
    public T Clone();
}

public class EffetLumineux : IMessageSerializable, ICloneable<EffetLumineux>
{
    public float Decalage = 0.0f;
    public string[] GroupesCible = new string[0];
    public float Duree = 0.0f; 
    public string CourbeAnimation = "";
    public LightObjectState State;
    public ushort Order = 0;

    public EffetLumineux Clone()
    {
        var ret = new EffetLumineux();
        ret.Decalage = Decalage;
        ret.GroupesCible = GroupesCible;
        ret.Duree = Duree;
        ret.CourbeAnimation = CourbeAnimation;
        ret.State = State;
        ret.Order = Order;
        return ret;
    }

    public void Deserialize(Message message)
    {
        Decalage = message.GetFloat();
        GroupesCible = message.GetStrings();
        Duree = message.GetFloat();
        CourbeAnimation = message.GetString();
        State = message.GetSerializable<LightObjectState>();
        Order = message.GetUShort();
    }

    public void Serialize(Message message)
    {
        message.AddFloat(Decalage);
        message.AddStrings(GroupesCible);
        message.AddFloat(Duree);
        message.AddString(CourbeAnimation);
        message.AddSerializable(State);
        message.AddUShort(Order);
    }
}

public class GroupeEffets : IMessageSerializable, ICloneable<GroupeEffets>
{
    public List<EffetLumineux> EffetsLumineux;
    public float TempsDebut;
    
    public GroupeEffets(List<EffetLumineux> effetsLumineux = null, float tempsDebut = 0.0f)
    {
        EffetsLumineux = effetsLumineux ?? new List<EffetLumineux>();
        TempsDebut = tempsDebut;
    }

    public void Serialize(Message message)
    {
        message.AddFloat(TempsDebut);
        message.AddSerializables(EffetsLumineux.ToArray());
    }
    public void Deserialize(Message message)
    {
        TempsDebut = message.GetFloat();
        EffetsLumineux = new List<EffetLumineux>(message.GetSerializables<EffetLumineux>());
    }
    
    public override string ToString()
    {
        return $"{EffetsLumineux.Count} Effets";
    }
    
    public void AddNewEffet()
    {
        var effet = new EffetLumineux();
        effet.State = new LightObjectState().Init();
        EffetsLumineux.Add(effet);
    }
    
    public void RemoveEffet(int effetIndex)
    {
        EffetsLumineux.RemoveAt(effetIndex);
    }

    public GroupeEffets Clone()
    {
        var clone = new GroupeEffets();
        foreach(var eff in EffetsLumineux)
        {
            clone.EffetsLumineux.Add(eff.Clone());
        }
        clone.TempsDebut = TempsDebut;
        return clone;
    }
}
