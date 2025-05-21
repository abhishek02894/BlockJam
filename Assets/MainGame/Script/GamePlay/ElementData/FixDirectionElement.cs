using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tag.Block
{
    public class FixDirectionElement : BaseElement
    {
        #region PUBLIC_VARS
        [SerializeField] private MeshRenderer meshRenderer;
        private FixDirectionElementData fixDirectionElementData;
        #endregion

        #region PRIVATE_VARS
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public override void Init(BaseElementData baseElementData, BaseItem baseItem)
        {
            base.Init(baseElementData, baseItem);
            fixDirectionElementData = (FixDirectionElementData)baseElementData;
            SetItem();
        }

        public void SetItem()
        {
            if (fixDirectionElementData.IsVertical)
            {
                if (baseItem.verticalFixDirectionParent != null)
                    transform.SetParent(baseItem.verticalFixDirectionParent);
                transform.localEulerAngles = new Vector3(90, 0, 0);
            }
            else
            {
                if (baseItem.horizontalFixDirectionParent != null)
                    transform.SetParent(baseItem.horizontalFixDirectionParent);
                transform.localEulerAngles = new Vector3(90, 90, 0);
            }
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one * 0.5f;
        }
        public override List<MeshRenderer> GetMeshRender()
        {
            List<MeshRenderer> meshRenderers = base.GetMeshRender();
            meshRenderers.Add(meshRenderer);
            return meshRenderers;
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
#if UNITY_EDITOR
        public override void InitForEditor(BaseElementData baseElementData, BaseItem baseItem)
        {
            base.Init(baseElementData, baseItem);
            fixDirectionElementData = (FixDirectionElementData)baseElementData;
            if (fixDirectionElementData.IsVertical)
            {
                if (baseItem.verticalFixDirectionParent != null)
                    transform.SetParent(baseItem.verticalFixDirectionParent);
                transform.localEulerAngles = new Vector3(90, 0, 0);
            }
            else
            {
                if (baseItem.horizontalFixDirectionParent != null)
                    transform.SetParent(baseItem.horizontalFixDirectionParent);
                transform.localEulerAngles = new Vector3(90, 90, 0);
            }
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one * 0.5f;
        }
#endif
    }
}
