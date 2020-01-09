﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using SofaUnityAPI;


namespace SofaUnity
{
    /// <summary>
    /// Main class that map the Sofa Context API to a Unity GameObject
    /// This class will control the simulation parameters and load Sofa scene creating a hiearachy between objects.
    /// </summary>
    [ExecuteInEditMode]
    public class SofaContext : MonoBehaviour
    {
        /// Pointer to the Sofa Context API.
        SofaContextAPI m_impl;

        /// Parameter to activate logging of this Sofa GameObject
        public bool m_log = false;

        /// Parameter: Vector representing the gravity force.
        public Vector3 m_gravity = new Vector3(0f, -9.8f, 0f);
        /// Parameter: Float representing the simulation timestep to use.
        public float m_timeStep = 0.02f; // ~ 1/60
        
        /// Booleen to update sofa simulation
        public bool IsSofaUpdating = true;

        /// Booleen to activate sofa message handler
        public bool CatchSofaMessages = true;

        /// Booleen to start Sofa simulation on Play
        [SerializeField]
        public bool StartOnPlay = true;
        
        public bool StepbyStep = false;

        //[SerializeField]
        private SofaDAGNodeManager m_nodeGraphMgr = null;

        [SerializeField]
        private PluginManager m_pluginMgr = null;
        public PluginManager PluginManager
        {
            get { return m_pluginMgr; }
        }

        [SerializeField]
        private SceneFileManager m_sceneFileMgr = null;
        public SceneFileManager SceneFileMgr
        {
            get { return m_sceneFileMgr; }
        }


        List<SRayCaster> m_casters = null;

        public bool testAsync = false;
        

        /// Getter of current Sofa Context API, @see m_impl
        public IntPtr getSimuContext()
        {
            if (m_impl == null)
                init();

            if (m_impl == null) // still null
            {
                Debug.LogError("Error: SofaContext has not be created. method getSimuContext return IntPtr.Zero");
                return IntPtr.Zero;
            }
            return m_impl.getSimuContext();
        }

        /// Getter/Setter of current gravity @see m_gravity
        public Vector3 gravity
        {
            get { return m_gravity; }
            set
            {
                if (m_gravity != value)
                {
                    m_gravity = value;
                    if (m_impl != null)
                        m_impl.setGravity(m_gravity);
                }
            }
        }

        /// Getter/Setter of current timeStep @see m_timeStep
        public float timeStep
        {
            get { return m_timeStep; }
            set
            {
                if (m_timeStep != value)
                {
                    m_timeStep = value;
                    if (m_impl != null)
                        m_impl.timeStep = m_timeStep;
                }
            }
        }


        public Vector3 getScaleSofaToUnity()
        {
            return new Vector3(this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
        }

        public Vector3 getScaleUnityToSofa()
        {
            Vector3 scale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
            for (int i = 0; i < 3; i++)
                if (scale[i] != 0)
                    scale[i] = 1 / scale[i];

            return scale;
        }

        public float getFactorSofaToUnity(int dir = -1)
        {
            Vector3 scale = this.transform.localScale;
            float factor;
            if (dir == -1)
                factor = (Math.Abs(scale.x) + Math.Abs(scale.y) + Math.Abs(scale.z)) / 3;
            else
                factor = scale[dir];

            return factor;
        }

        public float getFactorUnityToSofa(int dir = -1)
        {
            float factor = getFactorSofaToUnity(dir);               
            if (factor != 0.0f) factor = 1 / factor;

            return factor;
        }



        public bool breakerActivated = false;
        private int cptBreaker = 0;
        private int countDownBreaker = 10;
        public void breakerProcedure()
        {
            breakerActivated = true;
            cptBreaker = 0;
        }


        public void registerCaster(SRayCaster obj)
        {
            if (m_casters == null)
                m_casters = new List<SRayCaster>();
            m_casters.Add(obj);
        }

        /// Method called at GameObject creation.
        void Awake()
        {
            if (Application.isPlaying)
                Debug.Log("#### SofaContext is playing | StartOnPlay: " + StartOnPlay);
            else
                Debug.Log("#### SofaContext is editor | StartOnPlay: " + StartOnPlay);

            if (Application.isPlaying && StartOnPlay == false)
                return;

            StartSofa();
        }

        // Use this for initialization
        void Start()
        {

        }

        /// Method called at GameObject destruction.
        void OnDestroy()
        {
            if(m_log)
                Debug.Log("SofaContext::OnDestroy stop called.");
            if (m_impl != null)
            {
                if (m_log)
                    Debug.Log("SofaContext::OnDestroy stop now.");

                if (isMsgHandlerActivated)
                {
                    m_impl.activateMessageHandler(false);
                    isMsgHandlerActivated = false;
                }

                if (m_log)
                    Debug.Log("## SofaContext status before stop: " + m_impl.contextStatus());

                m_impl.stop();

                if (m_log)
                    Debug.Log("## SofaContext status after stop: " + m_impl.contextStatus());

                m_impl.unload();

                if (m_log)
                    Debug.Log("## SofaContext status after unload: " + m_impl.contextStatus());

                m_impl.Dispose();
            }
        }

        private void OnApplicationQuit()
        {
            if (m_casters != null)
            {
                foreach (SRayCaster child in m_casters)
                {
                    if (child != null)
                        child.stopRay();
                }
            }
        }

        void StartSofa()
        {
            // Call the init method to create the Sofa Context
            init();

            if (m_impl == null)
            {
                this.enabled = false;
                this.gameObject.SetActive(false);
                return;
            }

            breakerActivated = false;
            cptBreaker = 0;
        }

        public void resetSofa()
        {
            if (m_impl != null)
            {
                m_impl.reset();
            }
        }


        /// Internal Method to init the SofaContext object
        void init()
        {
            if (this.transform.localScale.x > 0)
            {
                Vector3 scale = this.transform.localScale;
                //scale.x *= -1;
                this.transform.localScale = scale;
            }

            if (m_log)
                Debug.Log("## SofaContext ## init ");

            if (m_impl == null)
            {
                m_impl = new SofaContextAPI(testAsync);

                if (m_nodeGraphMgr == null)
                {
                    m_nodeGraphMgr = new SofaDAGNodeManager(this, m_impl);
                }
                else
                {
                    // TODO make this serializable might help for custom simulation in futur.
                    Debug.Log("## m_nodeGraphMgr already created...");
                }

                // handle sofa plugins
                if (m_pluginMgr == null)
                    m_pluginMgr = new PluginManager(m_impl);
                else
                    m_pluginMgr.SetSofaContextAPI(m_impl);

                m_pluginMgr.LoadPlugins();

                // start sofa instance
                if (m_log)
                    Debug.Log("## SofaContext status before start: " + m_impl.contextStatus());

                m_impl.start();

                if (m_log)
                    Debug.Log("## SofaContext status after start: " + m_impl.contextStatus());

                // handle SOFA scene file
                if (m_sceneFileMgr == null)
                    m_sceneFileMgr = new SceneFileManager(this);
                else
                    m_sceneFileMgr.SetSofaContext(this);

                if (m_sceneFileMgr.HasScene)
                {
                    //m_sceneFileMgr.LoadFilename();
                    ReconnectSofaScene();
                }

                catchSofaMessages();
                if (m_log)
                    Debug.Log("## SofaContext status end init: " + m_impl.contextStatus());

                // set gravity and timestep if changed in editor
                m_impl.timeStep = m_timeStep;
                m_impl.setGravity(m_gravity);
            }
            else
            {
                Debug.LogError("### SofaContext init No Impl");
            }

            
        }
        
        // Update is called once per fix frame
        void FixedUpdate()
        {

        }

        private float nextUpdate = 0.0f;

        // Update is called once per frame
        void Update()
        {
            // only if scene is playing or if sofa is running
            if (IsSofaUpdating == false || Application.isPlaying == false) return; 

            if (testAsync)
                updateImplASync();
            else
                updateImplSync();

            // log sofa messages
            catchSofaMessages();

            // counter if need to freeze the simulation for several iterations
            cptBreaker++;
            if (cptBreaker == countDownBreaker)
            {
                cptBreaker = 0;
                breakerActivated = false;
            }

            if (StepbyStep)
            {
                IsSofaUpdating = false;
            }
        }




        protected void updateImplSync()
        {
            if (Time.time >= nextUpdate)
            {
                nextUpdate += m_timeStep;

                m_impl.step();

                if (m_nodeGraphMgr != null)
                    m_nodeGraphMgr.PropagateSetDirty(true);
            }
        }

        protected void updateImplASync()
        {
            if (Time.time >= nextUpdate)
            {
                nextUpdate += m_timeStep;

                //Debug.Log(Time.deltaTime);

                // if physics simulation async step is still running do not wait and return the control to Unity
                if (m_impl.isAsyncStepCompleted())
                {
                   // Debug.Log("isAsyncStepCompleted: YES ");
                    
                    // physics simulation step completed and is not running
                    // perform data synchronization safely (no need of synchronization locks)                        
                    //if (m_hierarchyPtr.m_objects != null)
                    //{
                    //    // Set all objects to dirty to force and update.
                    //    foreach (SofaBaseObject child in m_hierarchyPtr.m_objects)
                    //    {
                    //        //child.setDirty();
                    //        child.updateImpl();
                    //        //Debug.Log(child.name);
                    //    }
                    //}

                    // update the ray casters
                    if (m_casters != null)
                    {
                        // Set all objects to dirty to force and update.
                        foreach (SRayCaster child in m_casters)
                        {
                            //child.setDirty();
                            child.updateImpl();
                            //Debug.Log(child.name);
                        }
                    }

                    //m_impl.step();
                    // run a new physics simulation async step
                    m_impl.asyncStep();
                }
                //else
                //{
                //    Debug.Log("isAsyncStepCompleted: NO ");
                //}
            }
        }

        private bool isMsgHandlerActivated = false;
        protected void catchSofaMessages()
        {
            // first time activated
            if (CatchSofaMessages && !isMsgHandlerActivated)
            {
                m_impl.activateMessageHandler(true);
                isMsgHandlerActivated = true;
            }
            else if(!CatchSofaMessages && isMsgHandlerActivated)
            {
                m_impl.activateMessageHandler(false);
                isMsgHandlerActivated = false;
            }

            if (isMsgHandlerActivated)
            {
                 m_impl.DisplayMessages();
            }
        }

        //protected void reloadFilename()
        //{
        //    // stop simulation first
        //    m_impl.stop();
        //    m_impl.freeGlutGlew();

        //    // clear hierarchy
        //    //m_hierarchyPtr.clearHierarchy();
        //    List<GameObject> childToDestroy = new List<GameObject>();
        //    foreach (Transform child in this.transform)
        //    {
        //        SofaBaseObject obj = child.GetComponent<SofaBaseObject>();
        //        if (obj != null)
        //        {
        //            childToDestroy.Add(child.gameObject);
        //        }
        //    }

        //    foreach (GameObject child in childToDestroy)
        //        DestroyImmediate(child);

        //    // destroy sofaContext
        //    m_impl.Dispose();
            
        //    // recreate sofaContext
        //    m_impl = new SofaContextAPI(testAsync);
        //    m_pluginMgr.LoadPlugins();
        //    m_impl.start();
            
        //    // loadFilename
        //    loadFilename();
        //}


        /// Method to load a filename and create GameObject per Sofa object found.
        public void LoadSofaScene()
        {
            if (m_sceneFileMgr == null)
                return;

            Debug.Log("## SofaContext ## loadFilename: " + m_sceneFileMgr.AbsoluteFilename());
            // load scene file in SOFA
            m_impl.loadScene(m_sceneFileMgr.AbsoluteFilename());

            // Retrieve current timestep and gravity
            m_timeStep = m_impl.timeStep;
            m_gravity = m_impl.getGravity();

            // recreate node hiearchy in unity
            m_nodeGraphMgr.loadGraph();

            int nbrObj = m_impl.getNumberObjects();
            Debug.Log("######### nbr Objects: " + nbrObj);
            for (int i = 0; i < nbrObj; i++)
            {
                Debug.Log(i + " -> " + m_impl.getObjectName(i));
            }
        }

        protected void ReconnectSofaScene()
        {
            if (m_sceneFileMgr == null)
                return;

            Debug.Log("## SofaContext ## ReconnectSofaScene: " + m_sceneFileMgr.AbsoluteFilename());
            // load scene file in SOFA
            m_impl.loadScene(m_sceneFileMgr.AbsoluteFilename());

            // Do not retrieve timestep of gravity in case it has been changed in editor

            // reconnect node hiearchy in unity
            m_nodeGraphMgr.ReconnectNodeGraph();
        }

        public void ClearSofaScene()
        {
            m_impl.stop();
            m_impl.unload();
            //m_nodeGraphMgr.clear();
            m_impl.start();
        }

    }
}
