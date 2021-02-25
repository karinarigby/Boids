using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactored
{
    public class BoidSystemR : MonoBehaviour
    {
        List<GameObject>[,,] spatialGrid;
        static int numberBoids = 15;
        static int flySpace = 80;
        public static GameObject[] boidObjectPool = new GameObject[numberBoids];



        void Start()
        {
            InitSpatialGrid();
            UpdateBoidPositions();
        }

        void InitSpatialGrid()
        {
            spatialGrid = new List<GameObject>[flySpace, flySpace, flySpace];

            Vector3 boidStartPosition = new Vector3();
            for (int i = 0; i < numberBoids; i++)
            {
                // boidStartPosition =
                boidStartPosition = GetStartPosition();
            }
        }

        Vector3 GetStartPosition()
        {
            return new Vector3(Random.Range(0.0f + (3 * flySpace / 7), flySpace - (3 * flySpace / 7)),
                               Random.Range(0.0f + (3 * flySpace / 7), flySpace - (3 * flySpace / 7)),
                               Random.Range(0.0f + (3 * flySpace / 7), flySpace - (3 * flySpace / 7)));

        }

        void UpdateBoidPositions()
        {
            for(int i = 0; i < flySpace; i++)
            {
                for(int j=0; j < flySpace; j++)
                {
                    for(int k = 0; k < flySpace; k++)
                    {
                        if (VoxelEmpty(i, j, k)) continue;


                    }
                }
            }
        }

        bool VoxelEmpty(int i, int j, int k)
        {
            return spatialGrid[i, j, k] == null;
        }

        float ComputeDistanceWeight(Vector3 distance)
        {
            float closeDistanceThreshold = 0.5f;
            float farDistanceThreshold = 1.5f;
            float magnitude = distance.magnitude;
            if (magnitude < closeDistanceThreshold)
                return 1.0f;
            if ((closeDistanceThreshold <= magnitude) && (magnitude <= farDistanceThreshold))
                return (farDistanceThreshold - magnitude) /(farDistanceThreshold-closeDistanceThreshold);
            return 0;
        }

        float ComputeVisualFieldWeight(BoidR thisBoid, BoidR thatBoid)
        {
            float binocularAngleOverTwo = thisBoid.BinocularAreaRangeAngle / 2;
            float fullVisionAngleOverTwo = thisBoid.FullVisionAngle / 2;
            float visualAngleMagnitude = Vector3.Angle(thisBoid.Velocity, thatBoid.transform.position);

            if (visualAngleMagnitude > fullVisionAngleOverTwo)
                return 0;

            if ((binocularAngleOverTwo <= visualAngleMagnitude) && (visualAngleMagnitude < fullVisionAngleOverTwo))
                return ((fullVisionAngleOverTwo - visualAngleMagnitude) / (fullVisionAngleOverTwo - binocularAngleOverTwo));

            if ((binocularAngleOverTwo < visualAngleMagnitude) && (visualAngleMagnitude < binocularAngleOverTwo))
                return 1.0f;
            return -1;

        }

        Vector3 GetAccelerationPrioritization(Vector3 collisionAvoidance, Vector3 velocityMatch, Vector3 centering)
        {
            float accelerationCap = 0.5f;
            float residualAcceleration = accelerationCap;

            Vector3 combinedAcceleration = Mathf.Min(residualAcceleration, collisionAvoidance.magnitude) * collisionAvoidance.normalized;
            residualAcceleration = accelerationCap - combinedAcceleration.magnitude;

            combinedAcceleration = combinedAcceleration +
                Mathf.Min(residualAcceleration, velocityMatch.magnitude) * velocityMatch.normalized;
            residualAcceleration = accelerationCap - combinedAcceleration.magnitude;

            combinedAcceleration = combinedAcceleration + Mathf.Min(residualAcceleration, centering.magnitude) * centering.normalized;

            return combinedAcceleration;
        }

        
    }
}
