using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tag.Block
{
    public class EventSystemHelper : Manager<EventSystemHelper>
    {
        #region private variables
        [SerializeField] private EventSystem eventSystem;
        #endregion

        #region properties
        public static EventSystem EventSystem => Instance.eventSystem;
        #endregion

        #region public methods

        public static bool IsPointerOverUIObject()
        {
            if (EventSystem == null)
                return false;
            if (EventSystem.currentSelectedGameObject != null)
                return true;
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem);
            eventDataCurrentPosition.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        public void SetIntractable(bool intractable)
        {
            EventSystem.enabled = intractable;
            //InputManager.StopInteraction = !intractable;
        }

        #endregion
    }
}