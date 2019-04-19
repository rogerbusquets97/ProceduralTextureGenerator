using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(MakeMaskMap))]
    public class MaskMapInspector : Editor
    {
        MakeMaskMap n = null;
        public override void OnInspectorGUI()
        {
            n = (MakeMaskMap)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
