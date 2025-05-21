using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class ThreadRing : MonoBehaviour
    {
        #region PUBLIC_VARS
        [SerializeField] private MeshRenderer m_MeshRenderer;
        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public void SetRing(Material material)
        {
            m_MeshRenderer.material = material;
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion

    }
}
