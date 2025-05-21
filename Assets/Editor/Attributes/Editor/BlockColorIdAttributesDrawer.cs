using System.Collections.Generic;
using UnityEditor;

namespace Tag.Block.Editor
{
    public class BlockColorIdAttributesDrawer : BaseIdAttributesDrawer<BlockColorIdAttribute>
    {
        protected override void Initialize()
        {
            itemList = AssetDatabase.LoadAssetAtPath<BaseIDMappingConfig>(EditorCosntant.MAPPING_IDS_PATH + "/BlockColorIdMappings.asset");
            values = new List<string>();
            names = new List<string>();
        }

    }
}
