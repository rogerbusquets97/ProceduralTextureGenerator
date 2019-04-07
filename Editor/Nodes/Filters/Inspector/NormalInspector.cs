using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(NormalNode))]
    public class NormalInspector : Editor
    {
        NormalNode n = null;

        public override void OnInspectorGUI()
        {
            n = (NormalNode)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
