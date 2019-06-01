using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace PTG
{
    [CustomEditor(typeof(MinusOneNode))]
    public class OneMinusInspector : Editor
    {
        MinusOneNode n = null;

        public override void OnInspectorGUI()
        {
            n = (MinusOneNode)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
