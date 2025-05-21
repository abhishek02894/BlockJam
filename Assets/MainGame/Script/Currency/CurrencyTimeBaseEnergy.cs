using System;
using UnityEngine;

namespace Tag.Block
{
    [CreateAssetMenu(menuName = Constant.GAME_NAME + "/Currency TimeBase Energy")]
    public class CurrencyTimeBaseEnergy : CurrencyTimeBase
    {
        public int defaultEnergyValue;

        #region Ovreride_Methods
        public override int UnitTimeUpdate
        {
            get { return unitTimeUpdate; }
        }

        public override void Init()
        {
            base.Init();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        public override void Add(int value, Action successAction = null, Vector3 position = default(Vector3))
        {
            if (value >= 0)
            {
                base.Add(value, successAction, position);
                return;
            }

            value = Mathf.Abs(value);
            if (Value >= value)
            {
                base.Add(-value, successAction, position);
                successAction?.Invoke();
            }
        }

        public override bool IsSufficentValue(int value)
        {
            if (Value >= value)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Public Function
        #endregion

        #region private Funcation

        #endregion
    }
}