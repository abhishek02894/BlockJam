using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tag.Block
{
    public class BaseItem : SerializedMonoBehaviour
    {
        #region PUBLIC_VARS
        [BlockId] public int blocktype;
        [SerializeField] private LayerMask itemLayerMask;
        [SerializeField] private List<Vector3> shapePositions = new List<Vector3>();
        [SerializeField] private List<ItemSlot> itemSlot = new List<ItemSlot>();
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshRenderer shadowRenderer;
        [SerializeField] Dictionary<int, Transform> elementParent = new Dictionary<int, Transform>();
        [SerializeField] public Transform verticalFixDirectionParent;
        [SerializeField] public Transform horizontalFixDirectionParent;
        [SerializeField] private float spacing = 1f;

        [ShowInInspector, ReadOnly] private List<BaseCell> cells = new List<BaseCell>();
        [ShowInInspector, ReadOnly] private int pickedShapeIndex = 0;
        private Vector3 lastValidPosition;
        private List<BaseElement> baseElements = new List<BaseElement>();
        [ShowInInspector, ReadOnly] private ItemData itemData;
        #endregion

        public int ColorType => itemData.colorType;
        public float Spacing => spacing;
        public int BlockType => itemData.blockType;

        public int ItemSlotCount => itemSlot.Count;
        public ItemData ItemData => itemData;

        // Modified ShapePositions to be a property with a getter
        public List<Vector3> ShapePositions 
        {
            get 
            {
                EnsureShapePositionsInitialized();
                return shapePositions;
            }
        }

        #region PRIVATE_VARS
        #endregion

        #region UNITY_CALLBACKS
        public void Init(ItemData itemData)
        {
            this.itemData = itemData;
            blocktype = itemData.blockType;
            meshRenderer.material = ResourceManager.Instance.GetMaterial(itemData.colorType);
            gameObject.SetActive(true);
            // Only use cellId if it's valid
            if (itemData.cellId >= 0)
            {
                SetPosition(LevelManager.Instance.CurrentLevel.Board.GetCellById(itemData.cellId).transform.position);
            }
            InitElement();
            UpdateOccupiedCells();
        }

        private void InitElement()
        {
            ClearBaseElement();
            for (int i = 0; i < itemData.elements.Count; i++)
            {
                itemData.elements[i].Init(this);
                BaseElement baseElement = ResourceManager.Instance.GetElementPrefab(itemData.elements[i].elementType, elementParent[itemData.elements[i].elementType]);
                if (baseElement != null)
                {
                    baseElement.transform.localPosition = Vector3.zero;
                    baseElement.gameObject.SetActive(true);
                    baseElement.Init(itemData.elements[i], this);
                    baseElements.Add(baseElement);
                }
            }
        }
        private void ClearBaseElement()
        {
            for (int i = 0; i < baseElements.Count; i++)
            {
                if (baseElements[i] != null)
                    Destroy(baseElements[i].gameObject);
            }
            baseElements.Clear();
        }
        // Special initialization for extra items that appear at specific positions
        public void InitAtPosition(ItemData itemData, Vector3 spawnPosition)
        {
            this.itemData = itemData;
            blocktype = itemData.blockType;
            meshRenderer.material = ResourceManager.Instance.GetMaterial(itemData.colorType);
            gameObject.SetActive(true);
            // Use the provided position instead of looking up by cellId
            SetPosition(spawnPosition);
            UpdateOccupiedCells();
        }

        public void ResetLevel()
        {
            gameObject.SetActive(true);
            SetPosition(LevelManager.Instance.CurrentLevel.Board.GetCellById(itemData.cellId).transform.position);
            UpdateOccupiedCells();
        }
        public List<MeshRenderer> GetAllMeshRender()
        {
            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
            meshRenderers.Add(meshRenderer);
            meshRenderers.Add(shadowRenderer);
            for (int i = 0; i < baseElements.Count; i++)
            {
                meshRenderers.AddRange(baseElements[i].GetMeshRender());
            }
            return meshRenderers;
        }
        public void OnHammerBoosterUse()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Item = null;
            }
            cells.Clear();
            gameObject.SetActive(false);
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public virtual bool CanMoveItem()
        {
            return true;
        }
        public virtual void OnItemPick()
        {
            lastValidPosition = transform.position;
        }
        public virtual void OnItemPut(BaseCell putCell)
        {
            // This method is called when the item is successfully placed.
            // Derived classes can override this to perform specific actions.
            // Do not call ResetItemPosition() here as it would undo the placement.
        }
        public virtual void OnItemDrag(Vector3 position)
        {
            // Set the block's position directly to the pointer's world position
            transform.position = new Vector3(position.x, transform.position.y, position.z);
        }
        public virtual void OnHoldItem()
        {

        }

        public virtual bool CanUseBooster()
        {
            return true;
        }
        public virtual bool CanCallHold()
        {
            return true;
        }
        public virtual bool IsMyCell(BaseCell cell)
        {
            return true;
        }
        public List<BaseCell> GetMyCells()
        {
            return cells;
        }
        public void SetLastValidPosition(Vector3 position)
        {
            lastValidPosition = new Vector3(position.x, transform.position.y, position.z);
        }
        public void SetPosition(Vector3 vector3)
        {
            transform.position = vector3;
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }
        public void ResetItemPosition()
        {
            transform.position = lastValidPosition;
        }
        public void SetRenderer(Material material)
        {
            meshRenderer.material = material;
        }
        public virtual Vector3 GetPickedShapeOffset()
        {
            // EnsureShapePositionsInitialized(); // Removed direct call, ShapePositions property getter will handle this.
            
            // Accessing this.ShapePositions will trigger EnsureShapePositionsInitialized() via the getter
            List<Vector3> currentShapePositions = this.ShapePositions; 

            // Existing logging from previous step (keep it)
            Debug.Log($"[BaseItem] GetPickedShapeOffset for {this.gameObject.name}: pickedShapeIndex = {pickedShapeIndex}, shapePositions.Count = {(currentShapePositions != null ? currentShapePositions.Count : 0)}");

            if (currentShapePositions == null || currentShapePositions.Count == 0 || pickedShapeIndex < 0 || pickedShapeIndex >= currentShapePositions.Count)
            {
                // Existing logging (keep it)
                Debug.Log("[BaseItem] Returning Vector3.zero offset (list empty or index out of bounds).");
                return Vector3.zero;
            }
            
            // Existing logging (keep it)
            Debug.Log($"[BaseItem] Returning offset: {currentShapePositions[pickedShapeIndex]} (raw), scaled by spacing in ItemMovement.");
            return currentShapePositions[pickedShapeIndex];
        }

        public void SetPickedShapeIndex(int index)
        {
            pickedShapeIndex = Mathf.Clamp(index, 0, shapePositions.Count - 1);
        }
        public bool IsContainElement(int elementType)
        {
            return itemData.elements.Find(x => x.elementType == elementType) != null;
        }
        public TBaseElement GetElement<TBaseElement>(int elementType) where TBaseElement : BaseElementData
        {
            if (itemData.elements.Find(x => x.elementType == elementType) != null)
                return (TBaseElement)itemData.elements.Find(x => x.elementType == elementType);
            return null;
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void EnsureShapePositionsInitialized()
        {
            if ((shapePositions == null || shapePositions.Count == 0) && itemSlot != null && itemSlot.Count > 0)
            {
                Debug.Log($"[BaseItem] {gameObject.name}: Initializing shapePositions from itemSlot children because it was empty.");
                shapePositions = new List<Vector3>();
                foreach (ItemSlot childSlot in itemSlot)
                {
                    if (childSlot != null)
                    {
                        // Assuming 'spacing' is not zero to avoid division by zero.
                        // If spacing can be zero, add a check. For typical use, it should be > 0.
                        if (spacing == 0) {
                            Debug.LogWarning($"[BaseItem] {gameObject.name}: Spacing is 0, cannot derive shapePositions from itemSlot localPositions accurately. Assuming localPosition is pre-scaled or shapePosition is (0,0,0).");
                            shapePositions.Add(childSlot.transform.localPosition); // Or Vector3.zero or skip
                        } else {
                            Vector3 derivedShapePosition = childSlot.transform.localPosition / spacing;
                            shapePositions.Add(derivedShapePosition);
                        }
                    }
                }
                // After dynamically populating shapePositions, it's possible pickedShapeIndex might be invalid
                // if it was set based on a previously empty or different list.
                // However, SetPickedShapeIndex in ItemPick will run after this, so it should be okay.
                // For safety, ensure pickedShapeIndex is valid if we modify shapePositions here.
                // This might be overly cautious as ItemPick recalculates it.
                 if (pickedShapeIndex >= shapePositions.Count && shapePositions.Count > 0) { // Ensure shapePositions is not empty before accessing index 0
                     pickedShapeIndex = 0;
                 } else if (shapePositions.Count == 0) { // If still no shape positions, index must be 0 or invalid.
                     pickedShapeIndex = 0; // Or -1 if that's preferred for an invalid state
                 }
            }
        }
        public void UpdateOccupiedCells()
        {
            Board board = LevelManager.Instance.CurrentLevel.Board;
            foreach (var cell in cells)
            {
                if (cell.Item == this)
                {
                    cell.Item = null;
                }
            }
            cells.Clear();
            foreach (var pos in itemSlot)
            {
                Vector3 worldPos = pos.transform.position;
                BaseCell targetCell = board.GetCellAtWorldPos(worldPos);
                if (targetCell != null)
                {
                    cells.Add(targetCell);
                    targetCell.Item = this;
                }
            }
        }
        private void ExitItem()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Item = null;
            }
            cells.Clear();
            gameObject.SetActive(false);
        }
        private Vector3 GetExitDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector3.back;
                case Direction.Down: return Vector3.up;
                case Direction.Left: return Vector3.right;
                case Direction.Right: return Vector3.left;
                case Direction.Forward: return Vector3.back;
                case Direction.Back: return Vector3.forward;
                default: return Vector3.zero;
            }
        }

        public void ExitBlock(BaseExitDoor baseExitDoor)
        {
            foreach (var cell in cells)
            {
                if (cell.Item == this)
                {
                    cell.Item = null;
                }
            }
            cells.Clear();
            Vector3 originalPosition = transform.position;
            Vibrator.Vibrate(Vibrator.averageIntensity);
            baseExitDoor.StartExitDoorAnimation();
            PlayRopeCollecterAnimation(baseExitDoor);
            PlayAnimation(baseExitDoor, () =>
            {
                for (int i = 0; i < baseElements.Count; i++)
                {
                    baseElements[i].ElementData.ExitItem(originalPosition);
                }
                ExitItem();
                baseExitDoor.OnItemExited();
            });
        }



        private void PlayAnimation(BaseExitDoor baseExitDoor, Action finishAction)
        {
            StartCoroutine(PlayAnimationCoroutine(baseExitDoor, finishAction));
        }

        private IEnumerator PlayAnimationCoroutine(BaseExitDoor baseExitDoor, Action finishAction)
        {
            float distance = Vector3.Distance(transform.position, baseExitDoor.transform.position) * 2.5f;
            Vector3 exitDirection = GetExitDirection(baseExitDoor.RaycastDirection) * distance;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = transform.position + exitDirection;

            float duration = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                //for (int i = 0; i < itemSlot.Count; i++)
                //{
                //    //float dis = Vector3.Distance(itemSlot[i].transform.position, baseExitDoor.transform.position);
                //    float dis = Mathf.Abs(Vector3.Dot(itemSlot[i].transform.position - baseExitDoor.transform.position, exitDirection));
                //    // Only show debug error if the ItemSlot is in front of the exit door
                //    Vector3 toItemSlot = itemSlot[i].transform.position - baseExitDoor.transform.position;
                //    float dotProduct = Vector3.Dot(toItemSlot, exitDirection.normalized);
                //    if (dis <= (itemSlot[i].transform.localScale.x) && dotProduct > 0)
                //        Debug.LogError(itemSlot[i].gameObject.name);
                //}
                baseExitDoor.RotateSlot();
                yield return null;
            }
            transform.position = targetPosition;
            finishAction?.Invoke();
        }

        private void PlayRopeCollecterAnimation(BaseExitDoor baseExitDoor)
        {
            StartCoroutine(PlayRopeCollecterAnimationCoroutine(baseExitDoor));
        }
        private IEnumerator PlayRopeCollecterAnimationCoroutine(BaseExitDoor baseExitDoor)
        {
            yield return new WaitForSeconds(0.2f);

            float duration = 0.5f;
            float elapsedTime = 0f;
            int ringColorCount = (ItemSlotCount * 100) / LevelManager.Instance.CurrentLevel.totalBlockCell;
            //Debug.LogError("LevelManager.Instance.CurrentLevel.totalBlockCell " + LevelManager.Instance.CurrentLevel.totalBlockCell);
            //Debug.LogError("ItemSlotCount " + ItemSlotCount);
            Debug.LogError("ringColorCount__ " + ringColorCount);
            LevelManager.Instance.CurrentLevel.ThreadCollector.ActiveRing(ringColorCount, ResourceManager.Instance.GetRopeMaterial(ColorType));
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
#if UNITY_EDITOR
        [Space(50)]
        public ItemSlot itemBlock;

        public List<ItemSlot> ItemSlot => itemSlot;

        [Button]
        public void BuildVisualBlock()
        {
            // Optional: Clear old visuals
            //for (int i = transform.childCount - 1; i >= 0; i--)
            //{
            //    DestroyImmediate(transform.GetChild(i).gameObject);
            //}
            itemSlot.Clear();
            // Instantiate new visuals
            foreach (var pos in shapePositions)
            {
                ItemSlot go = Instantiate(itemBlock, transform);
                go.transform.localPosition = new Vector3(pos.x * spacing, 0, pos.z * spacing); // 2D positioning with spacing
                itemSlot.Add(go);
            }
        }
        public void AddCell(BaseCell baseCell)
        {
            if (!cells.Contains(baseCell))
                cells.Add(baseCell);
        }
        public void ClearCell()
        {
            cells.Clear();
        }

        public void SetItemForEdiotr(ItemData data)
        {
            this.itemData = data;
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/MainGame/Material/BlockMaterials/Block_" + itemData.colorType + ".mat");
            meshRenderer.material = material;
            for (int i = 0; i < itemData.elements.Count; i++)
            {
                itemData.elements[i].Init(this);
                BaseElement baseElement = AssetDatabase.LoadAssetAtPath<BaseElement>("Assets/MainGame/Prefabs/Elements/Element_" + itemData.elements[i].elementType + ".prefab");
                if (elementParent.ContainsKey(itemData.elements[i].elementType))
                {
                    Instantiate(baseElement, elementParent[itemData.elements[i].elementType]);
                    if (baseElement != null)
                    {
                        baseElement.transform.localPosition = Vector3.zero;
                        baseElement.gameObject.SetActive(true);
                        baseElement.InitForEditor(itemData.elements[i], this);
                    }
                }
            }
        }
#endif
    }
}
