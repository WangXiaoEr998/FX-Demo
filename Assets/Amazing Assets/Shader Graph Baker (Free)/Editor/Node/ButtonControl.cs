// Shader Graph Baker (Free) <https://u3d.as/3ycp>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System;
using System.Reflection;

using UnityEngine.UIElements; 
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing.Controls;


namespace AmazingAssets.ShaderGraphBakerFree.Editor
{
    class ButtonControlAttribute : Attribute, IControlAttribute
    {
        string m_initMethod;
        string m_callbackAction;

        public ButtonControlAttribute(string initMethod, string callbackAction)
        {
            m_initMethod = initMethod;
            m_callbackAction = callbackAction;
        }

        VisualElement IControlAttribute.InstantiateControl(AbstractMaterialNode node, PropertyInfo propertyInfo)
        {
            if (!(node is Node))
                throw new ArgumentException("Property must be a 'Shader Graph Baker' Node.", "node");

            return new ButtonControlView((Node)node, m_initMethod, m_callbackAction);
        }
    }

    class ButtonControlView : VisualElement
    {
        Node m_Node;
        string m_callbackAction;
        UnityEngine.UIElements.Button m_Button;

        public ButtonControlView(Node node, string initMethod, string callbackAction)
        {
            m_Node = node;
            m_callbackAction = callbackAction;

            m_Button = new Button(Callback);
            m_Button.style.height = new StyleLength(30);
            Add(m_Button);

            //Init
            m_Node.GetType().GetMethod(initMethod).Invoke(m_Node, new object[] { m_Button });
        }


        void Callback()
        {
            m_Node.GetType().GetMethod(m_callbackAction).Invoke(m_Node, null);
        }
    }
}
