using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactored
{
    public class BoidSystemR : MonoBehaviour
    {
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

