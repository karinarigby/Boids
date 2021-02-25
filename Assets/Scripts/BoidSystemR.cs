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
        public GameObject boidPrefab;
        public static GameObject[] boidObjectPool = new GameObject[numberBoids];

        void Start()
        {
            InitSpatialGrid();
            UpdateBoidPositions();
        }

        /// <summary>
        /// Sets up the boid object pool and hashes each
        /// boid to a spatial uniform grid.
        /// </summary>
        void InitSpatialGrid()
        {
            InitGridDataStructures();
            InitBoidPool();
            Vector3 position;

            for (int i = 0; i < numberBoids; i++)
            {
                position = GetBoidPosition(i);
                AddBoidToVoxel(position, boidObjectPool[i]);  
            }
        }

        /// <summary>
        ///  Init the uniform spatial grid and each voxel list
        /// </summary>
        void InitGridDataStructures()
        {
            spatialGrid = new List<GameObject>[flySpace, flySpace, flySpace];
            for (int i = 0; i < flySpace; i++)
            {
                for (int j = 0; j < flySpace; j++)
                {
                    for (int k = 0; k < flySpace; k++)
                    {
                        spatialGrid[i, j, k] = new List<GameObject>();
                    }
                }
            }
        }

        /// <summary>
        /// Adds the game object to the list of boids at the voxel
        /// </summary>
        /// <param name="position">Where in spatial uniform
        /// grid to hash position to</param>
        /// <param name="boid">The game object to append on to list</param>
        void AddBoidToVoxel(Vector3 position, GameObject boid)
        {
            ListAtPosition(position).Add(boid);
        }

        /// <summary>
        /// Returns the list of the boids at the voxel in uniform spatial grid
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        List<GameObject> ListAtPosition(Vector3 position)
        {
            return spatialGrid[(int)Mathf.Floor(position.x),
                               (int)Mathf.Floor(position.y),
                               (int)Mathf.Floor(position.z)];
        }

        /// <summary>
        /// Retrieves the position of the GameObject of boidObjectPool
        /// </summary>
        /// <param name="i">the index of boidObjectPool</param>
        /// <returns></returns>
        Vector3 GetBoidPosition(int i)
        {
            return boidObjectPool[i].transform.position;
        }

        /// <summary>
        /// Checks if there is a list of game objects initialized at the voxel
        /// </summary>
        /// <param name="position">The hash location to check</param>
        /// <returns></returns>
        bool VoxelAtPositionEmpty(Vector3 position)
        {
            return spatialGrid[(int)Mathf.Floor(position.x), (int)Mathf.Floor(position.y), (int)Mathf.Floor(position.z)].Count == 0;
        }

        bool VoxelEmpty(int i, int j, int k)
        {
            return spatialGrid[i, j, k].Count == 0;
        }

        /// <summary>
        /// Instantiate an objectPool of boids for use and reuse
        /// </summary>
        void InitBoidPool()
        {
            Vector3 startPosition;
            for(int i = 0; i < numberBoids; i++)
            {
                startPosition = GetRandomStartPosition();
                boidObjectPool[i] = Instantiate(boidPrefab, startPosition, Quaternion.identity);
            }
        }

        Vector3 GetRandomStartPosition()
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
                        if (spatialGrid[i,j,k].Count == 0) continue;


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
