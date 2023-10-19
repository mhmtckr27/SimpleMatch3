using System;
using UnityEngine;

namespace SimpleMatch3.Util
{
    public class SetTargetFrameRate : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}