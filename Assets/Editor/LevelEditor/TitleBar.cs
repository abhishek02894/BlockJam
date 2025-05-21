using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block.Editor
{
    [HideReferenceObjectPicker, HideLabel, PropertyOrder(-2)]
    public class TitleBar
    {
        #region private veriables

        private Texture back;
        private Action backAction;
        private string title;

        #endregion

        #region constructor

        public TitleBar(string title, Action action)
        {
            this.title = title;
            backAction = action;
        }

        #endregion

        #region private methods

        [OnInspectorGUI, PropertyOrder(-10)]
        private void OnInspectorGUI()
        {
            var title = new GUIStyle(SirenixGUIStyles.SectionHeaderCentered);
            title.fixedHeight = 50;
            title.fontSize = 30;
            GUIHelper.RequestRepaint();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUIContent gUIContent = new GUIContent("back");
            if (back == null)
            {
                back = EditorExtension.GetEditorTexture("back.png");
            }

            if (back != null)
            {
                gUIContent = new GUIContent(back);
            }

            if (backAction != null)
            {
                if (GUILayout.Button(gUIContent, new GUILayoutOption[] {GUILayout.Width(50), GUILayout.Height(50)}))
                    OnBackButton();
            }

            GUILayout.Label(new GUIContent(this.title), title);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            SirenixEditorGUI.HorizontalLineSeparator();
            GUILayout.Space(10);
            GUILayout.EndVertical();
        }

        private void OnBackButton()
        {
            if (backAction != null)
                backAction.Invoke();
        }

        #endregion
    }
}