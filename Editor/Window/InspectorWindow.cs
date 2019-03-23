using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{

    public class InspectorWindow : EditorWindow
    {
        public NodeBase node;

        private void OnEnable()
        {
            
        }

        private void OnGUI()
        {
         
            if(node!= null)
            {
                node.DrawInspector();
            }
         
        }
    }

}