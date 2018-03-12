﻿using UnityEngine;
using SofaUnity;

/// <summary>
/// Script that create a 2D texture from a raw data.
/// work in progress.
/// </summary>
public class Texture2DFromRaw : MonoBehaviour
{
    ////////////////////////////////////////////
    /////          Object members          /////
    ////////////////////////////////////////////

    /// GameObject having the SComponentObject that is listening the raw img Data
    public GameObject target;
    /// SComponentObject that is listening the raw img Data 
    protected SComponentObject m_object = null;

    /// 2D texture object created in this component
    protected Texture2D m_texture = null;
    /// Size of the texture
    protected int texWidth = 400;
    protected int texHeight = 400;

    /// Name of the Data containing the raw data
    public string dataName = "";
    /// Pointer to the Data containing the raw data
    protected SData rawImg = null;

    /// Name of the Data containing the raw diff data
    public string dataName2 = "";
    /// Pointer to the Data containing the raw diff data
    protected SData rawImgDiff = null;

    /// raw data of the 2d texture
    protected float[] m_rawData = null;


    

    ////////////////////////////////////////////
    /////       Object behavior API        /////
    ////////////////////////////////////////////

    public void Start()
    {
        m_object = target.GetComponent<SComponentObject>();
        foreach (SData entry in m_object.datas)
        {
            if (entry.nameID == dataName)
            {
                Debug.Log("found: " + dataName);
                rawImg = entry;
            }
            else if (entry.nameID == dataName2)
            {
                rawImgDiff = entry;
            }
        }
    }


    protected bool firstTime = true;
    protected bool initDiff = false;
    public void Update()
    {
        if (m_object != null)
        {
            if (m_texture == null && rawImg != null) // first time create init texture
            {
                int res = m_object.impl.getVecfSize(rawImg.nameID);
                if (res == 0)
                    return;

                if (m_texture == null)
                {
                    m_texture = new Texture2D(texWidth, texHeight);
                    m_rawData = new float[res];
                    GetComponent<Renderer>().material.mainTexture = m_texture;
                }
                //for (int i = 0; i < 100; i++)
                //    m_rawData[i] = 69;
                

                int resValue = m_object.impl.getVecfValue(rawImg.nameID, res, m_rawData);
                int cpt = 0;
                int cpt1 = 0;
                //var line = "";
                for (int y = 0; y < m_texture.height; y++)
                {                    
                    for (int x = 0; x < m_texture.width; x++)
                    {
                        //Color color = ((x & y) != 0 ? Color.white : Color.gray);
                        float value = m_rawData[cpt];
                       // line = line + value + " ";
                        //if (cpt<1000)
                        //Debug.Log(cpt + " -> " + value);

                        if (value == 1)
                            cpt1++;

                        m_texture.SetPixel(x, y, new Vector4(value, value, value, 1));
                        ////m_texture.SetPixel(x, y, color);
                        cpt++;
                    }

                   // line = line + " || ";                    
                }
               // Debug.Log(line);
               // Debug.Log("cpt1: " + cpt1);
                m_texture.Apply();
                return;
            }

            
            if (m_texture != null && rawImgDiff != null) // second time, init diff image (used for optimisation as sparse data)
            {
                int resDiff = m_object.impl.getVecfSize(rawImgDiff.nameID);
                //Debug.Log("resDiff : " + resDiff);
                if (resDiff == 0)
                    return;

                if (initDiff == false)
                {
                    m_rawData = new float[resDiff];
                    initDiff = true;
                }

                int resValue = m_object.impl.getVecfValue(rawImgDiff.nameID, resDiff, m_rawData);

                for (int i = 0; i < resDiff; i += 2)
                {
                    int id = (int)m_rawData[i];
                    if (id == -1)
                    {
                       // Debug.Log("Stop at: " + i*0.5);
                        break;
                    }

                    int y = (int)Mathf.Floor(id / texWidth);
                    int x = id % texHeight;
                    float value = m_rawData[i + 1];
                    m_texture.SetPixel(x, y, new Vector4(value, value, value, 1));
                }
                m_texture.Apply();
            }
        }
    }
}