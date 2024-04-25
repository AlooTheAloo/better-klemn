using Nova;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Editor
{
    public class ItemLumiereEditeur : MonoBehaviour
    {
        [SerializeField] private ItemView view;
        public static event Action<LightObjectInitiaialisePacket> onDemandeModifier;
        public static event Action<LightObjectInitiaialisePacket> onDemandeDupliquer;

        public void OnClickModifier()
        {
            if(view.Visuals is EditorLightObjectVisuals visuals)
            {
                onDemandeModifier?.Invoke(visuals.packet);
            }
        }
        public void OnClickDupliquer()
        {
            if (view.Visuals is EditorLightObjectVisuals visuals)
            {
                onDemandeDupliquer?.Invoke(visuals.packet);
            }
        }
    }
    
}
