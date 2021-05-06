using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowPreview : MonoBehaviour
{
    private struct Point2
    {
        public float x;
        public float y;
        public Point2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private struct Parabola
    {
        private float constantValue;
        private Point2 peak;
        public Parabola(Point2 point, Point2 peak)
        {
            this.peak = peak;
            constantValue = (point.y - peak.y) / ((point.x - peak.x) * (point.x - peak.x));
        }

        public float getY(float x)
        {
            return (constantValue * (x - peak.x) * (x - peak.x)) + peak.y;
        }
    }

    [Tooltip("Line prefab you want to instantiate.")]
    public LineRenderer linePrefab;
    [Tooltip("Line start position relative to the camera.")]
    public Vector3 cameraOffset = new Vector3(0, 0, 5);
    [Tooltip("X intervals of the parabola. If you increase this, the parabola's lines will be more unvisible.")]
    public float pointInterval = 1;
    [Tooltip("How much do you want to raise the parabola in Y axis.")]
    public float peakY = 3;
    [Tooltip("The max distance you want the parabola to be drawn.")]
    public float maxDistance = 1000;
    [Tooltip("The layer you want to assign to the line.")]
    public int lineLayer = 2;
    [Tooltip("The layers whose objects will be considered as a surface.")]
    public LayerMask surfaceLayerMask = ~0;
    private LineRenderer instantiatedLine;
    private Vector3 targetPosition;
    private Vector3 previousTargetPosition;
    private Camera cameraComponent;
    private Parabola currentParabola;
    private Point2[] getParabolaPoints()
    {
        Vector3 differenceVector = targetPosition - transform.position;
        float differenceMagnitude = differenceVector.magnitude;
        int pointCount = (int)Mathf.Round(differenceMagnitude / pointInterval);
        Point2[] pointArray = new Point2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            float xValue = i * pointInterval;
            pointArray[i] = new Point2(xValue, currentParabola.getY(xValue));
        }
        return pointArray;
    }

    private void setLinePoints(Point2[] points)
    {
        int pointCount = points.Length;
        instantiatedLine.positionCount = pointCount;
        for (int i = 0; i < pointCount; i++)
        {
            Point2 iteratedPoint = points[i];
            instantiatedLine.SetPosition(i, new Vector3(0, iteratedPoint.y, iteratedPoint.x));
        }
    }
    private void Start()
    {
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent == null)
        {
            throw new System.Exception("ThrowPreview script must be attached to a gameObject that has a camera component.");
        }
        surfaceLayerMask.value = surfaceLayerMask.value & ~lineLayer;
        instantiatedLine = Instantiate(linePrefab, transform.position, transform.rotation, transform);
        instantiatedLine.gameObject.layer = lineLayer;
        instantiatedLine.transform.localPosition = cameraOffset;
        targetPosition = cameraComponent.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Update()
    {
        Ray ray = cameraComponent.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        bool hit = Physics.Raycast(ray, out raycastHit, maxDistance, surfaceLayerMask, QueryTriggerInteraction.Ignore);
        if (hit)
        {
            targetPosition = raycastHit.point;
            Vector3 deltaVector = targetPosition - previousTargetPosition;
            float deltaMagnitude = deltaVector.magnitude;
            bool targetDidNotChange = Mathf.Approximately(deltaMagnitude, 0f);
            if (!targetDidNotChange)
            {
                Vector3 differenceVector = targetPosition - transform.position;
                float differenceMagnitude = differenceVector.magnitude;
                instantiatedLine.transform.LookAt(targetPosition, Vector3.up);
                currentParabola = new Parabola(new Point2(deltaVector.z, deltaVector.y), new Point2(differenceMagnitude / 2, peakY));
                Point2[] parabolaPoints = getParabolaPoints();
                setLinePoints(parabolaPoints);
                previousTargetPosition = targetPosition;
            }
        }
    }
}
