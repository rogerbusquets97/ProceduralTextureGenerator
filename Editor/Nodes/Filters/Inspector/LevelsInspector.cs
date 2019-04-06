using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(LevelsNode))]
    public class LevelsInspector : Editor
    {
        LevelsNode n = null;

        public override void OnInspectorGUI()
        {
            n = (LevelsNode)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
