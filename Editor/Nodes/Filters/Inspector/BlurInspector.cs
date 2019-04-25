using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(BlurNode))]
    public class BlurInspector : Editor
    {
        BlurNode n = null;
        public override void OnInspectorGUI()
        {
            n = (BlurNode)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
