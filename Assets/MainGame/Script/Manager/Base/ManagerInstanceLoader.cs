using UnityEngine;

namespace Tag.Block
{
    public class ManagerInstanceLoader : MonoBehaviour
    {
        #region public veriables

        public LoadingType loadingType;
        [HideInInspector] public bool loaded;

        #endregion

        #region unity callback

        public void Awake()
        {
            if (loadingType == LoadingType.auto)
            {
                loaded = true;
            }
        }

        public void OnLoadingStart()
        {
            if (loadingType == LoadingType.Manual)
                loaded = false;
        }

        public void OnLoadingDone()
        {
            if (loadingType == LoadingType.Manual)
                loaded = true;
        }

        #endregion
    }
}