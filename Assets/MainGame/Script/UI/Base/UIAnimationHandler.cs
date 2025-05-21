using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class UIAnimationHandler : BaseUiAnimation
    {
        #region PUBLIC_VARS
        [SerializeField] private List<UIAnimationObject> _animationData;
        #endregion

        #region PRIVATE_VARS
        private Coroutine coroutine;
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public override void ShowAnimation(Action action)
        {
            base.ShowAnimation(action);
            SetPostionBeforAnimation();
            SetObjectsBeforeInAnimation();
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
                SetPostionBeforAnimation();
                SetObjectsBeforeInAnimation();
            }
            if (_animationData.Count > 0)
            {
                coroutine = StartCoroutine(DoShowFx());
            }
        }

        public override void HideAnimation(Action action)
        {
            base.HideAnimation(action);
            SetObjectsBeforeOutAnimation();
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
                SetObjectsBeforeOutAnimation();
            }

            if (_animationData.Count > 0)
            {
                coroutine = StartCoroutine(DoHideFx());
            }
        }

        public void ShowAnimation()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(DoShowFx());
        }

        public void SetInAnimationData()
        {
            for (int i = 0; i < _animationData.Count; i++)
            {
                _animationData[i].SetInAnimationData(0);
            }
        }

        public void HideAnimation()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(DoHideFx());
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void SetObjectsBeforeInAnimation()
        {
            for (int i = 0; i < _animationData.Count; i++)
            {
                _animationData[i].SetInAnimationData(0);
            }
        }

        private void SetPostionBeforAnimation()
        {
            for (int i = 0; i < _animationData.Count; i++)
            {
                _animationData[i].SetPostionBeforAnimation();
            }
        }

        private void SetObjectsBeforeOutAnimation()
        {
            for (int i = 0; i < _animationData.Count; i++)
            {
                _animationData[i].SetOutAnimationData(0);
            }
        }

        #endregion

        #region CO-ROUTINES

        IEnumerator DoShowFx()
        {
            for (int i = 0; i < _animationData.Count; i++)
            {
                _animationData[i].PlayInAnimation();
                yield return null;
            }
            onShowComplete?.Invoke();
        }

        IEnumerator DoHideFx()
        {
            for (int i = 0; i < _animationData.Count; i++)
            {
                _animationData[i].PlayOutAnimation();
                yield return null;
            }
            onHideComplete?.Invoke();
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}
