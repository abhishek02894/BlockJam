using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tag.Block;
using System; // Required for List

public class RopeBridgeAnimator : MonoBehaviour
{
    public GameObject ropeBridgePrefab;
    public Transform ropeBridgeParent;
    public Vector3 ropeBridgeStartPos = Vector3.zero;
    public float endMoveDuration = 2f;
    public float waitAfterEndMove = 1f;
    public float startToEndDuration = 2f;
    public bool autoStart = false;
    private List<Vector3> cellTransforms = new List<Vector3>();
    public float cellTransitionDuration = 0.5f;
    public float oscillationOffsetDistance = 1f; // Offset distance along the cell line
    public Vector3 defaultOffsetDirection = Vector3.right; // Used if only one cell or cells are at the same position

    private Transform endTargetPosition;
    private RopeBridge currentRopeBridge;
    private Vector3 baseStartPos;
    private bool isStartAnimating = false;
    private bool animationStarted = false;

    IEnumerator RopeAnimationSequence()
    {
        isStartAnimating = true;
        Coroutine oscillationCoroutine = null;

        if (currentRopeBridge != null)
        {
            oscillationCoroutine = StartCoroutine(OscillateThroughCellsCoroutine());
        }

        // Wait for oscillation to potentially start and move to its initial position
        // This yield helps ensure OscillateThroughCellsCoroutine's initial move completes
        // before EndPoint movement starts, if that's desired.
        // Adjust or remove if EndPoint should move concurrently with initial oscillation setup.
        yield return null;


        if (currentRopeBridge != null)
        {
            float timer = 0f;
            Vector3 currentEndPointStartPos = currentRopeBridge.EndPoint.position;
            while (timer < endMoveDuration)
            {
                if (currentRopeBridge == null) yield break;
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / endMoveDuration);
                currentRopeBridge.EndPoint.position = Vector3.Lerp(currentEndPointStartPos, endTargetPosition.position, t);
                yield return null;
            }
            if (currentRopeBridge != null) currentRopeBridge.EndPoint.position = endTargetPosition.position;
        }

        yield return new WaitForSeconds(waitAfterEndMove);

        isStartAnimating = false; // Signal oscillation to stop

        if (oscillationCoroutine != null)
        {
            // Optionally wait for the oscillation coroutine to fully stop if it has cleanup
            // yield return oscillationCoroutine; 
        }


        if (currentRopeBridge != null)
        {
            Vector3 startFrom = currentRopeBridge.StartPoint.position;
            Vector3 startTo = currentRopeBridge.EndPoint.position;
            float timer = 0f;
            while (timer < startToEndDuration)
            {
                if (currentRopeBridge == null) yield break;
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / startToEndDuration);
                currentRopeBridge.StartPoint.position = Vector3.Lerp(startFrom, startTo, t);
                yield return null;
            }
            if (currentRopeBridge != null) currentRopeBridge.StartPoint.position = startTo;
        }

        yield return new WaitForSeconds(0.5f);
        if (currentRopeBridge != null)
        {
            Destroy(currentRopeBridge.gameObject);
        }
        animationStarted = false;
    }

    IEnumerator OscillateThroughCellsCoroutine()
    {
        if (currentRopeBridge == null) yield break;

        List<Vector3> validTargetsCache = new List<Vector3>();
        int currentTargetIndexInCache = 0;
        int multiCellDirection = 1;
        bool singleCellTargetIsPlusOffset = true;

        Vector3 currentCalculatedOffsetDirection = defaultOffsetDirection.normalized;
        if (defaultOffsetDirection == Vector3.zero) currentCalculatedOffsetDirection = Vector3.right; // Ensure it's not zero

        // Initial move:
        if (isStartAnimating)
        {
            if (cellTransforms != null)
            {
                foreach (var t in cellTransforms) if (t != null) validTargetsCache.Add(t);
            }

            if (validTargetsCache.Count > 0)
            {
                if (validTargetsCache.Count > 1)
                {
                    Vector3 dir = (validTargetsCache[validTargetsCache.Count - 1] - validTargetsCache[0]).normalized;
                    if (dir != Vector3.zero) currentCalculatedOffsetDirection = dir;
                }
                // else for 1 cell, currentCalculatedOffsetDirection remains default

                Vector3 initialRopePos = currentRopeBridge.StartPoint.position;
                Vector3 firstCellTransform = validTargetsCache[0];
                Vector3 firstCellBasePos = firstCellTransform;
                Vector3 actualInitialTargetPos = firstCellBasePos - (currentCalculatedOffsetDirection * oscillationOffsetDistance);

                float initialMoveTimer = 0f;
                while (initialMoveTimer < cellTransitionDuration && isStartAnimating)
                {
                    if (currentRopeBridge == null) yield break;
                    if (firstCellTransform == null || !validTargetsCache.Contains(firstCellTransform)) break;

                    firstCellBasePos = firstCellTransform;
                    // Recalculate direction if cells might move during initial setup (optional, usually not needed for initial)
                    // if (validTargetsCache.Count > 1) { ... update currentCalculatedOffsetDirection ... }
                    actualInitialTargetPos = firstCellBasePos - (currentCalculatedOffsetDirection * oscillationOffsetDistance);

                    initialMoveTimer += Time.deltaTime;
                    float t = Mathf.Clamp01(initialMoveTimer / cellTransitionDuration);
                    currentRopeBridge.StartPoint.position = Vector3.Lerp(initialRopePos, actualInitialTargetPos, t);
                    yield return null;
                }
                if (isStartAnimating && currentRopeBridge != null && firstCellTransform != null && validTargetsCache.Contains(firstCellTransform))
                {
                    currentRopeBridge.StartPoint.position = actualInitialTargetPos;
                }
                singleCellTargetIsPlusOffset = true;
            }
        }


        while (isStartAnimating)
        {
            if (currentRopeBridge == null) yield break;

            validTargetsCache.Clear();
            if (cellTransforms != null)
            {
                foreach (var t in cellTransforms) if (t != null) validTargetsCache.Add(t);
            }

            if (validTargetsCache.Count == 0)
            {
                yield return null; continue;
            }

            // Determine offset direction based on current valid cells
            if (validTargetsCache.Count > 1)
            {
                Vector3 dir = (validTargetsCache[validTargetsCache.Count - 1] - validTargetsCache[0]).normalized;
                if (dir != Vector3.zero) currentCalculatedOffsetDirection = dir;
                else currentCalculatedOffsetDirection = (defaultOffsetDirection != Vector3.zero ? defaultOffsetDirection.normalized : Vector3.right);
            }
            else // Count is 1
            {
                currentCalculatedOffsetDirection = (defaultOffsetDirection != Vector3.zero ? defaultOffsetDirection.normalized : Vector3.right);
            }
            Vector3 appliedOffsetVector = currentCalculatedOffsetDirection * oscillationOffsetDistance;


            Vector3 lerpStartPos = currentRopeBridge.StartPoint.position;
            Vector3 lerpEndPos;
            Vector3 currentSegmentTargetTransform = Vector3.zero;


            if (validTargetsCache.Count == 1)
            {
                currentSegmentTargetTransform = validTargetsCache[0];
                if (currentSegmentTargetTransform == null) { yield return null; continue; }

                Vector3 centerPos = currentSegmentTargetTransform;
                if (singleCellTargetIsPlusOffset) lerpEndPos = centerPos + appliedOffsetVector;
                else lerpEndPos = centerPos - appliedOffsetVector;
            }
            else // Multi-cell logic
            {
                if (currentTargetIndexInCache < 0 || currentTargetIndexInCache >= validTargetsCache.Count)
                {
                    currentTargetIndexInCache = 0; multiCellDirection = 1;
                }

                int nextVisualCellIndex = currentTargetIndexInCache;
                if (multiCellDirection == 1)
                {
                    nextVisualCellIndex = currentTargetIndexInCache + 1;
                    if (nextVisualCellIndex >= validTargetsCache.Count)
                    {
                        multiCellDirection = -1;
                        nextVisualCellIndex = Mathf.Max(0, validTargetsCache.Count - 2);
                    }
                }
                else
                {
                    nextVisualCellIndex = currentTargetIndexInCache - 1;
                    if (nextVisualCellIndex < 0)
                    {
                        multiCellDirection = 1;
                        nextVisualCellIndex = Mathf.Min(validTargetsCache.Count - 1, 1);
                    }
                }

                if (nextVisualCellIndex < 0 || nextVisualCellIndex >= validTargetsCache.Count)
                {
                    currentTargetIndexInCache = 0; multiCellDirection = 1; nextVisualCellIndex = 0;
                    if (validTargetsCache.Count <= 0) { yield return null; continue; }
                }

                currentSegmentTargetTransform = validTargetsCache[nextVisualCellIndex];
                if (currentSegmentTargetTransform == null) { yield return null; continue; }

                Vector3 visualTargetPos = currentSegmentTargetTransform;
                lerpEndPos = visualTargetPos;

                if (nextVisualCellIndex == 0) lerpEndPos = visualTargetPos - appliedOffsetVector;
                else if (nextVisualCellIndex == validTargetsCache.Count - 1) lerpEndPos = visualTargetPos + appliedOffsetVector;
            }

            float journeyTimer = 0f;
            Vector3 dynamicLerpStartPos = lerpStartPos;

            while (journeyTimer < cellTransitionDuration)
            {
                if (!isStartAnimating || currentRopeBridge == null) yield break;
                if (currentSegmentTargetTransform == null) break;

                Vector3 currentVisualCellPos = currentSegmentTargetTransform;
                Vector3 currentDynamicLerpEndPos;

                // Re-calculate appliedOffsetVector here if direction could change mid-lerp due to other cells moving
                // For simplicity, we use the one calculated at the start of this segment's logic
                // Vector3 currentAppliedOffset = currentCalculatedOffsetDirection * oscillationOffsetDistance;


                if (validTargetsCache.Count == 1)
                {
                    if (singleCellTargetIsPlusOffset) currentDynamicLerpEndPos = currentVisualCellPos + appliedOffsetVector;
                    else currentDynamicLerpEndPos = currentVisualCellPos - appliedOffsetVector;
                }
                else
                {
                    currentDynamicLerpEndPos = currentVisualCellPos;
                    int actualTargetIndexInCache = validTargetsCache.IndexOf(currentSegmentTargetTransform);
                    if (actualTargetIndexInCache == 0) currentDynamicLerpEndPos = currentVisualCellPos - appliedOffsetVector;
                    else if (actualTargetIndexInCache == validTargetsCache.Count - 1) currentDynamicLerpEndPos = currentVisualCellPos + appliedOffsetVector;
                }

                journeyTimer += Time.deltaTime;
                float t = Mathf.Clamp01(journeyTimer / cellTransitionDuration);
                currentRopeBridge.StartPoint.position = Vector3.Lerp(dynamicLerpStartPos, currentDynamicLerpEndPos, t);
                yield return null;
            }

            if (!isStartAnimating || currentRopeBridge == null) yield break;

            if (currentSegmentTargetTransform != null)
            {
                Vector3 finalSnapPos;
                Vector3 finalCellPos = currentSegmentTargetTransform;
                // Vector3 snapAppliedOffset = currentCalculatedOffsetDirection * oscillationOffsetDistance;


                if (validTargetsCache.Count == 1)
                {
                    if (singleCellTargetIsPlusOffset) finalSnapPos = finalCellPos + appliedOffsetVector;
                    else finalSnapPos = finalCellPos - appliedOffsetVector;
                    singleCellTargetIsPlusOffset = !singleCellTargetIsPlusOffset;
                }
                else
                {
                    finalSnapPos = finalCellPos;
                    int actualTargetIndexInCache = validTargetsCache.IndexOf(currentSegmentTargetTransform);
                    if (actualTargetIndexInCache == 0) finalSnapPos = finalCellPos - appliedOffsetVector;
                    else if (actualTargetIndexInCache == validTargetsCache.Count - 1) finalSnapPos = finalCellPos + appliedOffsetVector;

                    int newCurrentIndex = validTargetsCache.IndexOf(currentSegmentTargetTransform);
                    if (newCurrentIndex != -1)
                    {
                        currentTargetIndexInCache = newCurrentIndex;
                    }
                    else
                    {
                        currentTargetIndexInCache = 0; multiCellDirection = 1;
                    }
                }
                currentRopeBridge.StartPoint.position = finalSnapPos;
            }
            else
            {
                currentTargetIndexInCache = 0; multiCellDirection = 1;
            }
        }
    }

    public void StartRopeAnimation(Material material)
    {
        gameObject.SetActive(true);
        endTargetPosition = LevelManager.Instance.CurrentLevel.ThreadCollector.EndPoint;
        if (animationStarted) return;
        animationStarted = true;

        if (currentRopeBridge != null)
        {
            Destroy(currentRopeBridge.gameObject);
        }

        GameObject ropeObj = Instantiate(ropeBridgePrefab, ropeBridgeStartPos, Quaternion.identity, ropeBridgeParent);
        ropeObj.SetActive(true);
        currentRopeBridge = ropeObj.GetComponent<RopeBridge>();
        currentRopeBridge.SetRopeMaterial(material);
        if (currentRopeBridge == null)
        {
            Debug.LogError("RopeBridgeAnimator: Instantiated RopeBridge prefab does not have a RopeBridge component!");
            animationStarted = false;
            Destroy(ropeObj);
            return;
        }
        baseStartPos = currentRopeBridge.StartPoint.position;
        StartCoroutine(RopeAnimationSequence());
    }

    public void SetCell(List<BaseCell> nearCells, Direction direction)
    {
        cellTransforms.Clear();
        for (int i = 0; i < nearCells.Count; i++)
        {
            Vector3 offset = Vector3.zero;

            // Apply offset based on direction
            switch (direction)
            {
                case Direction.Right:
                    offset = new Vector3(-0.5f, 0, 0);
                    break;
                case Direction.Left:
                    offset = new Vector3(0.5f, 0, 0);
                    break;
                case Direction.Forward:
                    offset = new Vector3(0, 0, -0.5f);
                    break;
                case Direction.Back:
                    offset = new Vector3(0, 0, 0.5f);
                    break;
                case Direction.Down:
                    offset = new Vector3(0, -0.5f, 0);
                    break;
                case Direction.Up:
                    offset = new Vector3(0, 0.5f, 0);
                    break;
            }

            cellTransforms.Add(nearCells[i].transform.position + offset);
        }
    }
}