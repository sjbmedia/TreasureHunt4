﻿/* InstantVR Generic functions
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.0
 * date: May 22, 2016
 *
 */
using UnityEngine;

namespace IVR {

    public static class Angles {
        // Clamp all vector acis between the given min and max values
        // Angles are normalized
        public static Vector3 ClampVector3(Vector3 angles, Vector3 min, Vector3 max) {
            float x = Clamp(angles.x, min.x, max.x);
            float y = Clamp(angles.y, min.y, max.y);
            float z = Clamp(angles.z, min.z, max.z);
            return new Vector3(x, y, z);
        }

        // clamp the angle between the given min and max values
        // Angles are normalized
        public static float Clamp(float angle, float min, float max) {
            float normalizedAngle = Normalize(angle);
            return Mathf.Clamp(normalizedAngle, min, max);
        }

        // Determine the angle difference, result is a normalized angle
        public static float Difference(float a, float b) {
            float r = Normalize(b - a);
            return r;
        }

        // Normalize an angle to the range -180 < a <= 180
        public static float Normalize(float a) {
            while (a <= -180) a += 360;
            while (a > 180) a -= 360;
            return a;
        }
    }
}