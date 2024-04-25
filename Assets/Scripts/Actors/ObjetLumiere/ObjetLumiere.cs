using Riptide;
using System.Collections.Generic;
using UnityEngine;

public struct ObjetLumiereData : IMessageSerializable
{
    public string nomLumiere;
    public ShapeActorType type;
    public string[] groupes;
    public ushort projectorID;

    public ObjetLumiereData(ShapeActorType type, string[] groupes, ushort projectorID, string name)
    {
        nomLumiere = name;
        this.groupes = groupes;
        this.type = type;
        this.projectorID = projectorID;
    }
    public ObjetLumiereData init()
    {
        type = ShapeActorType.ELLIPSE;
        groupes = new string[0];
        projectorID = 0;
        nomLumiere = "Nouveau Laser";
        return this;
    }


    public void Serialize(Message message)
    {
        message.Add((ushort)type);
        message.Add(groupes);
        message.Add(projectorID);
    }

    public void Deserialize(Message message)
    {
        type = (ShapeActorType) message.GetUShort();
        groupes = message.GetStrings();
        projectorID = message.GetUShort();

    }
}

public enum StateMeasurement
{
    ABSOLUTE = 0,
    RELATIVE = 1
}

public enum LightStates
{
    POSITION = 0,
    SCALE = 1,
    COLOR = 2,
    OPACITY = 3,
}

public struct LightObjectState : IMessageSerializable
{
    public LightState[] listeEtats;

    public LightObjectState(LightState[] etats)
    {
        listeEtats = etats;
    }

    public LightObjectState Init()
    {
        List<LightState> lightStates = new List<LightState>()
        {
            new PositionLightState(new Vector2(0, 0), StateMeasurement.ABSOLUTE),
            new ScaleLightState(new Vector2(0, 0), StateMeasurement.ABSOLUTE),
            new ColorLightState("#000000", StateMeasurement.ABSOLUTE),
            new OpacityLightState(0, StateMeasurement.ABSOLUTE)
        };
        listeEtats = lightStates.ToArray();
        return this;
    }



    public void Deserialize(Message message)
    {
        listeEtats = new List<LightState>
        {
            message.GetSerializable<PositionLightState>(),
            message.GetSerializable<ScaleLightState>(),
            message.GetSerializable<ColorLightState>(),
            message.GetSerializable<OpacityLightState>()
        }.ToArray();
    }

    public void Serialize(Message message)
    {
        foreach(var state in listeEtats)
        {
            message.AddSerializable(state);
        }
    }
}


public struct LightObjectInitiaialisePacket : IMessageSerializable, ICloneable<LightObjectInitiaialisePacket>
{
    public LightObjectState initialState;
    public ObjetLumiereData lumiereData;

    public void Deserialize(Message message)
    {
        initialState = message.GetSerializable<LightObjectState>();
        lumiereData = message.GetSerializable<ObjetLumiereData>();
    }
    public void Serialize(Message message)
    {
        message.AddSerializable(initialState);
        message.AddSerializable(lumiereData);
    }

    public LightObjectInitiaialisePacket Clone()
    {
        LightObjectInitiaialisePacket clone = new LightObjectInitiaialisePacket();
        clone.initialState = initialState;
        clone.lumiereData = lumiereData;
        return clone;
    }

    public LightObjectInitiaialisePacket(LightObjectState initialState, ObjetLumiereData lumiereData)
    {
        this.initialState = initialState;
        this.lumiereData = lumiereData;
    }
}

public enum ShapeActorType
{
    RECTANGLE,
    ELLIPSE
}


