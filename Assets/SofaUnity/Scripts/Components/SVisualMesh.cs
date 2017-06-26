﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SofaUnity;
using SofaUnityAPI;
using System;

namespace SofaUnity
{
    [ExecuteInEditMode]
    class SVisualMesh : SBaseMesh
    {
        private void Awake()
        {
            if (m_log)
                Debug.Log("UNITY_EDITOR - SVisualMesh::Awake");

            loadContext();

            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            if (mf == null)
                gameObject.AddComponent<MeshFilter>();
            
            //to see it, we have to add a renderer
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = gameObject.AddComponent<MeshRenderer>();
        }

        protected override void initMesh()
        {
            if (m_impl == null)
                return;

            m_mesh.name = "SofaMesh";
            m_mesh.vertices = new Vector3[0];
            m_impl.updateMesh(m_mesh);
            m_mesh.triangles = m_impl.createTriangulation();
            m_impl.updateMesh(m_mesh);
            m_impl.recomputeTexCoords(m_mesh);
        }

        protected override void createObject()
        {
            if (m_impl != null)
                m_context.objectcpt = m_context.objectcpt + 1;
            else
                Debug.LogError("SVisualMesh:: Object not created");

            IntPtr _simu = m_context.getSimuContext();
            if (_simu != IntPtr.Zero)
            {
                m_impl = new SofaMeshObject(_simu, m_context.objectcpt, false);
                m_impl.loadObject();
            }
        }

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
