using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace PTG
{
    [CustomEditor(typeof(AmbientOcclusionNode))]
    public class OcclusionInspector : Editor
    {
        AmbientOcclusionNode n = null;
        public override void OnInspectorGUI()
        {
            n = (AmbientOcclusionNode)target;
            if(n!= null)
            {
                n.DrawInspector();
            }
        }
    }
}