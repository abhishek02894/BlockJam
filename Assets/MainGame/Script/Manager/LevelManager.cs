using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Tag.Block
{
    public class LevelManager : SerializedManager<LevelManager>
    {
        #region PUBLIC_VARS
        [SerializeField] private Transform levelParent;
        private Level currentLevel;
        private LevelDataSO currentLevelDataSO;
        public Level CurrentLevel => currentLevel;
        #endregion

        #region PRIVATE_VARS
        public LevelDataSO CurrentLevelDataSO => currentLevelDataSO;
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        [Button]
        public void LoadLevel()
        {
            LevelDataSO levelDataSO = ResourceManager.Instance.GetLevelDataSO(DataManager.PlayerData.playerGameplayLevel);
            if (levelDataSO == null)
                return;
            UnloadLevel();
            currentLevelDataSO = levelDataSO;
            currentLevel = Instantiate(levelDataSO.Level, levelParent);
            currentLevel.transform.localPosition = Vector3.zero;
            currentLevel.Init(levelDataSO.ItemsData);
            MainSceneUIManager.Instance.ShowView<GameplayBottomView>();
        }
        public void LoadLevel(int level)
        {
            LevelDataSO levelDataSO = ResourceManager.Instance.GetLevelDataSO(level);
            if (levelDataSO == null)
                return;
            UnloadLevel();
            currentLevelDataSO = levelDataSO;
            currentLevel = Instantiate(levelDataSO.Level, levelParent);
            currentLevel.transform.localPosition = Vector3.zero;
            currentLevel.Init(levelDataSO.ItemsData);
            MainSceneUIManager.Instance.ShowView<GameplayBottomView>();
        }
        public void LevelWin()
        {
            OnLevelWin(currentLevel);
            DataManager.Instance.IncreaseLevel();
        }
        public void LevelFailed()
        {
            OnLevelFailed(currentLevel);
            MainSceneUIManager.Instance.GetView<LevelRestartView>().Show();
        }
        public void LevelRetry()
        {
            OnLevelFailed(currentLevel);
            LoadLevel();
        }
        public void UnloadLevel()
        {
            if (currentLevel != null)
                Destroy(currentLevel.gameObject);
        }
        public void OnNextLevel()
        {
            LoadLevel();
        }

        // Spawns an extra item of specified blockType and colorType at the given position
        public void SpawnExtraItem(int blockType, int colorType, Vector3 spawnPosition)
        {
            ItemData extraItemData = new ItemData
            {
                blockType = blockType,
                colorType = colorType,
                cellId = -1, // Not using cellId for exact positioning
                elements = new List<BaseElementData>() // No elements on the spawned item
            };

            // Create the item instance
            BaseItem newItem = ResourceManager.Instance.CreateItemFromData(extraItemData, currentLevel.ItemParent);

            if (newItem != null)
            {
                // Initialize the item at the specific position (using the specialized method)
                newItem.InitAtPosition(extraItemData, spawnPosition);

                // Add to the level's item list
                currentLevel.AddItem(newItem);

                // Make the new item appear with a nice effect
                StartCoroutine(AppearExtraItemEffect(newItem));
            }
        }

        // Animate the appearance of the extra item
        private IEnumerator AppearExtraItemEffect(BaseItem item)
        {
            if (item == null)
                yield break;

            // Store original scale
            Vector3 originalScale = item.transform.localScale;

            // Start from zero scale
            item.transform.localScale = Vector3.zero;

            // Create a flash effect
            float flashDuration = 0.3f;
            // Create a scale effect with slight bounce
            float scaleDuration = 0.4f;

            // Flash effect
            MeshRenderer renderer = item.GetComponent<MeshRenderer>();
            Color originalColor = Color.white;
            if (renderer != null && renderer.material != null)
            {
                originalColor = renderer.material.color;
                Color brightColor = Color.white;

                float elapsedFlash = 0f;
                while (elapsedFlash < flashDuration)
                {
                    float t = elapsedFlash / flashDuration;
                    if (renderer.material != null)
                    {
                        renderer.material.color = Color.Lerp(brightColor, originalColor, t);
                    }
                    elapsedFlash += Time.deltaTime;
                    yield return null;
                }

                // Reset color
                if (renderer.material != null)
                {
                    renderer.material.color = originalColor;
                }
            }

            // Scale up with bounce effect
            float elapsed = 0;
            while (elapsed < scaleDuration)
            {
                float t = elapsed / scaleDuration;
                // Bounce effect - overshoot and settle
                float scale = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);

                item.transform.localScale = originalScale * Mathf.Clamp(scale, 0, 1.2f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure final scale is correct
            item.transform.localScale = originalScale;
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private List<Action<Level>> onLevelStart = new List<Action<Level>>();
        public void RegisterOnLevelStart(Action<Level> action)
        {
            if (!onLevelStart.Contains(action))
                onLevelStart.Add(action);
        }
        public void DeregisterOnLevelStart(Action<Level> action)
        {
            if (onLevelStart.Contains(action))
                onLevelStart.Remove(action);
        }
        private void OnLevelStart(Level level)
        {
            for (int i = 0; i < onLevelStart.Count; i++)
            {
                onLevelStart[i]?.Invoke(level);
            }
        }
        private List<Action<Level>> onLevelFailed = new List<Action<Level>>();
        public void RegisterOnLevelFailed(Action<Level> action)
        {
            if (!onLevelFailed.Contains(action))
                onLevelFailed.Add(action);
        }
        public void DeregisterOnLevelFailed(Action<Level> action)
        {
            if (onLevelFailed.Contains(action))
                onLevelFailed.Remove(action);
        }
        private void OnLevelFailed(Level level)
        {
            for (int i = 0; i < onLevelFailed.Count; i++)
            {
                onLevelFailed[i]?.Invoke(level);
            }
        }

        private List<Action<Level>> onLevelWin = new List<Action<Level>>();
        public void RegisterOnLevelWin(Action<Level> action)
        {
            if (!onLevelWin.Contains(action))
                onLevelWin.Add(action);
        }
        public void DeregisterOnLevelWin(Action<Level> action)
        {
            if (onLevelWin.Contains(action))
                onLevelWin.Remove(action);
        }
        private void OnLevelWin(Level level)
        {
            for (int i = 0; i < onLevelWin.Count; i++)
            {
                onLevelWin[i]?.Invoke(level);
            }
        }
        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}
