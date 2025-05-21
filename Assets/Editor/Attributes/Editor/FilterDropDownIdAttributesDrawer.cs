using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tag.Block.Editor
{
    public class FilterDropDownIdAttributesDrawer<T> : OdinAttributeDrawer<T, int> where T : BaseIdAttribute
    {
        public List<string> values = new List<string>();
        public List<string> names = new List<string>();
        public Dictionary<string, int> mapValues = new Dictionary<string, int>();
        public Dictionary<int, string> map = new Dictionary<int, string>();
        public static BaseIDMappingConfig itemList;
        public string searchName = "";
        public string oldSearchName = " ";

        protected string[] dropDownOptions;
        protected int selectedIndex = 0;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();

            if (values.Count > 0 && map.ContainsKey(this.ValueEntry.SmartValue))
            {
                string item = SirenixEditorFields.Dropdown(label, map[this.ValueEntry.SmartValue], values.ToArray(), names.ToArray());
                this.ValueEntry.SmartValue = mapValues[item];
            }
            else
            {
                SirenixEditorGUI.ErrorMessageBox("No IDs found for: " + searchName);
                this.ValueEntry.SmartValue = SirenixEditorFields.IntField(label, this.ValueEntry.SmartValue);
            }

            selectedIndex = EditorGUILayout.Popup(selectedIndex, dropDownOptions, GUILayout.Width(150));
            searchName = dropDownOptions[selectedIndex];

            if (!oldSearchName.Equals(searchName))
            {
                FilterIDs();
            }
            EditorGUILayout.EndHorizontal();
        }

        protected void FilterIDs()
        {
            oldSearchName = searchName;
            if (itemList != null && itemList.idMapping != null && itemList.idMapping.Count > 0)
            {
                map = itemList.idMapping;
                names.Clear();
                values.Clear();
                mapValues.Clear();

                foreach (var m in itemList.idMapping)
                {
                    if (searchName == "All") 
                    {
                        names.Add(m.Key + "-" + m.Value);
                        values.Add(m.Value);
                        if (!mapValues.ContainsKey(m.Value))
                            mapValues.Add(m.Value, m.Key);
                    }
                    else if (m.Value.StartsWith(searchName))
                    {
                        names.Add(m.Key + "-" + m.Value);
                        values.Add(m.Value);
                        if (!mapValues.ContainsKey(m.Value))
                            mapValues.Add(m.Value, m.Key);
                    }
                }
            }
        }
    }
}
