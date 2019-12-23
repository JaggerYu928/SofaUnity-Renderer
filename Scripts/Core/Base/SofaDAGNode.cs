﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SofaUnityAPI;

namespace SofaUnity
{
    public class SofaDAGNode : SofaBase
    {
        /// Pointer to the Sofa Context API.
        SofaDAGNodeAPI m_impl = null;

        protected string m_parentNodeName = "None";
        public string getParentName() { return m_parentNodeName; }


        void Awake()
        {
            Debug.Log("######## Awake SofaDAGNode: " + UniqueNameId);

            if (m_impl == null)
                Debug.Log("###### HAS impl");
            else
                Debug.Log("###### NO impl");
        }


        protected override void InitImpl() 
        {
            if (m_impl == null)
            Debug.Log("####### SofaDAGNode::InitImpl: " + UniqueNameId);
            if (m_impl == null) 
                CreateSofaAPI();
            else
                Debug.Log("SofaDAGNode::InitImpl, already created: " + UniqueNameId);
        }


        protected void CreateSofaAPI()
        {
            Debug.Log("####### SofaDAGNode::CreateSofaAPI: " + UniqueNameId);
            if (m_impl != null)
            {
                Debug.LogError("SofaDAGNode " + UniqueNameId + " already has a SofaDAGNodeAPI.");
                return;
            }

            m_impl = new SofaDAGNodeAPI(m_sofaContext.getSimuContext(), UniqueNameId);

            string componentsS = m_impl.GetDAGNodeComponents();            
            if (componentsS.Length == 0)
                return;

            SofaLog("#####################!!!############################ SofaDAGNode: " + UniqueNameId + " -> " + componentsS);

            List<string> compoNames = ConvertStringToList(componentsS);
            foreach (string compoName in compoNames)
            {
                string baseType = m_impl.GetBaseComponentType(compoName);

                if (baseType.Contains("Error"))
                    SofaLog("Component " + compoName + " returns baseType: " + baseType, 2);                    
                else
                    SComponentFactory.CreateSofaComponent(compoName, baseType, this, this.gameObject);
            }

            m_parentNodeName = m_impl.GetParentNodeName();
            if (m_parentNodeName.Contains("Error"))
            {
                SofaLog("Node Parent Name return error: " + m_parentNodeName + ", will use None.");
                m_parentNodeName = "None";
            }
        }

    }

} // namespace SofaUnity
