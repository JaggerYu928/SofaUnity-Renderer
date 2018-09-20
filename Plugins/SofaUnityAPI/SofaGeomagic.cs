﻿using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class SofaGeomagic : IDisposable
{
    /// Name of the Sofa object mapped to this Object.
    protected string m_name;

    /// Pointer to the SofaPhysicsAPI 
    protected IntPtr m_simu = IntPtr.Zero;

    /// Parameter to activate internal logging
    protected bool log = true;

    // TODO: check if needed
    bool m_isDisposed;

    /// Memory free method
    public void Dispose()
    {
        //Dispose(true);
        //GC.SuppressFinalize(this);
    }

    /// Memory free method
    protected virtual void Dispose(bool disposing)
    {
        //if (!m_isDisposed)
        //{
        //    m_isDisposed = true;
        //}
    }

    public SofaGeomagic(IntPtr simu, string nameID)
    {
        m_simu = simu;
        m_name = nameID;

        int res = 0;
        if (m_simu != IntPtr.Zero)
        {
            res = sofaPhysicsAPI_createGeomagicManager(m_simu, m_name);
            if (res < 0)
                Debug.LogError("SofaPliers creation: " + m_name + " returns error: " + res);
        }
        else
            Debug.LogError("SofaEntact creation: " + nameID + " failed. Can't access Object Pointer simulation.");
        Debug.Log("Creation returns: " + res);
    }

    public void geomagicPosition()
    {
        float[] val = new float[7];
        int res = sofaPhysicsAPI_getGeomagicPosition(m_simu, m_name, val);

        Debug.Log("Position returns: " + res);
        Debug.Log(val[0] + " " + val[1] + " " + val[2] + " " + val[3] + " " + val[4] + " " + val[5] + " " + val[6]);

    }

   

    /////////////////////////////////////////////////////////////////////////////////////////
    ///////////      Communication API to set/get basic values into Sofa     ////////////////
    /////////////////////////////////////////////////////////////////////////////////////////

    [DllImport("SofaAdvancePhysicsAPI", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int sofaPhysicsAPI_createGeomagicManager(IntPtr obj, string nameID);

    [DllImport("SofaAdvancePhysicsAPI", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int sofaPhysicsAPI_getGeomagicPosition(IntPtr obj, string nameID, float[] values);

    [DllImport("SofaAdvancePhysicsAPI", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int sofaPhysicsAPI_getGeomagicStatus(IntPtr obj, string nameID, int[] value);
}
