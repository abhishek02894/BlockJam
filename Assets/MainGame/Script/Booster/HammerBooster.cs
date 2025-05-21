using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Tag.Block
{
    public class HammerBooster : BaseBooster
    {
        #region PUBLIC_VARS

        public LayerMask itemLayerMask;

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private Animator hammerFx;
        [SerializeField] private string hammerAnimationName;
        private RaycastHit hit;
        private bool inProcess = false;
        public static event Action OnHammerUseEvent;

        #endregion

        #region UNITY_CALLBACKS

        private void Update()
        {
            if (!isActive)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (GetRayHit(Input.mousePosition, out hit, itemLayerMask))
                {
                    GameplayManager.Instance.StartTimer();
                    BaseItem baseItem = hit.collider.GetComponent<BaseItem>();
                    if (baseItem != null)
                    {
                        if (baseItem.CanUseBooster())
                        {
                            OnHammerUse(baseItem);
                        }
                    }
                }
            }
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void OnActive(Action onUse)
        {
            ActiveConfirmationView();
            InputManager.StopInteraction = true;
            base.OnActive(onUse);
        }

        public override void OnUse()
        {
            DeActiveConfirmationView();
            base.OnUse();
            GameplayManager.Instance.SaveAllDataOfLevel();
            InputManager.StopInteraction = false;
        }

        public override void OnUnUse()
        {
            base.OnUnUse();
            DeActiveConfirmationView();
            InputManager.StopInteraction = false;
        }

        public bool GetRayHit(Vector3 pos, out RaycastHit hit, LayerMask layerMask)
        {
            Ray ray = InputManager.EventCamera.ScreenPointToRay(pos);
            Debug.DrawRay(ray.origin, ray.direction * 15, Color.magenta, 0.2f);
            return Physics.Raycast(ray, out hit, 100, layerMask);
        }
        [Button]
        public void OnHammerUse(BaseItem baseItem)
        {
            Debug.LogError("OnHammerUse" + baseItem.gameObject.name + "_" + baseItem.ColorType);
            OnHammerUseEvent?.Invoke();
            inProcess = true;
            if (hammerFx != null)
            {
                hammerFx.transform.SetParent(baseItem.transform);
                hammerFx.gameObject.SetActive(true);
                hammerFx.Play(hammerAnimationName);
                CoroutineRunner.instance.Wait(hammerFx.GetAnimationLength(hammerAnimationName) - 0.3f, () =>
                {
                    //Vibrator.Vibrate(Vibrator.hugeIntensity);
                    hammerFx.transform.SetParent(transform);
                    hammerFx.gameObject.SetActive(false);
                    baseItem.OnHammerBoosterUse();
                    OnUse();
                    inProcess = false;
                });
            }
            else
            {
                baseItem.OnHammerBoosterUse();
                OnUse();
                inProcess = false;
            }
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
}
