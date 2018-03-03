﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampTestSofa : MonoBehaviour
{
#if sleeveProject
    public ClampTool toolRef;
#endif
    public SPlierTool sofaTool;

    private Animator animator { get { return GetComponent<Animator>(); } }

    private float animSpeed = 0.1f;

    private void Start()
    {
        animator.speed = animSpeed;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            //   new WaitForSeconds(5);
            Clamp();
        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            Unclamp();
        }

#if sleeveProject
        if (toolRef == null) return;
        transform.position = toolRef.sofaTransformRef.position;
        transform.rotation = toolRef.sofaTransformRef.rotation;
#endif
    }

    public void Clamp()
    {
        animator.SetBool("isClamped", true);

        if(sofaTool)
            sofaTool.clampSofaPlier();
    }
    public void Unclamp()
    {
        animator.SetBool("isClamped", false);
        if(sofaTool)
            sofaTool.releaseSofaPlier();
    }
}