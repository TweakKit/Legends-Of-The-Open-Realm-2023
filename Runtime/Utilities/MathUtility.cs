using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Utilities
{
    public static class MathUtility
    {
        #region Members

        private static readonly int s_numberOfSamplingPoint = 10;

        #endregion Members

        #region Class Methods

        public static int RandomChoiceFollowingDistribution(List<float> probabilities)
        {
            // Sum to create CDF.
            float[] cdf = new float[probabilities.Count];
            float sum = 0;
            for (int i = 0; i < probabilities.Count; ++i)
            {
                cdf[i] = sum + probabilities[i];
                sum = cdf[i];
            }

            // Choose from CDF.
            float cdf_value = Random.Range(0.0f, cdf[probabilities.Count - 1]);
            int index = Array.BinarySearch(cdf, cdf_value);

            if (index < 0)
                index = ~index; // if not found (probably won't be) BinarySearch returns bitwise complement of next-highest index.

            return index;
        }

        /// <summary>
        /// Calculate rect intercept point.
        /// </summary>
        public static Vector2 CalculateEdgeIntercept(Vector2 pivot, Rect rect)
        {
            var direction = pivot - rect.center;
            var directionBase = rect.max - rect.center;
            var directionRatio = new Vector2(directionBase.x / Mathf.Max(0.1f, Mathf.Abs(direction.x)), directionBase.y / Mathf.Max(0.1f, Mathf.Abs(direction.y)));
            Vector2 edgePoint = Vector2.zero;
            if (directionRatio.x < directionRatio.y)
            {
                // Point is on the vertical side of the rect.
                edgePoint.x = direction.x > 0 ? rect.xMax : rect.xMin;
                edgePoint.y = direction.y * directionRatio.x + rect.center.y;
            }
            else
            {
                edgePoint.x = direction.x * directionRatio.y + rect.center.x;
                edgePoint.y = direction.y > 0 ? rect.yMax : rect.yMin;
            }

            return edgePoint;
        }

        /// <summary>
        /// Calculate UI offset point.
        /// </summary>
        public static Vector2 CalculateOffsetPoint(Vector2 center, Vector2 interceptPoint, Vector2 displaySize)
        {
            var direction = interceptPoint - center;
            var radius = new Vector2(displaySize.x / 2, displaySize.y / 2).magnitude;
            return direction.normalized * (direction.magnitude + radius) + center;
        }

        /// <summary>
        /// Calculate UI on screen point when target is off screen.
        /// </summary>
        public static void CalculateOutOfScreenPosition(GameObject obj, Camera camera, Canvas canvas, ref Vector3 targetLocation, float screenOffset)
        {
            if (camera == null || canvas == null)
                return;

            float posAngle, slope, cosine, sine, clampX, clampY;
            Vector3 viewPortCenter = new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2);
            Vector2 mappedPosition;

            var currentScreenPosition = camera.WorldToScreenPoint(obj.transform.position);
            if (currentScreenPosition.z < 0)
            {
                currentScreenPosition.x = camera.pixelWidth - currentScreenPosition.x;
                currentScreenPosition.y = camera.pixelHeight - currentScreenPosition.y;
            }

            // Adjust the center to the viewport center.
            currentScreenPosition -= viewPortCenter;

            // Get the angle and adjust it to viewport angle.
            posAngle = Mathf.Atan2(currentScreenPosition.y, currentScreenPosition.x);
            posAngle -= 90 * Mathf.Deg2Rad;
            cosine = Mathf.Cos(posAngle);
            sine = -Mathf.Sin(posAngle);
            slope = cosine / sine;
            var clampScreenPos = viewPortCenter * screenOffset;

            // Check the position of the intercept point.
            if (cosine > 0)
            {
                // Up.
                clampY = clampScreenPos.y;
            }
            else
            {
                // Down.
                clampY = -clampScreenPos.y;
            }

            clampX = clampY / slope;

            // Check out of bound situation.
            if (clampX > clampScreenPos.x)
            {
                // Out of bound right.
                clampX = clampScreenPos.x;
                clampY = clampX * slope;
            }
            else if (clampX < -clampScreenPos.x)
            {
                // Out of bound left.
                clampX = -clampScreenPos.x;
                clampY = clampX * slope;
            }

            // Map back to viewport.
            currentScreenPosition = new Vector3(clampX, clampY) + viewPortCenter;
            Camera screenCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, currentScreenPosition, screenCam, out mappedPosition))
                targetLocation = mappedPosition;
            else
                targetLocation = Vector3.zero;
        }

        /// <summary>
        /// Calculate UI on screen point when target is off screen.
        /// </summary>
        public static bool CalculateOutOfScreenPosition(Vector3 position, Camera camera, Canvas canvas, ref Vector3 targetLocation, float screenOffset, out bool outOfScreen)
        {
            if (camera == null || canvas == null)
            {
                outOfScreen = false;
                return false;
            }

            float posAngle, slope, cosine, sine, clampX, clampY;
            Vector3 viewPortCenter = new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2);
            var clampScreenPosition = viewPortCenter * screenOffset;
            Vector2 mappedPosition;
            var curScreenPosition = CalculateScreenPosition(position, camera);

            // Adjust the center to the viewport center.
            curScreenPosition -= viewPortCenter;

            if (TargetInBoundCheck(curScreenPosition, clampScreenPosition))
            {
                outOfScreen = false;
                curScreenPosition += viewPortCenter;
            }
            else
            {
                outOfScreen = true;
                // Get the angle and adjust it to viewport angle.
                posAngle = Mathf.Atan2(curScreenPosition.y, curScreenPosition.x);
                posAngle -= 90 * Mathf.Deg2Rad;
                cosine = Mathf.Cos(posAngle);
                sine = -Mathf.Sin(posAngle);
                slope = cosine / sine;

                // Check the position of the intercept point.
                if (cosine > 0)
                {
                    // Up.
                    clampY = clampScreenPosition.y;
                }
                else
                {
                    // Down.
                    clampY = -clampScreenPosition.y;
                }

                clampX = clampY / slope;

                // Check out of bound situation.
                if (clampX > clampScreenPosition.x)
                {
                    // Out of bound right.
                    clampX = clampScreenPosition.x;
                    clampY = clampX * slope;
                }
                else if (clampX < -clampScreenPosition.x)
                {
                    // Out of bound left.
                    clampX = -clampScreenPosition.x;
                    clampY = clampX * slope;
                }

                // Map back to viewport.
                curScreenPosition = new Vector3(clampX, clampY) + viewPortCenter;
            }

            Camera screenCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, curScreenPosition, screenCam, out mappedPosition))
            {
                targetLocation = mappedPosition;
                return true;
            }
            else
            {
                targetLocation = Vector3.zero;
                return false;
            }
        }

        public static bool TargetInBoundCheck(Vector3 position, Vector3 screenBorderPosition)
        {
            if (position.z > 0 && Mathf.Abs(position.x) < screenBorderPosition.x && Mathf.Abs(position.y) < screenBorderPosition.y)
                return true;
            else
                return false;
        }

        public static Vector3 CalculateScreenPosition(Vector3 position, Camera camera)
        {
            var currentScreenPosition = camera.WorldToScreenPoint(position);
            if (currentScreenPosition.z < 0)
            {
                currentScreenPosition.x = camera.pixelWidth - currentScreenPosition.x;
                currentScreenPosition.y = camera.pixelHeight - currentScreenPosition.y;
            }
            return currentScreenPosition;
        }

        public static bool CalculateLocalRectPoint(Vector3 worldPosition, Camera camera, Canvas canvas, out Vector3 screenPosition)
        {
            var currentScreenPosition = CalculateScreenPosition(worldPosition, camera);
            Camera screenCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 mappedPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, currentScreenPosition, screenCamera, out mappedPosition))
            {
                screenPosition = mappedPosition;
                screenPosition.z = currentScreenPosition.z;
                return true;
            }
            else
            {
                screenPosition = Vector3.zero;
                return false;
            }
        }

        public static Vector2 Bezier(Vector2 a, Vector2 b, float t)
            => Vector2.Lerp(a, b, t);

        public static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
            => Vector2.Lerp(Bezier(a, b, t), Bezier(b, c, t), t);

        public static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
            => Vector2.Lerp(Bezier(a, b, c, t), Bezier(b, c, d, t), t);

        public static List<Vector2> GetPointsInCirce(float circleRadius, float objectRadius)
        {
            var generatedPoints = new List<Vector2>();
            var circleCenterPoint = Vector2.one * circleRadius;

            foreach (var checkedPoint in GetPointsInRegion(objectRadius, Vector2.one * circleRadius * 2))
                if ((checkedPoint - circleCenterPoint).sqrMagnitude < circleRadius * circleRadius)
                    generatedPoints.Add(checkedPoint);

            return generatedPoints;
        }

        private static List<Vector2> GetPointsInRegion(float objectRadius, Vector2 regionSize)
        {
            float cellSize = objectRadius / Mathf.Sqrt(2);
            int[,] grid = new int[Mathf.CeilToInt(regionSize.x / cellSize), Mathf.CeilToInt(regionSize.y / cellSize)];
            var generatedPoints = new List<Vector2>();
            var checkedPoints = new List<Vector2>();

            checkedPoints.Add(regionSize / 2);
            while (checkedPoints.Count > 0)
            {
                bool haveFoundNewPoint = false;
                int checkedPointIndex = Random.Range(0, checkedPoints.Count);

                for (int i = 0; i < s_numberOfSamplingPoint; i++)
                {
                    var sampleAngle = Random.value * Mathf.PI * 2;
                    var direction = new Vector2(Mathf.Sin(sampleAngle), Mathf.Cos(sampleAngle));
                    var checkedPoint = checkedPoints[checkedPointIndex] + direction * objectRadius;

                    if (checkedPoint.x < 0 || checkedPoint.x >= regionSize.x || checkedPoint.y < 0 || checkedPoint.y >= regionSize.y)
                        continue;

                    if (IsValidPoint(checkedPoint, cellSize, objectRadius, generatedPoints, grid))
                    {
                        generatedPoints.Add(checkedPoint);
                        checkedPoints.Add(checkedPoint);
                        grid[(int)(checkedPoint.x / cellSize), (int)(checkedPoint.y / cellSize)] = generatedPoints.Count;
                        haveFoundNewPoint = true;
                        break;
                    }
                }

                if (!haveFoundNewPoint)
                    checkedPoints.RemoveAt(checkedPointIndex);
            }

            return generatedPoints;
        }

        private static bool IsValidPoint(Vector2 checkedPoint, float cellSize, float objectRadius, List<Vector2> generatedPoints, int[,] grid)
        {
            int cellIndexX = (int)(checkedPoint.x / cellSize);
            int cellIndexY = (int)(checkedPoint.y / cellSize);

            for (int x = Mathf.Max(0, cellIndexX - 2); x <= Mathf.Min(cellIndexX + 2, grid.GetLength(0) - 1); x++)
                for (int y = Mathf.Max(0, cellIndexY - 2); y <= Mathf.Min(cellIndexY + 2, grid.GetLength(1) - 1); y++)
                    if (grid[x, y] - 1 != -1 && (checkedPoint - generatedPoints[grid[x, y] - 1]).sqrMagnitude < objectRadius * objectRadius)
                        return false;

            return true;
        }

        #endregion Class Methods
    }
}