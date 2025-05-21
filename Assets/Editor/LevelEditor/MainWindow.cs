using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;


namespace Tag.Block.Editor
{
    public class MainWindow : OdinEditorWindow
    {
        #region public veriables

        public TitleBar titleBar;

        public LevelCreatore levelCreatore;
        public ItemCreatore itemCreatore;
        public ExitDoorCreatore exitDoorCreatore;
        public BorderCreatore borderCreatore;

        #endregion

        #region privat static veriables

        private static MainWindow instance;

        #endregion

        #region propertices

        public static MainWindow Instance
        {
            get { return instance; }
        }

        #endregion

        [MenuItem("Block Game/Editor/LevelEditor")]
        private static void OpenWindow()
        {
            instance = GetWindow<MainWindow>();
            instance?.Show();
        }

        protected override void OnEnable()
        {
            if (!instance)
                instance = GetWindow<MainWindow>();
            titleBar = new TitleBar("Block GAME", null);

            if (levelCreatore == null)
                levelCreatore = new LevelCreatore();

            if (itemCreatore == null)
            {
                itemCreatore = new ItemCreatore(levelCreatore);
            }

            if (exitDoorCreatore == null)
            {
                exitDoorCreatore = new ExitDoorCreatore(levelCreatore);

            }

            if (borderCreatore == null)
            {
                borderCreatore = new BorderCreatore(levelCreatore);

            }

            levelCreatore.SetItemCreatore(itemCreatore);

            levelCreatore.SetExitDoorCreatore(exitDoorCreatore);

            SceneView.duringSceneGui += this.OnSceneGUI;
        }

       
        void OnSceneGUI(SceneView sceneView)
        {
            if (levelCreatore != null)
            {
                levelCreatore.OnSceneGui();
            }
            if (itemCreatore != null)
            {
                itemCreatore.OnSceneGui();
            }
            if (exitDoorCreatore != null)
            {
                exitDoorCreatore.OnSceneGui();
            }
            if (borderCreatore != null)
            {
                borderCreatore.OnSceneGui();
            }
            if (Event.current.type == EventType.MouseUp)
            {
                if (levelCreatore != null && exitDoorCreatore != null &&
                    levelCreatore.level != null)
                {
                    exitDoorCreatore.OnLevelChanged(levelCreatore.level);
                    itemCreatore.OnLevelChanged(levelCreatore.level);
                }
            }
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

    }
}
