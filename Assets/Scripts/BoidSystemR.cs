using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactored
{
    public class BoidSystemR : MonoBehaviour
    {


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
            float binocularAngleOverTwo = thisBoid.BinocularAreaRangeAngle/2;
            float fullVisionAngleOverTwo = thisBoid.FullVisionAngle/2;
            float visualAngleMagnitude = Vector3.Angle(thisBoid.Velocity,thatBoid.transform.position);

            if (visualAngleMagnitude > fullVisionAngleOverTwo)
                return 0;

            if ((binocularAngleOverTwo <= visualAngleMagnitude)&&(visualAngleMagnitude<fullVisionAngleOverTwo))
                return ((fullVisionAngleOverTwo - visualAngleMagnitude) / (fullVisionAngleOverTwo - binocularAngleOverTwo));

            if ((binocularAngleOverTwo < visualAngleMagnitude) && (visualAngleMagnitude < binocularAngleOverTwo))
                return 1.0f;
            return -1;

        }
    }

