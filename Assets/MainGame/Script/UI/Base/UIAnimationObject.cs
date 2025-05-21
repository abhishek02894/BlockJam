using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Tag.Block
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIAnimationObject : MonoBehaviour
    {
        #region PUBLIC_VARS
        [SerializeField] private Transform obj;
        [Space(1)] public bool isInAnimation;
        [ShowIf("isInAnimation")] public ObjectAnimationData inAnimationData;
        [Space(1)] public bool isOutAnimation;
        [ShowIf("isOutAnimation")] public ObjectAnimationData outAnimationData;
        [Space(1)] public CanvasGroup cg;
        [SerializeField] private bool getValueRuntime;
        #endregion

        #region PRIVATE_VARS
        private Coroutine coroutine;
        #endregion

        #region UNITY_CALLBACKS

        private void Awake()
        {
            if (getValueRuntime)
            {
                inAnimationData.endPos = obj.localPosition;
                outAnimationData.startPos = obj.localPosition;
            }
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public void SetPostionBeforAnimation()
        {
            //if (getValueRuntime)
            //{
            //    inAnimationData.endPos = obj.localPosition;
            //    outAnimationData.startPos = obj.localPosition;
            //}
        }

        [Button]
        public void GetData()
        {
            obj = gameObject.GetComponent<RectTransform>();
            cg = gameObject.GetComponent<CanvasGroup>();
        }
        [ContextMenu("SetCurrentPositionAsInAnimationStartPosition")]
        public void SetCurrentPositionAsInAnimationStartPosition()
        {
            if (isInAnimation && inAnimationData.isPosition)
                inAnimationData.startPos = obj.localPosition;
        }
        [ContextMenu("SetCurrentPositionAsInAnimationEndPosition")]
        public void SetCurrentPositionAsInAnimationEndPosition()
        {
            if (isInAnimation && inAnimationData.isPosition)
                inAnimationData.endPos = obj.localPosition;
        }
        [ContextMenu("SetCurrentPositionAsOutAnimationStartPosition")]
        public void SetCurrentPositionAsOutAnimationStartPosition()
        {
            if (isOutAnimation && outAnimationData.isPosition)
                outAnimationData.startPos = obj.localPosition;
        }
        [ContextMenu("SetCurrentPositionAsOutAnimationEndPosition")]
        public void SetCurrentPositionAsOutAnimationEndPosition()
        {
            if (isOutAnimation && outAnimationData.isPosition)
                outAnimationData.endPos = obj.localPosition;
        }

        [ContextMenu("SetCurrentRotationAsInAnimationStartRotation")]
        public void SetCurrentRotationAsInAnimationStartRotation()
        {
            if (isInAnimation && inAnimationData.isRotation)
                inAnimationData.startRotation = obj.rotation.eulerAngles;
        }
        [ContextMenu("SetCurrentRotationAsInAnimationEndRotation")]
        public void SetCurrentRotationAsInAnimationEndRotation()
        {
            if (isInAnimation && inAnimationData.isRotation)
                inAnimationData.endRotation = obj.rotation.eulerAngles;
        }
        [ContextMenu("SetCurrentRotationAsOutAnimationStartRotation")]
        public void SetCurrentRotationAsOutAnimationStartRotation()
        {
            if (isOutAnimation && outAnimationData.isRotation)
                outAnimationData.startRotation = obj.rotation.eulerAngles;
        }
        [ContextMenu("SetCurrentRotationAsOutAnimationEndRotation")]
        public void SetCurrentRotationAsOutAnimationEndRotation()
        {
            if (isOutAnimation && outAnimationData.isRotation)
                outAnimationData.endRotation = obj.rotation.eulerAngles;
        }

        [ContextMenu("SetCurrentScaleAsInAnimationStartScale")]
        public void SetCurrentScaleAsInAnimationStartScale()
        {
            if (isInAnimation && inAnimationData.isScale)
                inAnimationData.startScale = obj.localScale;
        }
        [ContextMenu("SetCurrentScaleAsInAnimationEndScale")]
        public void SetCurrentScaleAsInAnimationEndScale()
        {
            if (isInAnimation && inAnimationData.isScale)
                inAnimationData.endScale = obj.localScale;
        }
        [ContextMenu("SetCurrentScaleAsOutAnimationStartScale")]
        public void SetCurrentScaleAsOutAnimationStartScale()
        {
            if (isOutAnimation && outAnimationData.isScale)
                outAnimationData.startScale = obj.localScale;
        }
        [ContextMenu("SetCurrentScaleAsOutAnimationEndScale")]
        public void SetCurrentScaleAsOutAnimationEndScale()
        {
            if (isOutAnimation && outAnimationData.isScale)
                outAnimationData.endScale = obj.localScale;
        }

        [ContextMenu("SetCurrentAlphaAsInAnimationStartAlpha")]
        public void SetCurrentAlphaAsInAnimationStartAlpha()
        {
            if (isInAnimation && inAnimationData.isAlpha)
                inAnimationData.startAlpha = cg.alpha;
        }
        [ContextMenu("SetCurrentAlphaAsInAnimationEndAlpha")]
        public void SetCurrentAlphaAsInAnimationEndAlpha()
        {
            if (isInAnimation && inAnimationData.isAlpha)
                inAnimationData.endAlpha = cg.alpha;
        }
        [ContextMenu("SetCurrentAlphaAsOutAnimationStartAlpha")]
        public void SetCurrentAlphaAsOutAnimationStartAlpha()
        {
            if (isOutAnimation && outAnimationData.isAlpha)
                outAnimationData.startAlpha = cg.alpha;
        }
        [ContextMenu("SetCurrentAlphaAsOutAnimationEndAlpha")]
        public void SetCurrentAlphaAsOutAnimationEndAlpha()
        {
            if (isOutAnimation && outAnimationData.isAlpha)
                outAnimationData.endAlpha = cg.alpha;
        }

        [Button]
        public void PlayInAnimation()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                SetInAnimationData(0);
                if (isInAnimation && gameObject.activeInHierarchy)
                    coroutine = StartCoroutine(DoAnimation(inAnimationData));
            }
            else if (isInAnimation && gameObject.activeInHierarchy)
                coroutine = StartCoroutine(DoAnimation(inAnimationData));
        }

        [Button]
        public void PlayOutAnimation()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                SetInAnimationData(0);
                if (isOutAnimation && gameObject.activeInHierarchy)
                    coroutine = StartCoroutine(DoAnimation(outAnimationData));
            }
            else if (isOutAnimation && gameObject.activeInHierarchy)
                coroutine = StartCoroutine(DoAnimation(outAnimationData));
        }
        public void SetInAnimationData(float value)
        {
            if (isInAnimation)
            {
                if (inAnimationData.isPosition)
                    obj.localPosition = Vector3.LerpUnclamped(inAnimationData.startPos, inAnimationData.endPos, inAnimationData.positionCurve.Evaluate(value));
                if (inAnimationData.isRotation)
                    obj.localEulerAngles = Vector3.LerpUnclamped(inAnimationData.startRotation, inAnimationData.endRotation, inAnimationData.rotationCurve.Evaluate(value));
                if (inAnimationData.isScale)
                    obj.transform.localScale = Vector3.LerpUnclamped(inAnimationData.startScale, inAnimationData.endScale, inAnimationData.scaleCurve.Evaluate(value));
                if (inAnimationData.isAlpha)
                    cg.alpha = Mathf.LerpUnclamped(inAnimationData.startAlpha, inAnimationData.endAlpha, inAnimationData.alphaCurve.Evaluate(value));
            }
        }
        public void SetOutAnimationData(float value)
        {
            if (isOutAnimation)
            {
                if (outAnimationData.isPosition)
                    obj.localPosition = Vector3.LerpUnclamped(outAnimationData.startPos, outAnimationData.endPos, outAnimationData.positionCurve.Evaluate(value));
                if (outAnimationData.isRotation)
                    obj.localEulerAngles = Vector3.LerpUnclamped(outAnimationData.startRotation, outAnimationData.endRotation, outAnimationData.rotationCurve.Evaluate(value));
                if (outAnimationData.isScale)
                    obj.transform.localScale = Vector3.LerpUnclamped(outAnimationData.startScale, outAnimationData.endScale, outAnimationData.scaleCurve.Evaluate(value));
                if (outAnimationData.isAlpha)
                    cg.alpha = Mathf.LerpUnclamped(outAnimationData.startAlpha, outAnimationData.endAlpha, outAnimationData.alphaCurve.Evaluate(value));
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES

        private IEnumerator DoAnimation(ObjectAnimationData data)
        {
            yield return new WaitForSeconds(data.startAnimationDelay);
            float i = 0;
            float rate = 1 / data.animationTime;
            while (i < 1)
            {
                i += Time.deltaTime * rate;
                if (data.isPosition)
                    obj.localPosition = Vector3.LerpUnclamped(data.startPos, data.endPos, data.positionCurve.Evaluate(i));
                if (data.isRotation)
                    obj.localEulerAngles = Vector3.LerpUnclamped(data.startRotation, data.endRotation, data.rotationCurve.Evaluate(i));
                if (data.isScale)
                    obj.localScale = Vector3.LerpUnclamped(data.startScale, data.endScale, data.scaleCurve.Evaluate(i));
                if (data.isAlpha)
                    cg.alpha = Mathf.LerpUnclamped(data.startAlpha, data.endAlpha, data.alphaCurve.Evaluate(i));
                yield return null;
            }
            coroutine = null;
        }

        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
    [System.Serializable]
    public class ObjectAnimationData
    {
        public float animationTime = 0;
        public float startAnimationDelay = 0;
        public bool isPosition;
        [ShowIf("isPosition")] public Vector3 startPos = Vector3.zero;
        [ShowIf("isPosition")] public Vector3 endPos = Vector3.zero;
        [ShowIf("isPosition")] public AnimationCurve positionCurve;

        public bool isRotation;
        [ShowIf("isRotation")] public Vector3 startRotation = Vector3.zero;
        [ShowIf("isRotation")] public Vector3 endRotation = Vector3.zero;
        [ShowIf("isRotation")] public AnimationCurve rotationCurve;

        public bool isScale;
        [ShowIf("isScale")] public Vector3 startScale = Vector3.zero;
        [ShowIf("isScale")] public Vector3 endScale = Vector3.zero;
        [ShowIf("isScale")] public AnimationCurve scaleCurve;

        public bool isAlpha;
        [ShowIf("isAlpha")] public float startAlpha = 0;
        [ShowIf("isAlpha")] public float endAlpha = 0;
        [ShowIf("isAlpha")] public AnimationCurve alphaCurve;
    }
}
