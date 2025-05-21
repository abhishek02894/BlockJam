using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class ExitDoorSlot : MonoBehaviour
    {
        #region PUBLIC_VARS
        [SerializeField] private ParticleSystem effect;
        [SerializeField] private GameObject rotatePiece;
        [SerializeField] private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        #endregion

        #region PRIVATE_VARS
        public GameObject RotatePiece => rotatePiece;
        #endregion

        #region UNITY_CALLBACKS
        public void SetMaterial(Material material)
        {
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                meshRenderers[i].material = material;
            }
        }

        public void PlayEffect(bool isPlay)
        {
            if (isPlay)
            {
                effect.Play();
            }
            else
            {
                effect.Stop();
            }
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void SetEffectRotation(Direction raycastDirection)
        {
            // Determine the rotation based on the raycast direction
            Vector3 rotation = Vector3.zero;

            switch (raycastDirection)
            {
                case Direction.Left:
                    rotation = new Vector3(0, 90, 0);
                    break;
                case Direction.Back:
                    rotation = new Vector3(0, -90, 0);
                    break;
                case Direction.Right:
                    rotation = new Vector3(0, -90, 0);
                    break;
                case Direction.Forward:
                    rotation = new Vector3(0, 90, 0);
                    break;
                default:
                    Debug.LogWarning("Unsupported direction for effect rotation: " + raycastDirection);
                    break;
            }

            effect.transform.localRotation = Quaternion.Euler(rotation);
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
