using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class DisableHybridRendering : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<SkinnedMeshRenderer>().enabled = false;
    }
}
