﻿using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class SofaSphere : SofaMeshObject
{
    public SofaSphere(IntPtr simu, int idObject, bool isRigid)
        : base(simu, idObject, isRigid)
    {

    }

    ~SofaSphere()
    {
        Dispose(false);
    }

    public override int[] createTriangulation()
    {
        return base.createTriangulation();
    }

    protected override void createObject()
    {
        m_name = "sphere_" + m_idObject + "_node";

        if (m_native == IntPtr.Zero) // first time create object only
        {
            // Create the sphere
            int res = sofaPhysicsAPI_addSphere(m_simu, "sphere_" + m_idObject, m_isRigid);
            if (res == 1) // sphere added
            {
                Debug.Log("sphere Added! " + m_name);

                // Set created object to native pointer
                m_native = sofaPhysicsAPI_get3DObject(m_simu, m_name);
            }

            //    m_native = sofaPhysicsAPI_get3DObject(m_simu, "truc1");

            if (m_native == IntPtr.Zero)
                Debug.LogError("Error sphere created can't be found!");
        }
    }

    [DllImport("SofaAdvancePhysicsAPI", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int sofaPhysicsAPI_addSphere(IntPtr obj, string name, bool isRigid);

}
