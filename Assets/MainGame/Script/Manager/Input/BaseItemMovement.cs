using System.Collections;
using System.Collections.Generic;
using Tag.Block;
using UnityEngine;

namespace Tag.Block
{
    public class BaseItemMovement : MonoBehaviour
    {
        #region public veriables

        public LayerMask itemLayerMask;

        #endregion

        #region virtual methods

        public virtual bool ItemPick(Vector3 pos)
        {
            return false;
        }

        public virtual Vector3 ItemDrag(Vector3 pos)
        {
            return Vector3.zero;
        }

        public virtual void ItemPut(Vector3 pos)
        {
        }

        public virtual void OnTouchCancel()
        {

        }

        public virtual bool IsStaticItem()
        {
            return false;
        }

        public virtual bool IsUIMovement()
        {
            return false;
        }
        #endregion

        #region public methods

        public bool GetRayHit(Vector3 pos, out RaycastHit hit, LayerMask layerMask)
        {
            Ray ray = InputManager.EventCamera.ScreenPointToRay(pos);
            Debug.DrawRay(ray.origin, ray.direction * 15, Color.magenta, 0.2f);
            return Physics.Raycast(ray, out hit, 100, layerMask);
        }

        #endregion
    }
}
