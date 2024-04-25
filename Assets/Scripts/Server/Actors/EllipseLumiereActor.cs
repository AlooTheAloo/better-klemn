using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EllipseLumiereActor : ObjetLumiereActor
{
    [SerializeField] Disc sprite;
    public override ShapeRenderer GetShape()
    {
        return sprite;
    }
}
