using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tag.Block.Editor
{
    public class BlockIdAttributesDrawer : BaseIdAttributesDrawer<BlockIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorCosntant.MAPPING_IDS_PATH + "/BlockIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }
    }
}
