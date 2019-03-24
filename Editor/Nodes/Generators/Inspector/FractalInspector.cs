using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(FractalNode))]
    public class FractalInspector : Editor
    {
        FractalNode n = null;

        public override void OnInspectorGUI()
        {
            n = (FractalNode)target;
            if (n!= null)
            {
                n.DrawInspector();
            }
  
        }
        
    }
}
