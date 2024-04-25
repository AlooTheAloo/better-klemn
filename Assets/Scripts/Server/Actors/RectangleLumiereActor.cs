using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectangleLumiereActor : ObjetLumiereActor
{
    [SerializeField] Rectangle sprite;
    public override ShapeRenderer GetShape()
    {
        return sprite;
    }
}
