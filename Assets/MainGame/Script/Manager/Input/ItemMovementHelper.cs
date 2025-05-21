using System;
using System.Collections;
using System.Collections.Generic;
using Tag.Block;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    public class ItemMovementHelper : Manager<ItemMovementHelper>
    {
        #region private veribale
        [SerializeField] private List<BaseItemMovement> listOfMovement = new List<BaseItemMovement>();
        [ShowInInspector] private bool isAnyThingPick;
        private bool isUIClickEnable;

        [SerializeField] private ItemMovement cellItemMovement;
        [ShowInInspector, ReadOnly] private BaseItemMovement itemMovement;

        private List<Action> onItemPick = new List<Action>();
        private List<Action> onItemDrag = new List<Action>();
        private List<Action> onItemPut = new List<Action>();
        private BaseItem lastPickItem;

        #endregion

        #region propetices

        public bool IsItemPicked
        {
            get { return (cellItemMovement.PickItem != null); }
        }


        public bool IsItemStatic
        {
            get
            {
                if (itemMovement != null)
                    return itemMovement.IsStaticItem();
                return false;
            }
        }
        public bool IsAnyItemPicked
        {
            get { return isAnyThingPick; }
        }
        public Vector3 ItemCurrentPosition { get; set; }

        public ItemMovement ItemMovement => cellItemMovement;

        #endregion

        #region unity callback

        private void Start()
        {
            InputManager.Instance.AddListenerMouseButtonDown(OnMouseDown);
            InputManager.Instance.AddListenerMouseButtonMove(OnMouseMove);
            InputManager.Instance.AddListenerMouseButtonUp(OnMouseUp);
            InputManager.Instance.AddListenerUIClick(OnUIClick);
            InputManager.Instance.AddListenerUIPointerUp(OnUIPointerUp);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            InputManager.Instance.RemoveListenerMouseButtonDown(OnMouseDown);
            InputManager.Instance.RemoveListenerMouseButtonMove(OnMouseMove);
            InputManager.Instance.RemoveListenerMouseButtonUp(OnMouseUp);
            InputManager.Instance.RemoveListenerUIClick(OnUIClick);
            InputManager.Instance.RemoveListenerUIPointerUp(OnUIPointerUp);
        }

        public void CancelPickItem()
        {
            if (itemMovement != null)
                itemMovement.OnTouchCancel();
            isAnyThingPick = false;
            itemMovement = null;
        }

        #endregion

        #region private methods

        private void OnMouseDown(Vector3 pos)
        {
            for (int i = 0; i < listOfMovement.Count; i++)
            {
                if (!isAnyThingPick && listOfMovement[i].ItemPick(pos))
                {
                    isAnyThingPick = true;
                    itemMovement = listOfMovement[i];
                }
            }
            if (cellItemMovement.PickItem == null || cellItemMovement.PickItem != lastPickItem)
            {
                lastPickItem = cellItemMovement.PickItem;
            }
        }

        private void OnMouseUp(Vector3 pos)
        {
            if (itemMovement != null)
                itemMovement.ItemPut(pos);
            isAnyThingPick = false;
            isUIClickEnable = false;
            itemMovement = null;
        }

        private void OnMouseMove(Vector3 pos)
        {
            if (itemMovement != null)
                ItemCurrentPosition = itemMovement.ItemDrag(pos);
        }

        private void OnUIClick(Vector3 pos)
        {
            if (isUIClickEnable)
            {
                if (itemMovement != null)
                    itemMovement?.ItemDrag(pos);
                return;
            }
            if (itemMovement != null)
                itemMovement.OnTouchCancel();

            for (int i = 0; i < listOfMovement.Count; i++)
            {
                if (listOfMovement[i].IsUIMovement())
                {
                    if (!isAnyThingPick && listOfMovement[i].ItemPick(pos))
                    {
                        isAnyThingPick = true;
                        isUIClickEnable = true;
                        itemMovement = listOfMovement[i];
                    }
                }
            }
        }

        private void OnUIPointerUp(Vector3 pos)
        {
            if (itemMovement != null)
                itemMovement.ItemPut(pos);
            isAnyThingPick = false;
            isUIClickEnable = false;
            itemMovement = null;
        }

        #endregion

        #region PUBLIC FUNCTIONS
        public void RegisterOnItemPick(Action action)
        {
            if (!onItemPick.Contains(action))
                onItemPick.Add(action);
        }
        public void DeRegisterOnItemPick(Action action)
        {
            if (onItemPick.Contains(action))
                onItemPick.Remove(action);
        }
        public void OnItemPick()
        {
            for (int i = 0; i < onItemPick.Count; i++)
            {
                if (onItemPick[i] != null)
                    onItemPick[i].Invoke();
            }
            GameplayManager.Instance.StartTimer();
        }
        public void RegisterOnItemPut(Action action)
        {

            if (!onItemPut.Contains(action))
                onItemPut.Add(action);
        }
        public void DeRegisterOnItemPut(Action action)
        {
            if (onItemPut.Contains(action))
                onItemPut.Remove(action);
        }
        public void OnItemPut()
        {
            for (int i = 0; i < onItemPut.Count; i++)
            {
                if (onItemPut[i] != null)
                    onItemPut[i].Invoke();
            }
        }
        public void RegisterOnItemDrag(Action action)
        {
            if (!onItemDrag.Contains(action))
                onItemDrag.Add(action);
        }
        public void DeRegisterOnItemDrag(Action action)
        {
            if (onItemDrag.Contains(action))
                onItemDrag.Remove(action);
        }
        public void OnItemDrag()
        {
            for (int i = 0; i < onItemPut.Count; i++)
            {
                if (onItemDrag[i] != null)
                    onItemDrag[i].Invoke();
            }
        }
        #endregion
    }
}
