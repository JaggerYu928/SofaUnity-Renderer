﻿using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class SofaRayCaster : IDisposable
{
    public SofaRayCaster(IntPtr simu)
    {
        m_simu = simu;

        if (m_simu != IntPtr.Zero)
            sofaPhysicsAPI_createRayCaster(m_simu, 10);
    }

    // TODO: check if needed
    bool m_isDisposed;

    /// Pointer to the SofaPhysicsAPI 
    protected IntPtr m_simu = IntPtr.Zero;

    /// Type of Sofa object mapped to this Object.
    protected string m_type;

    /// Memory free method
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// Memory free method
    protected virtual void Dispose(bool disposing)
    {
        if (!m_isDisposed)
        {
            m_isDisposed = true;
        }
    }

    public int castRay(Vector3 origin, Vector3 direction)
    {

        return -1;
    }


    [DllImport("SofaAdvancePhysicsAPI")]
    public static extern int sofaPhysicsAPI_createRayCaster(IntPtr obj, float length);

    [DllImport("SofaAdvancePhysicsAPI")]
    public static extern int sofaPhysicsAPI_castRay(IntPtr obj, float[] origin, float[] direction);
}
