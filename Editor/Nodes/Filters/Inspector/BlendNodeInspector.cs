using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(BlendNode))]
    public class BlendNodeInspector : Editor
    {
        BlendNode n = null;

        public override void OnInspectorGUI()
        {
            n = (BlendNode)target;
            if(n!= null)
            {
                n.DrawInspector();
            }
        }
    }

}