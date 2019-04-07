using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(FlatColor))]
    public class FlatColorInspector : Editor
    {
        FlatColor n = null;

        public override void OnInspectorGUI()
        {
            n = (FlatColor)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
