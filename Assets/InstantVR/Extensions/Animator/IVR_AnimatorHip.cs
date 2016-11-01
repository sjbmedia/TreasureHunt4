﻿/* InstantVR Animator hip
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.0
 * date: August 5, 2016
 *
 */

using UnityEngine;

namespace IVR {

    public class IVR_AnimatorHip : IVR_Controller {

        public bool followHead = true;
        public enum Rotations {
            NoRotation = 0,
            HandRotation = 1,
            LookRotation = 2,
            Auto = 3
        };
        public Rotations rotationMethod = Rotations.HandRotation;

        [HideInInspector]
        private Vector3 headStartPosition;
        [HideInInspector]
        private Vector3 spineLength;

        void Start() { }

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_Animator>();
            base.StartController(ivr);

            headStartPosition = ivr.headTarget.position - ivr.transform.position;
            spineLength = ivr.headTarget.position - ivr.hipTarget.position;

            controllerPosition = startPosition;
            controllerRotation = startRotation;
        }

        public override void UpdateController() {
            if (enabled) {
                if (followHead) {
                    if (isUpright()) {
                        headStartPosition = ivr.headTarget.position - ivr.transform.position;
                    }
                    FollowHead();
                }

                switch (GetRotationMethod()) {
                    case Rotations.HandRotation:
                        HandRotation();
                        break;
                    case Rotations.LookRotation:
                        LookRotation();
                        break;
                }

                tracking = true;
                base.UpdateController();
            } else
                tracking = false;
        }

        private Rotations GetRotationMethod() {
            if (rotationMethod == Rotations.Auto) {
                return Rotations.LookRotation;
            } else {
                return rotationMethod;
            }
        }

        private static readonly Vector3 romSpineNegative = new Vector3(-5, -45,-20);
        private static readonly Vector3 romSpinePositive = new Vector3(70, 45, 20);

        private void FollowHead() {
            Quaternion spineRotation = Quaternion.FromToRotation(Vector3.up, ivr.headTarget.position - ivr.hipTarget.position);
            Vector3 spineAngles = (Quaternion.Inverse(ivr.transform.rotation) * spineRotation).eulerAngles;
            Vector3 clampedSpineAngles = Angles.ClampVector3(spineAngles, romSpineNegative, romSpinePositive);

            Vector3 head2hip = ivr.headTarget.position - ivr.hipTarget.position;
            Vector3 deltaXZ = new Vector3(head2hip.x, 0, head2hip.z);
            float deltaY = ivr.headTarget.position.y - ivr.transform.position.y - Mathf.Sqrt(spineLength.magnitude * spineLength.magnitude - deltaXZ.magnitude * deltaXZ.magnitude);

            Vector3 headPosition = ivr.headTarget.position - ivr.transform.position;
            Vector3 bodyStretch = headPosition - headStartPosition;

            float oldControllerY = controllerPosition.y;
            Vector3 hipPosition = ivr.headTarget.position - Quaternion.Euler(clampedSpineAngles) * (Vector3.up * spineLength.magnitude);
            controllerPosition = hipPosition - ivr.transform.position;

            if (bodyStretch.y >= -0.05F) {
                // standing upright

                float angle = Vector3.Angle(controllerPosition, ivr.hitNormal);
                if (ivr.collisions && (ivr.collided && angle > 90.1)) {
                    position = new Vector3(this.position.x, deltaY, this.position.z);
                } else {
                    controllerPosition = new Vector3(controllerPosition.x, oldControllerY, controllerPosition.z);
                }
            }
        }

        private void HandRotation() {
            float dOrientation = 0;

            float dOrientationL = Angles.Difference(ivr.hipTarget.eulerAngles.y, ivr.leftHandTarget.eulerAngles.y);
            float dOrientationR = Angles.Difference(ivr.hipTarget.eulerAngles.y, ivr.rightHandTarget.eulerAngles.y);

            if (Mathf.Sign(dOrientationL) == Mathf.Sign(dOrientationR)) {
                if (Mathf.Abs(dOrientationL) < Mathf.Abs(dOrientationR))
                    dOrientation = dOrientationL;
                else
                    dOrientation = dOrientationR;
            }

            float neckOrientation = Angles.Difference(ivr.headTarget.eulerAngles.y, ivr.hipTarget.eulerAngles.y + dOrientation);
            if (neckOrientation < 90 && neckOrientation > -90) { // head cannot turn more than 90 degrees
                controllerRotation *= Quaternion.AngleAxis(dOrientation, Vector3.up);
            }
        }

        private void LookRotation() {
            controllerRotation = Quaternion.Euler(
                controllerRotation.eulerAngles.x,
                ivr.headTarget.eulerAngles.y - ivr.transform.eulerAngles.y,
                controllerRotation.eulerAngles.z);
        }

        public override void OnTargetReset() {
        }

        private Vector3 uprightDirection = new Vector3(0, 1, 0);

        private float lastNeckHeight;
        private Vector3 lastHeadPosition;
        private Quaternion lastHeadRotation;

        private bool isUpright() {
            float velocity = (ivr.headTarget.position - lastHeadPosition).magnitude / Time.deltaTime;
            float angularVelocity = Quaternion.Angle(lastHeadRotation, ivr.headTarget.rotation) / Time.deltaTime;

            lastHeadPosition = ivr.headTarget.position;
            lastHeadRotation = ivr.headTarget.rotation;

            float deviation = Vector3.Angle(uprightDirection, ivr.headTarget.up);

            if (deviation < 4 && velocity < 0.02 && angularVelocity < 3 && velocity + angularVelocity > 0) {

                float neckHeight = ivr.headTarget.position.y - ivr.transform.position.y;
                if (Mathf.Abs(neckHeight - lastNeckHeight) > 0.01F) {

                    lastNeckHeight = ivr.headTarget.position.y - ivr.transform.position.y;
                    ivr.SendMessage("OnNewNeckMeasurement", lastNeckHeight);

                    return true;
                }
            }

            return false;
        }
    }
}