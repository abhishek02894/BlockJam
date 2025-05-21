using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    public class CameraSizeHandler : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public float topPadding = 6f;    // Space above the board (for top UI)
        public float bottomPadding = 2f; // Space below the board (for bottom UI)
        public Camera myCam;
        public CameraCacheType changeCameraType;

        [Header("Board Settings")]
        public int rows = 8;
        public int columns = 8;
        public float cellSize = 1f;  // Size of each cell/unit
        public SpriteRenderer requiredGameplayBounds;
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PRIVATE_FUNCTIONS

        [Button]
        public void InitializeSize()
        {
            // Calculate board dimensions based on rows and columns
            float boardWidth = columns * cellSize;
            float boardDepth = rows * cellSize;
            
            // Calculate board center
            Vector3 boardPos = transform.position;
            Vector3 boardCenter = new Vector3(
                boardPos.x + (boardWidth / 2f) - (cellSize / 2f),
                boardPos.y,
                boardPos.z + (boardDepth / 2f) - (cellSize / 2f)
            );
            
            // If using bounds object for visualization, update it
            if (requiredGameplayBounds != null)
            {
                requiredGameplayBounds.size = new Vector3(boardWidth, requiredGameplayBounds.size.y, boardDepth);
                requiredGameplayBounds.transform.position = boardPos;
            }

            // Center the board between paddings
            float shift = (topPadding - bottomPadding) / 2f;
            
            float boardDepthWithPadding = boardDepth + topPadding + bottomPadding;

            Vector3 camPos = myCam.transform.position;
            float distance = Mathf.Abs(camPos.y - boardCenter.y);
            float aspect = myCam.aspect;

            float fovForDepth = 2f * Mathf.Atan((boardDepthWithPadding / 2f) / distance) * Mathf.Rad2Deg;
            float fovForWidth = 2f * Mathf.Atan((boardWidth / 2f) / (distance * aspect)) * Mathf.Rad2Deg;

            float requiredFOV = Mathf.Max(fovForDepth, fovForWidth);

            myCam.fieldOfView = Mathf.Clamp(requiredFOV, 1f, 179f);
        }

        // Use this method to adjust camera when rows/columns change
        [Button]
        public void AdjustCameraForGrid(int newRows, int newColumns)
        {
            rows = newRows;
            columns = newColumns;
            InitializeSize();
        }

        #endregion
    }
}