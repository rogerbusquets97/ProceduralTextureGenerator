using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(MixNode))]
    public class MixInspector : Editor
    {
        MixNode n = null;
        public override void OnInspectorGUI()
        {
            n = (MixNode)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
