using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(WarpNode))]
    public class WarpInspector : Editor
    {
        WarpNode n = null;
        public override void OnInspectorGUI()
        {
            n = (WarpNode)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
