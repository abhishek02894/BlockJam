using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.Block
{
    [CreateAssetMenu(menuName = Constant.GAME_NAME + "/Editor/IdMappingConfig")]
    public class BaseIDMappingConfig : SerializedScriptableObject
    {
        public Dictionary<int, string> idMapping = new Dictionary<int, string>();

        public string GetNameFromId(int itemId)
        {
            if (idMapping.ContainsKey(itemId))
            {
                return idMapping[itemId];
            }

            return "";
        }

        public int GetIdFromName(string name)
        {
            foreach (var id in idMapping)
            {
                if (id.Value.Equals(name))
                    return id.Key;
            }

            return -1;
        }

#if UNITY_EDITOR
#endif
    }
}
