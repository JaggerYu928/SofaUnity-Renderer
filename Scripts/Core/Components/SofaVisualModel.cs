﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SofaUnity
{
    /// <summary>
    /// Specific class describing a Sofa OblModel, will use a SofaBaseMeshAPI to retrieve info
    /// </summary>
    public class SofaVisualModel : SofaBaseComponent
    {
        ////////////////////////////////////////////
        //////     SofaVisualModel members     /////
        ////////////////////////////////////////////

        /// Member: Unity Mesh object of this GameObject
        protected Mesh m_mesh;

        /// Pointer to the corresponding SOFA API object
        protected SofaBaseMeshAPI m_sofaMeshAPI = null;



        ////////////////////////////////////////////
        //////       SofaVisualModel API       /////
        ////////////////////////////////////////////

        /// Method called by @sa CreateSofaAPI() method. To be implemented by child class if specific ComponentAPI has to be created.
        protected override void CreateSofaAPI_Impl()
        {
            SofaLog("SofaVisualModel::CreateSofaAPI: " + UniqueNameId + " | m_sofaContext: " + m_sofaContext + " | m_sofaContext.GetSimuContext(): " + m_sofaContext.GetSimuContext());
            m_impl = new SofaVisualModelAPI(m_sofaContext.GetSimuContext(), UniqueNameId);

            InitBaseMeshAPI();
        }


        /// Method called by @sa Update() method.
        protected override void Update_impl()
        {
            if (m_sofaMeshAPI == null)
                return;
            
            if (m_sofaMeshAPI.HasTopologyChanged())
            {
                Debug.Log("SofaVisualModel::updateImpl TopologyChanged");
                m_mesh.triangles = m_sofaMeshAPI.createTriangulation();
                //if (m_invertNormals)
                //{
                //    m_sofaMeshAPI.m_invertNormals = m_invertNormals;
                //    invertMeshNormals();
                //}
                m_sofaMeshAPI.setTopologyChange(false);
                m_sofaMeshAPI.updateMesh(m_mesh);
                m_mesh.RecalculateNormals();
            }
            else
            {
                //int res = m_sofaMeshAPI.updateMeshVelocity(m_mesh, m_sofaContext.timeStep);                    
                //if (res == -1)
                //    m_sofaContext.breakerProcedure();
                m_sofaMeshAPI.updateMesh(m_mesh);
            }
            m_mesh.RecalculateBounds();
        }


        ////////////////////////////////////////////
        //////   SofaVisualModel internal API  /////
        ////////////////////////////////////////////

        /// Method called by \sa CreateSofaAPI_Impl to init mesh at start
        protected void InitBaseMeshAPI()
        {
            if (m_sofaMeshAPI == null)
            {
                // Get access to the sofaContext
                IntPtr _simu = m_sofaContext.GetSimuContext();

                if (_simu == IntPtr.Zero)
                    return;
                
                // Create the API object for SofaMesh
                m_sofaMeshAPI = new SofaBaseMeshAPI(m_sofaContext.GetSimuContext(), UniqueNameId, false);
                SofaLog("SofaVisualModel::InitBaseMeshAPI object created");

                m_sofaMeshAPI.loadObject();

                // Add a MeshFilter to the GameObject
                MeshFilter mf = gameObject.GetComponent<MeshFilter>();
                if (mf == null)
                    mf = gameObject.AddComponent<MeshFilter>();

                //to see it, we have to add a renderer
                MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                if (mr == null)
                    mr = gameObject.AddComponent<MeshRenderer>();

                if (mr.sharedMaterial == null)
                {
                    mr.sharedMaterial = new Material(Shader.Find("Diffuse"));
                }

                //MeshCollider collid = gameObject.GetComponent<MeshCollider>();
                //if (collid == null)
                //    gameObject.AddComponent<MeshCollider>();

#if UNITY_EDITOR
                m_mesh = mf.mesh = new Mesh();
#else
                //do this in play mode
                m_mesh = GetComponent<MeshFilter>().mesh;
                if (m_log)
                    Debug.Log("SofaBox::Start play mode.");
#endif

                m_mesh.name = "SofaVisualModel";
                m_mesh.vertices = new Vector3[0];
                m_sofaMeshAPI.updateMesh(m_mesh);

                m_mesh.triangles = m_sofaMeshAPI.createTriangulation();

                m_sofaMeshAPI.recomputeTexCoords(m_mesh);

                SofaLog("SofaVisualModel::initMesh ok: " + m_mesh.vertices.Length);
            }
        }
    }

} // namespace SofaUnity
