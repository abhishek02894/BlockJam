using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tag.Block
{
    public static class UIRayUtility
    {
        public static bool IsPointerOverUIElement(string name)
        {
            //Debug.LogError("UIRayUtility " + name + "__"+ IsPointerOverUIElement(GetEventSystemRaycastResults(), name));
            return IsPointerOverUIElement(GetEventSystemRaycastResults(), name);
        }

        public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, string name)
        {
            for (int i = 0; i < eventSystemRaysastResults.Count; i++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[i];
                if (curRaysastResult.gameObject.name == name)
                    return true;
            }
            return false;
        }

        static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystemHelper.EventSystem);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystemHelper.EventSystem.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }
    }
}