﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactored
{
    public class BoidR : MonoBehaviour
    {
        //the "frontal binocular vision" range of the boid
        float binocularAreaRange = 30.0f;
        public float BinocularAreaRangeAngle { get { return binocularAreaRange; } }

        float fullVisionRange = 340.0f;
        public float FullVisionAngle { get { return fullVisionRange; } }

        public Vector3 Velocity { get; set; }
        public Vector3 LastPosition { get; set; }

        //This determines the boid's initial speed
        Vector3 relativeOldPosition = new Vector3(0.0f, 0.1f, 0.0f);

        private void Start()
        {
            LastPosition = CalculateLastPosition();
            Velocity.Set(0.01f, 0.01f, 0.0f);
        }

        public void Reset()
        {
            Velocity = Vector3.zero;
            LastPosition = CalculateLastPosition();
        }

        Vector3 CalculateLastPosition()
        {
            return transform.position - relativeOldPosition;
        }
    }
}

