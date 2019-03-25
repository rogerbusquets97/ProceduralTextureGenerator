using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(CellularNode))]
    public class CellularInspector : Editor
    {
        CellularNode n = null;

        public override void OnInspectorGUI()
        {
            n = (CellularNode)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }

}