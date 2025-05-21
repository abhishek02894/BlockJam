using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tag.Block
{
    public class ExtraItemElement : BaseElement
    {
        #region PUBLIC_VARS
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer extraItemRender;
        private ExtraItemElementData extraItemElementData;
        #endregion

        #region PRIVATE_VARS
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public override void Init(BaseElementData baseElementData, BaseItem baseItem)
        {
            base.Init(baseElementData, baseItem);
            extraItemElementData = (ExtraItemElementData)baseElementData;
            SetMaterial();
        }

        private void SetMaterial()
        {
            meshFilter.mesh = ResourceManager.Instance.GetExtraItemMesh(baseItem.blocktype);
            extraItemRender.material = ResourceManager.Instance.GetMaterial(baseItem.ColorType);
            baseItem.SetRenderer(ResourceManager.Instance.GetMaterial(extraItemElementData.ExtraColorType));
        }

        public override List<MeshRenderer> GetMeshRender()
        {
            List<MeshRenderer> meshRenderers = base.GetMeshRender();
            meshRenderers.Add(extraItemRender);
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
            extraItemElementData = (ExtraItemElementData)baseElementData;
            SetMaterialForEditor();
        }

        private void SetMaterialForEditor()
        {
            Material materialExtra = AssetDatabase.LoadAssetAtPath<Material>("Assets/MainGame/Material/BlockMaterials/Block_" + baseItem.ColorType + ".mat");
            extraItemRender.material = materialExtra;

            Material materialItem = AssetDatabase.LoadAssetAtPath<Material>("Assets/MainGame/Material/BlockMaterials/Block_" + extraItemElementData.ExtraColorType + ".mat");
            baseItem.SetRenderer(materialItem);

            Mesh MeshRenderer = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/MainGame/Mesh/Block00.001.asset");
            meshFilter.mesh = MeshRenderer;
        }
#endif
    }
}
