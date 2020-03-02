﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SofaUnity
{
    public class SofaMesh : SofaBaseComponent
    {
        /// Member: Unity Mesh object of this GameObject
        protected Mesh m_mesh;
        /// Pointer to the corresponding SOFA API object
        protected SofaBaseMeshAPI m_sofaMeshAPI = null;

        /// Initial number of vertices
        int nbVert = 0;

        protected override void CreateSofaAPI()
        {
            if (m_impl != null)
            {
                Debug.LogError("SofaBaseComponent " + UniqueNameId + " already has a SofaBaseComponentAPI.");
                return;
            }

            if (m_sofaContext == null)
            {
                SofaLog("CreateSofaAPI: " + UniqueNameId + " m_sofaContext is null", 1);
                return;
            }

            if (m_sofaContext.GetSimuContext() == null)
            {
                SofaLog("CreateSofaAPI: " + UniqueNameId + " m_sofaContext.GetSimuContext() is null", 1);
                return;
            }

            SofaLog("SofaVisualModel::CreateSofaAPI: " + UniqueNameId + " | m_sofaContext: " + m_sofaContext + " | m_sofaContext.GetSimuContext(): " + m_sofaContext.GetSimuContext());
            m_impl = new SofaVisualModelAPI(m_sofaContext.GetSimuContext(), UniqueNameId);


            InitBaseMeshAPI();
        }


        protected override void SetComponentType()
        {
            // overide name with current type
            m_componentType = m_impl.GetComponentType();
            this.gameObject.name = "SofaMesh" + "  -  " + m_uniqueNameId;
        }

        ///// public method that return the number of vertices, override base method by returning potentially the number of vertices from tetra topology.
        //public override int nbVertices()
        //{
        //    return nbVert;
        //}

        ///// public method that return the number of elements, override base method by returning potentially the number of tetrahedra.
        //public override int nbTriangles()
        //{
        //    return nbTetra;
        //}



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
                    gameObject.AddComponent<MeshFilter>();

                //to see it, we have to add a renderer
                MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                if (mr == null)
                {
                    mr = gameObject.AddComponent<MeshRenderer>();
                    mr.enabled = false;
                }

                if (mr.sharedMaterial == null)
                {
                    mr.sharedMaterial = new Material(Shader.Find("Diffuse"));
                }

                //MeshCollider collid = gameObject.GetComponent<MeshCollider>();
                //if (collid == null)
                //    gameObject.AddComponent<MeshCollider>();

                initMesh(true);
            }
        }


        ////////////////////////////////////////////
        /////       Object behavior API        /////
        ////////////////////////////////////////////

        /// Method called by \sa Start() method to init the current object and impl. @param toUpdate indicate if updateMesh has to be called.
        protected void initMesh(bool toUpdate)
        {
            if (m_sofaMeshAPI == null)
                return;

#if UNITY_EDITOR
            //Only do this in the editor
            MeshFilter mf = GetComponent<MeshFilter>();   //a better way of getting the meshfilter using Generics
            //Mesh meshCopy = Mesh.Instantiate(mf.sharedMesh) as Mesh;  //make a deep copy
            Mesh meshCopy = new Mesh();
            m_mesh = mf.mesh = meshCopy;                    //Assign the copy to the meshes

#else
            //do this in play mode
            m_mesh = GetComponent<MeshFilter>().mesh;
            if (m_log)
                Debug.Log("SofaBox::Start play mode.");
#endif

            m_mesh.name = "SofaMesh";
            m_mesh.vertices = new Vector3[0];
            m_sofaMeshAPI.updateMesh(m_mesh);


            // Special part for tetra
            if (nbTetra == 0)
            {
                nbTetra = m_sofaMeshAPI.GetNbTetrahedra();
                if (nbTetra > 0)
                {
                    SofaLog("Tetra found! Number: " + nbTetra, 1, true);
                    m_tetra = new int[nbTetra * 4];

                    m_sofaMeshAPI.getTetrahedra(m_tetra);
                    m_mesh.triangles = this.computeForceField();
                }
                else
                    m_mesh.triangles = m_sofaMeshAPI.createTriangulation();
            }

            SofaLog("SofaVisualModel::initMesh ok: " + m_mesh.vertices.Length);
            //base.initMesh(false);

            if (toUpdate)
            {
                if (nbTetra > 0)
                    updateTetraMesh();
                else
                    m_sofaMeshAPI.updateMesh(m_mesh);
            }
                
        }

        public bool m_forceUpdate = false;
        /// Method called by @sa Update() method.
        protected override void UpdateImpl()
        {
            SofaLog("SofaVisualMesh::updateImpl called.");

            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

            if (m_impl != null && (m_forceUpdate || mr.enabled))
            {
                // TODO: for the moment the recompute of tetra is too expensive. Only update the number of vertices and tetra
                // Need to find another solution.
                //if (m_impl.hasTopologyChanged())
                //{
                //    m_impl.setTopologyChange(false);

                //    if (nbTetra > 0)
                //        updateTetraMesh();
                //    else
                //        m_impl.updateMesh(m_mesh);
                //}

                if (nbTetra > 0)
                    updateTetraMesh();
                else if (mr.enabled == true) // which is true
                    m_sofaMeshAPI.updateMeshVelocity(m_mesh, m_sofaContext.TimeStep);
                else // pass from false to true.
                {
                    m_sofaMeshAPI.updateMesh(m_mesh);
                }
            }
        }


    }

} // namespace SofaUnity
