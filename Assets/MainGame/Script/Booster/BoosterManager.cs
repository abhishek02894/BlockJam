using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    public class BoosterManager : SerializedManager<BoosterManager>
    {
        #region PUBLIC_VARS

        public bool IsAnyBoosterActive
        {
            get { return currentActiveBooster != null; }
        }

        public BaseBooster CurrentActiveBooster { get => currentActiveBooster; }

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private BoosterConfig[] boosterConfigs;
        private Dictionary<int, BaseBooster> boosters = new Dictionary<int, BaseBooster>();
        [ShowInInspector] private BaseBooster currentActiveBooster;

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            for (int i = 0; i < boosterConfigs.Length; i++)
            {
                if (!boosters.ContainsKey(boosterConfigs[i].boosterId))
                    boosters.Add(boosterConfigs[i].boosterId, boosterConfigs[i].booster);
            }
        }

        #endregion

        #region PUBLIC_FUNCTIONS
        [Button]
        public void ActvieBooster(int boosterId, Action onUse)
        {
            currentActiveBooster = boosters[boosterId];
            currentActiveBooster.OnActive(onUse);
        }

        public void DeActvieBooster()
        {
            currentActiveBooster = null;
        }

        public BaseBooster GetBooster(int boosterId)
        {
            return boosters[boosterId];
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
    }
    public class BoosterConfig
    {
        [CurrencyId] public int boosterId;
        public BaseBooster booster;
    }
}
