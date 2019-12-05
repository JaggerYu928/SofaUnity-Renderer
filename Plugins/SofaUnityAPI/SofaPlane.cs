﻿using UnityEngine;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Class used to handle bindings to the Sofa Plane object, using a Regular Grid topology in 2D.
/// </summary>
public class SofaPlane : SofaBaseMeshAPI
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="simu">Pointer to the SofaPhysicsAPI</param>
    /// <param name="nameID">Name of this Object</param>
    /// <param name="isRigid">Type rigid or deformable</param>
    public SofaPlane(IntPtr simu, string nameID, bool isRigid)
        : base(simu, nameID, isRigid)
    {

    }

    /// Destructor
    ~SofaPlane()
    {
        Dispose(false);
    }

    /// Implicit method to really create object and link to Sofa object. Called by SofaBaseObject constructor
    protected override bool createObject()
    {
        if (m_native == IntPtr.Zero) // first time create object only
        {
            // Create the plane
            int res = sofaPhysicsAPI_addPlane(m_simu, m_name, m_isRigid);
            m_name += "_node";

            if (res != 0)
            {
                Debug.LogError("SofaPlane::createObject plane creation method return error: " + SofaDefines.msg_error[res] + " for object " + m_name);
                return false;
            }

            if (displayLog)
                Debug.Log("plane Added! " + m_name);

            // Set created object to native pointer
            int[] res1 = new int[1];
            m_native = sofaPhysicsAPI_get3DObject(m_simu, m_name, res1);
            
            if (res1[0] != 0 || m_native == IntPtr.Zero)
            {
                Debug.LogError("SofaPlane::createObject get3DObject method returns: " + SofaDefines.msg_error[res1[0]]);
                res1 = null;
                return false;
            }

            res1 = null;
            return true;
        }

        return false;
    }


    /// Post processing method to recompute topology if needed.
    public override void recomputeTopology(Mesh mesh)
    {
        // recompute triangles to face up.
        int[] triangles = mesh.triangles;
        int nbrTri = triangles.Length/3;

        for (int i=0; i<nbrTri; i++)
        {
            int buff = triangles[i * 3 + 1];
            triangles[i * 3 + 1] = triangles[i * 3 + 2];
            triangles[i * 3 + 2] = buff;
        }

        mesh.triangles = triangles;

        // recompute normals to face up.
        Vector3[] norms = mesh.normals;
        for (int i = 0; i < norms.Length; i++)
            norms[i] = new Vector3(0.0f, 1.0f, 0.0f);

        mesh.normals = norms;
    }


    /// Method to recompute the Tex coords according to mesh position and geometry.
    public override void recomputeTexCoords(Mesh mesh)
    {
        Vector3[] verts = mesh.vertices;
        Vector2[] uvs = new Vector2[verts.Length];

        this.computeBoundingBox(mesh);

        // assume plane has normal on the Y value. To be done more generic in the future.
        float rangeX = 1 / (m_max.x - m_min.x);
        float rangeZ = 1 / (m_max.z - m_min.z);

        for (int i = 0; i < verts.Length; i++)
        {
            uvs[i] = new Vector2((m_max.x - verts[i].x) * rangeX, (verts[i].z - m_min.z) * rangeZ);
        }
        
        mesh.uv = uvs;
    }



    /////////////////////////////////////////////////////////////////////////////////////////
    ////////////          Communication API to sofaPhysicsAdvanceAPI         ////////////////
    /////////////////////////////////////////////////////////////////////////////////////////

    [DllImport("SofaAdvancePhysicsAPI", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int sofaPhysicsAPI_addPlane(IntPtr obj, string name, bool isRigid);

}
