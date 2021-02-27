using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactored
{
    public class BoidSystemR : MonoBehaviour
    {
        List<GameObject>[,,] spatialGrid;
        public int numberBoids;
        public int flySpace = 40;
        public GameObject boidPrefab;
        public GameObject[] boidObjectPool;

        //these are variables to play with that affects flocking behaviour
        public float kCollisionScale = 0.5f;
        public float kVelocityScale = 0.7f;
        public float kCenteringScale = 0.1f;


        void Start()
        {
            boidObjectPool = new GameObject[numberBoids];
            InitSpatialGrid();
        }

        void FixedUpdate()
        {
            Reset();
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

        void Reset()
        {
            //init grid
            // for each boid in num boids
            BoidR currentBoid;
            Vector3 position;
            for (int i = 0; i < numberBoids; i++)
            {
                currentBoid = boidObjectPool[i].GetComponent<BoidR>();
                position = currentBoid.transform.position;

                if (BoidOutOfBounds(position))
                {
                    position = GetRandomStartPosition();
                    currentBoid.transform.position = position;
                    currentBoid.Reset();
                }

                AddBoidToVoxel(position, currentBoid.gameObject);
            }
          
            //hash to position and add the boid to the spot

        }

        /// <summary>
        /// Calculates whether the position is outside of the fly space
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool BoidOutOfBounds(Vector3 position)
        {
            return (position.x < 0.0f || position.x > flySpace
                || position.y < 0.0f || position.y > flySpace
                || position.z < 0.0f || position.z > flySpace);
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

        /// <summary>
        /// Compute a random vector within the flyspace
        /// </summary>
        /// <returns>A randomized position vector</returns>
        Vector3 GetRandomStartPosition()
        {
            return new Vector3(Random.Range(0.0f + (3 * flySpace / 7), flySpace - (3 * flySpace / 7)),
                               Random.Range(0.0f + (3 * flySpace / 7), flySpace - (3 * flySpace / 7)),
                               Random.Range(0.0f + (3 * flySpace / 7), flySpace - (3 * flySpace / 7)));
        }

        void UpdateBoidPositions()
        {
            BoidR currentBoid;
            Vector3 currentBoidPosition;
            List<GameObject> boidNeighbours = new List<GameObject>();
            for(int i = 0; i < flySpace; i++)
            {
                for(int j=0; j < flySpace; j++)
                {
                    for(int k = 0; k < flySpace; k++)
                    {
                        if (VoxelEmpty(i,j,k)) continue;
                        int boidCountAtVoxel = VoxelCount(i, j, k);
                        
                        int neighbourCount;

                        //init forces
                        Vector3 collision = Vector3.zero;
                        Vector3 velocityMatch = Vector3.zero;
                        Vector3 centering = Vector3.zero;

                        for (int boidVoxelIndex = 0; boidVoxelIndex < boidCountAtVoxel; boidVoxelIndex++)
                        {
                            currentBoid = GetBoid(i, j, k, boidVoxelIndex);
                            currentBoidPosition = GetBoidPosition(i, j, k, boidVoxelIndex);

                            boidNeighbours = GetBoidNeighbours(i, j, k);
                            neighbourCount = boidNeighbours.Count;

                            for (int neighbourIndex = 0; neighbourIndex < neighbourCount; neighbourIndex++)
                            {
                                int currentBoidID = currentBoid.GetInstanceID();
                                int boidNeighbourID = boidNeighbours[boidVoxelIndex].GetInstanceID();

                                if (currentBoidID == boidNeighbourID) continue;

                                BoidR otherBoid = boidNeighbours[neighbourIndex].GetComponent<BoidR>();

                                ComputeWeightAndAccelerations(currentBoid, otherBoid, collision, velocityMatch, centering);
                            }


                            Vector3 acceleration = GetAccelerationPrioritization(collision, velocityMatch, centering);
                            currentBoid.Velocity = ComputeVelocity(acceleration, currentBoid);

                            

                    }
                }
            }
        }

        Vector3 GetBoidPosition(int i, int j, int k, int boidListPosition)
        {
            return spatialGrid[i, j, k][boidListPosition].transform.position;
        }

        /// <summary>
        /// Retrieves a list of all the boids in adjacent voxels to
        /// spatialGrid at i,j,k.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        List<GameObject> GetBoidNeighbours(int i, int j, int k)
        {
            List<GameObject> neighbours = new List<GameObject>();

            for (int iNeighbour = i - 1; iNeighbour <= i + 1; iNeighbour++)
            {
                if (NeighbourOutOfBounds(iNeighbour)) continue;
                for (int jNeighbour = j - 1; jNeighbour <= j + 1; jNeighbour++)
                {
                    if (NeighbourOutOfBounds(jNeighbour)) continue;
                    for (int kNeighbour = k - 1; kNeighbour <= k + 1; kNeighbour++)
                    {
                        if (NeighbourOutOfBounds(kNeighbour)) continue;
                        neighbours.AddRange(spatialGrid[iNeighbour, jNeighbour, kNeighbour]);
                    }
                }
            }

            return neighbours;
        }

        bool NeighbourOutOfBounds(int position)
        {
            return ((position < 0) || (position >= flySpace));
        }

        /// <summary>
        /// Retrieves the BoidR component of the boid at the position in the list
        /// and at the particular voxel.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="boidPosition"></param>
        /// <returns></returns>
        BoidR GetBoid(int i, int j, int k, int boidPosition)
        {
            return spatialGrid[i, j, k][boidPosition].GetComponent<BoidR>();
        }

        /// <summary>
        /// Returns the amount of boids at a given voxel
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        int VoxelCount(int i, int j, int k)
        {
            return spatialGrid[i, j, k].Count;
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
