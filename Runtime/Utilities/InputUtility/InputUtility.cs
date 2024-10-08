﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.Utilities
{
    public static class InputUtility
    {
        #region Class Methods

        public static bool IsPointerOverUIObject()
        {
            if (EventSystem.current == null)
                return true;

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);
            if (results.Count == 1 && results[0].gameObject.GetComponent<IIgnorePointer>() != null)
                return false;
            else
                return results.Count > 0;
        }

        public static List<RaycastResult> GetCurrentObjectsInPointerPosition()
        {
            if (EventSystem.current == null)
                return null;

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);
            if (results.Count == 1 && results[0].gameObject.GetComponent<IIgnorePointer>() != null)
                return null;
            else
                return results;
        }

        #endregion Class Methods
    }
}