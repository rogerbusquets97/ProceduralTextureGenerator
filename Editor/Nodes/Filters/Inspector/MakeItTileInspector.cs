using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    [CustomEditor(typeof(MakeItTile))]
    public class MakeItTileInspector : Editor
    {
        MakeItTile n = null;

        public override void OnInspectorGUI()
        {
            n = (MakeItTile)target;
            if(n!= null)
            {
                n.DrawInspector();
            }
        }
    }
}