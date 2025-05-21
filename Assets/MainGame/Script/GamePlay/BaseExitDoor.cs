using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEditor;

namespace Tag.Block
{
    public class BaseExitDoor : MonoBehaviour
    {
        #region PUBLIC_VARS
        [BlockColorId] public int colorType;
        [SerializeField] private List<BaseCell> allCheckingCells;
        [SerializeField] private List<BaseCell> nearCells;
        [SerializeField] private List<ExitDoorSlot> slotList;
        [SerializeField] private List<GameObject> raycastPoint;
        [SerializeField] private List<GameObject> advancedCells;
        [SerializeField] private Vector3 exitDirection;
        [SerializeField] private Direction raycastDirection;
        [SerializeField] private ParticleSystem exitParticles;
        [SerializeField] private AudioSource exitSound;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private MaskObject maskObject;
        [SerializeField] private RopeBridgeAnimator ropeBridgeAnimator;
        #endregion

        #region PRIVATE_VARS
        private Color originalColor;
        private Vector3 originalScale;
        public Direction RaycastDirection => raycastDirection;

        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            originalScale = transform.localScale;
            ropeBridgeAnimator.ropeBridgePrefab.GetComponent<MeshRenderer>().material = ResourceManager.Instance.GetMaterial(colorType);
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public virtual void CheckBlock()
        {
            CheckForAllCells();
        }

        public virtual List<BaseItem> GetItemInNearCell()
        {
            List<BaseItem> items = new List<BaseItem>();
            for (int i = 0; i < nearCells.Count; i++)
            {
                if (nearCells[i].HasItem && nearCells[i].Item.ColorType == colorType)
                {
                    items.Add(nearCells[i].Item);
                }
            }
            return items;
        }

        // Returns a position for spawning new items after exit
        public Vector3 GetSpawnPosition()
        {
            // Calculate spawn position based on exit direction (opposite to raycast direction)
            Vector3 spawnOffset = GetOppositeDirection(raycastDirection) * 2f; // 2 units away from door

            // Add a small random offset for better distribution if multiple items spawn
            spawnOffset += new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

            return transform.position + spawnOffset;
        }

        // Helper method to get the opposite vector from a direction
        private Vector3 GetOppositeDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return Vector3.down;
                case Direction.Down: return Vector3.up;
                case Direction.Left: return Vector3.right;
                case Direction.Right: return Vector3.left;
                case Direction.Forward: return Vector3.back;
                case Direction.Back: return Vector3.forward;
                default: return Vector3.zero;
            }
        }

        private void CheckForAllCells()
        {
            List<BaseItem> items = GetItemInNearCell();
            foreach (var item in items)
            {
                List<BaseCell> itemCells = item.GetMyCells();
                bool allCellsPresent = true;
                foreach (var cell in itemCells)
                {
                    if (!allCheckingCells.Contains(cell))
                    {
                        allCellsPresent = false;
                        break;
                    }
                }
                if (allCellsPresent)
                {
                    if (maskObject != null)
                        maskObject.AddObjectToMask(item.GetAllMeshRender());
                    if (IsItemExit(item))
                        item.ExitBlock(this);
                    break;
                }
            }
        }

        private bool IsItemExit(BaseItem baseItem)
        {
            List<BaseCell> hitItem = new List<BaseCell>();
            Vector3 direction = Vector3.zero;
            Direction dir = raycastDirection;
            switch (dir)
            {
                case Direction.Up: direction = Vector3.down; break;
                case Direction.Down: direction = Vector3.up; break;
                case Direction.Left: direction = Vector3.right; break;
                case Direction.Right: direction = Vector3.left; break;
                case Direction.Forward: direction = Vector3.back; break;
                case Direction.Back: direction = Vector3.forward; break;
            }

            foreach (var point in baseItem.GetMyCells())
            {
                if (point == null) continue;
                Vector3 startPos = point.transform.position;
                Debug.DrawRay(startPos, direction * 200, Color.yellow, 10f);
                RaycastHit[] hits = Physics.RaycastAll(startPos, direction, 200, LayerMask.GetMask("cell"));
                Debug.DrawRay(startPos, direction * 200, Color.yellow, 10f);
                foreach (var hit in hits)
                {
                    BaseCell cell = hit.collider.GetComponent<BaseCell>();
                    if (cell != null && !hitItem.Contains(cell))
                        hitItem.Add(cell);
                }
            }
            for (int i = 0; i < hitItem.Count; i++)
            {
                if (hitItem[i].HasItem && hitItem[i].Item != baseItem)
                {
                    return false;
                }
            }
            return true;
        }

        public void OnItemExited()
        {
            LevelManager.Instance.CurrentLevel.OnItemExited();
            StopExitDoorAnimation();
        }
        public void StartExitDoorAnimation()
        {
            if (exitParticles != null)
                exitParticles.Play();
            if (exitSound != null)
                exitSound.Play();
            for (int i = 0; i < slotList.Count; i++)
            {
                slotList[i].SetEffectRotation(raycastDirection);
                slotList[i].PlayEffect(true);
            }
            ropeBridgeAnimator.SetCell(nearCells, raycastDirection);
            ropeBridgeAnimator.StartRopeAnimation(ResourceManager.Instance.GetRopeMaterial(colorType));
        }
        public void StopExitDoorAnimation()
        {
            if (exitParticles != null)
                exitParticles.Stop();
            for (int i = 0; i < slotList.Count; i++)
            {
                slotList[i].PlayEffect(false);
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        public void RotateSlot()
        {
            for (int i = 0; i < slotList.Count; i++)
            {
                slotList[i].RotatePiece.transform.Rotate(GetRotationDirection() * (6 * Time.deltaTime * 100f));
            }
        }
        private Vector3 GetRotationDirection()
        {
            switch (raycastDirection)
            {
                //case Direction.Up: return Vector3.right;
                //case Direction.Down: return Vector3.right;
                case Direction.Left: return Vector3.right;
                case Direction.Right: return Vector3.left;
                case Direction.Forward: return Vector3.right;
                case Direction.Back: return Vector3.left;
            }
            return Vector3.zero;
        }
        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
#if UNITY_EDITOR

        public List<GameObject> RaycastPoint => raycastPoint;
        public void SetRayCastDirection(Direction direction)
        {
            this.raycastDirection = direction;
        }

        public void SetDoorValue(List<GameObject> RaycastPoint)
        {
            if (raycastPoint == null)
                raycastPoint = new List<GameObject>();
            raycastPoint.Clear();
            raycastPoint.AddRange(RaycastPoint);
        }

        [Button]
        public void SetCells()
        {
            allCheckingCells.Clear();
            List<BaseCell> hitCells = new List<BaseCell>();
            Vector3 direction = Vector3.zero;
            Direction dir = raycastDirection;
            switch (dir)
            {
                case Direction.Up: direction = Vector3.up; break;
                case Direction.Down: direction = Vector3.down; break;
                case Direction.Left: direction = Vector3.left; break;
                case Direction.Right: direction = Vector3.right; break;
                case Direction.Forward: direction = Vector3.forward; break;
                case Direction.Back: direction = Vector3.back; break;
            }

            foreach (var point in raycastPoint)
            {
                if (point == null) continue;
                Vector3 startPos = point.transform.position;
                Debug.DrawRay(startPos, direction * 200, Color.yellow, 10f);
                RaycastHit[] hits = Physics.RaycastAll(startPos, direction, 200, LayerMask.GetMask("cell"));
                foreach (var hit in hits)
                {
                    BaseCell cell = hit.collider.GetComponent<BaseCell>();
                    if (cell != null && !hitCells.Contains(cell))
                        hitCells.Add(cell);
                }
            }
            allCheckingCells = hitCells;
            GetNearCellsInAllDirections();
        }
        public void GetNearCellsInAllDirections()
        {
            nearCells.Clear();
            List<BaseCell> hitCells = new List<BaseCell>();
            Vector3 direction = Vector3.zero;
            Direction dir = raycastDirection;
            switch (dir)
            {
                case Direction.Up: direction = Vector3.up; break;
                case Direction.Down: direction = Vector3.down; break;
                case Direction.Left: direction = Vector3.left; break;
                case Direction.Right: direction = Vector3.right; break;
                case Direction.Forward: direction = Vector3.forward; break;
                case Direction.Back: direction = Vector3.back; break;

            }

            foreach (var point in raycastPoint)
            {
                if (point == null) continue;
                Vector3 startPos = point.transform.position;
                Debug.DrawRay(startPos, direction * 0.5f, Color.red, 5f);
                RaycastHit[] hits = Physics.RaycastAll(startPos, direction, 0.5f, LayerMask.GetMask("cell"));
                foreach (var hit in hits)
                {
                    BaseCell cell = hit.collider.GetComponent<BaseCell>();
                    if (cell != null && !hitCells.Contains(cell))
                        hitCells.Add(cell);
                }
            }
            nearCells = hitCells;
        }
        public void ClearSlot()
        {
            for (int i = 0; i < slotList.Count; i++)
            {
                if (slotList[i] != null)
                    DestroyImmediate(slotList[i].gameObject);
            }
            slotList.Clear();
        }
        public void AddSlot(ExitDoorSlot exitDoorSlot)
        {
            if (!slotList.Contains(exitDoorSlot))
            {
                slotList.Add(exitDoorSlot);
            }

        }
        public void SetDoorForEdiotr(int colorType)
        {
            this.colorType = colorType;
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/MainGame/Material/DoorMaterials/Block_" + colorType + ".mat");
            for (int i = 0; i < slotList.Count; i++)
            {
                slotList[i].SetMaterial(material);
            }
        }
#endif
    }
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back
    }
}
