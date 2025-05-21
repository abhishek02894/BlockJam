using System.Collections;
using System.Collections.Generic;
using System.Net;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    public class ThreadCollector : MonoBehaviour
    {
        #region PUBLIC_VARS
        [SerializeField] private ThreadRing ringPrefab;
        [SerializeField] private Transform ringSlotParent;
        [SerializeField] private Transform endPoint;
        [SerializeField] private float horizontalSpacing = 1.0f;
        [ReadOnly, SerializeField] private List<ThreadRing> threadRings = new List<ThreadRing>();
        #endregion

        #region PRIVATE_VARS
        private int currentRingNum = 0;
        public Transform EndPoint => endPoint;
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS


        public void ActiveRing(int number, Material material)
        {
            int endRingNum = currentRingNum + number;
            for (int i = currentRingNum; i < endRingNum && i < threadRings.Count; i++)
            {
                threadRings[currentRingNum].SetRing(material);
                endPoint.transform.position = threadRings[currentRingNum].transform.position;
                threadRings[currentRingNum].gameObject.SetActive(true);
                currentRingNum++;
            }
        }

        //[Button]
        //public void CreateHorizontalRings(int count)
        //{
        //    // Clear existing rings if any
        //    foreach (var ring in threadRings)
        //    {
        //        if (ring != null)
        //        {
        //            Destroy(ring.gameObject);
        //        }
        //    }
        //    threadRings.Clear();

        //    // Create new rings
        //    for (int i = 0; i < count; i++)
        //    {
        //        // Instantiate ring
        //        ThreadRing newRing = Instantiate(ringPrefab, ringSlotParent);
        //        newRing.transform.localPosition = new Vector3(0, i * horizontalSpacing, 0);

        //        // Add to list
        //        threadRings.Add(newRing);
        //    }
        //}
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
