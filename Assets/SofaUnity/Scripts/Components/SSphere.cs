﻿using System;
using System.Collections.Generic;
using UnityEngine;
using SofaUnity;
using SofaUnityAPI;

namespace SofaUnity
{
    [ExecuteInEditMode]
    public class SSphere : SGrid
    {
        /// Mesh of this object
        protected override void createObject()
        {
            if (m_impl != null)
                m_context.objectcpt = m_context.objectcpt + 1;
            else
                Debug.LogError("SVisualMesh:: Object not created");

            IntPtr _simu = m_context.getSimuContext();
            if (_simu != IntPtr.Zero)
                m_impl = new SofaSphere(_simu, m_context.objectcpt, false);
        }

        // Update is called once per frame
        protected override void updateImpl()
        {
            if (m_log)
                Debug.Log("SVisualMesh::updateImpl called.");

            if (m_impl != null)
            {
                m_impl.updateMesh(m_mesh);
                m_mesh.RecalculateNormals();
            }
        }

    }
}
