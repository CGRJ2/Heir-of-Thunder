using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    [SerializeField] private CinemachineConfiner2D confiner2D;

    void Start()
    {
        confiner2D.InvalidateCache();
    }
}
