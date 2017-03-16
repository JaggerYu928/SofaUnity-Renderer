﻿using System;
using System.Collections.Generic;
using UnityEngine;
using SofaUnity;
using SofaUnityAPI;

namespace SofaUnity
{
    [ExecuteInEditMode]
    public class SCylinder : SBaseGrid
    {
        /// Mesh of this object
        protected override void createObject()
        {
            IntPtr _simu = m_context.getSimuContext();
            if (_simu != IntPtr.Zero)
                m_impl = new SofaCylinder(_simu, m_context.objectcpt);
        }

        void init()
        {

        }


        // Update is called once per frame
        void Update()
        {
            if (m_log)
                Debug.Log("SCylinder::Update called.");

            updateImpl();
        }

    }
}
