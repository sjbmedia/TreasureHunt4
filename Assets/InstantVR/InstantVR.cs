/* InstantVR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.0
 * date: May 13, 2016
 * 
 * - Improved ground checking
 * - Improved collisions
 * - Added step offeset
 */

using UnityEngine;

namespace IVR {

    [HelpURL("http://serrarens.nl/passervr/support/vr-component-functions/instantvr-function/")]
    public class InstantVR : MonoBehaviour {

        [Tooltip("Target Transform for the head")]
        public Transform headTarget;
        [Tooltip("Target Transform for the left hand")]
        public Transform leftHandTarget;
        [Tooltip("Target Transform for the right hand")]
        public Transform rightHandTarget;
        [Tooltip("Target Transform for the hip")]
        public Transform hipTarget;
        [Tooltip("Target Transform for the left foot")]
        public Transform leftFootTarget;
        [Tooltip("Target Transform for the right foot")]
        public Transform rightFootTarget;

        public enum BodySide {
            Unknown = 0,
            Left = 1,
            Right = 2,
        };

        [HideInInspector]
        private IVR_Extension[] extensions;

        [HideInInspector]
        private IVR_Controller[] headControllers;
        [HideInInspector]
        private IVR_Controller[] leftHandControllers, rightHandControllers;
        [HideInInspector]
        private IVR_Controller[] hipControllers;
        [HideInInspector]
        private IVR_Controller[] leftFootControllers, rightFootControllers;

        private IVR_Controller headController;
        public IVR_Controller HeadController { get { return headController; } set { headController = value; } }
        private IVR_Controller leftHandController, rightHandController;
        public IVR_Controller LeftHandController { get { return leftHandController; } set { leftHandController = value; } }
        public IVR_Controller RightHandController { get { return rightHandController; } set { rightHandController = value; } }
        private IVR_Controller hipController;
        public IVR_Controller HipController { get { return hipController; } set { hipController = value; } }
        private IVR_Controller leftFootController, rightFootController;
        public IVR_Controller LeftFootController { get { return leftFootController; } set { leftFootController = value; } }
        public IVR_Controller RightFootController { get { return rightFootController; } set { rightFootController = value; } }

        [HideInInspector]
        private IVR_Movements headMovements;
        [HideInInspector]
        public IVR_HandMovementsBase leftHandMovements, rightHandMovements;
        [HideInInspector]
        private IVR_BodyMovements bodyMovements;

        [HideInInspector]
        public Transform characterTransform;

        [HideInInspector]
        public int playerID = 0;

        [Tooltip("The character will step up a stair only if it is closer to the ground than the indicated value.")]
        public float stepOffset = 0.3F;

        public bool useGravity = true;

        public bool collisions = true;
        [HideInInspector]
        public bool triggerEntered = false, collided = false;
        [HideInInspector]
        public Vector3 hitNormal = Vector3.zero;
        [HideInInspector]
        public Vector3 inputDirection;

        protected virtual void Awake() {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            extensions = this.GetComponents<IVR_Extension>();
            foreach (IVR_Extension extension in extensions)
                extension.StartExtension(this);

            headControllers = headTarget.GetComponents<IVR_Controller>();
            leftHandControllers = leftHandTarget.GetComponents<IVR_Controller>();
            rightHandControllers = rightHandTarget.GetComponents<IVR_Controller>();
            hipControllers = hipTarget.GetComponents<IVR_Controller>();
            leftFootControllers = leftFootTarget.GetComponents<IVR_Controller>();
            rightFootControllers = rightFootTarget.GetComponents<IVR_Controller>();

            headController = FindActiveController(headControllers);
            leftHandController = FindActiveController(leftHandControllers);
            rightHandController = FindActiveController(rightHandControllers);
            hipController = FindActiveController(hipControllers);
            leftFootController = FindActiveController(leftFootControllers);
            rightFootController = FindActiveController(rightFootControllers);

            headMovements = headTarget.GetComponent<IVR_Movements>();

            leftHandMovements = leftHandTarget.GetComponent<IVR_HandMovementsBase>();
            if (leftHandMovements != null)
                leftHandMovements.selectedController = (IVR_HandController)FindActiveController(leftHandControllers);

            rightHandMovements = rightHandTarget.GetComponent<IVR_HandMovementsBase>();
            if (rightHandMovements != null)
                rightHandMovements.selectedController = (IVR_HandController)FindActiveController(rightHandControllers);


            Animator[] animators = GetComponentsInChildren<Animator>();
            for (int i = 0; i < animators.Length; i++) {
                Avatar avatar = animators[i].avatar;
                if (avatar.isValid && avatar.isHuman) {
                    characterTransform = animators[i].transform;

                    if (collisions) {
                        AddRigidbody(characterTransform.gameObject);
                        //bodyCapsule = AddCharacterColliders(hipTarget.gameObject, proximitySpeed);
                        bodyCapsule = AddCharacterColliders(this, proximitySpeed);
                    }
                }
            }

            foreach (IVR_Controller c in headControllers)
                c.StartController(this);
            foreach (IVR_Controller c in leftHandControllers)
                c.StartController(this);
            foreach (IVR_Controller c in rightHandControllers)
                c.StartController(this);
            foreach (IVR_Controller c in hipControllers)
                c.StartController(this);
            foreach (IVR_Controller c in leftFootControllers)
                c.StartController(this);
            foreach (IVR_Controller c in rightFootControllers)
                c.StartController(this);

            bodyMovements = GetComponent<IVR_BodyMovements>();
            if (bodyMovements != null)
                bodyMovements.StartMovements();

            if (headMovements && headMovements.enabled)
                headMovements.StartMovements(this);
            if (leftHandMovements != null && leftHandMovements.enabled)
                leftHandMovements.StartMovements(this);
            if (rightHandMovements != null && rightHandMovements.enabled)
                rightHandMovements.StartMovements(this);
        }

        private IVR_Controller FindActiveController(IVR_Controller[] controllers) {
            for (int i = 0; i < controllers.Length; i++) {
                if (controllers[i].isTracking())
                    return (controllers[i]);
            }
            return null;
        }

        void Update() {
            Controllers.Clear();
            UpdateExtensions();
            UpdateControllers();

            UpdateMovements();
        }

        void LateUpdate() {
            LateUpdateExtensions();
        }

        private void UpdateExtensions() {
            foreach (IVR_Extension extension in extensions)
                extension.UpdateExtension();
        }

        private void LateUpdateExtensions() {
            foreach (IVR_Extension extension in extensions)
                extension.LateUpdateExtension();
        }

        private void UpdateControllers() {
            if (leftHandMovements != null)
                leftHandMovements.selectedController = (IVR_HandController)UpdateController(leftHandControllers, leftHandMovements.selectedController);
            if (rightHandMovements != null)
                rightHandMovements.selectedController = (IVR_HandController)UpdateController(rightHandControllers, rightHandMovements.selectedController);

            hipController = UpdateController(hipControllers, hipController);
            leftFootController = UpdateController(leftFootControllers, leftFootController);
            rightFootController = UpdateController(rightFootControllers, rightFootController);
            // Head needs to be after hands because of traditional controller.
            headController = UpdateController(headControllers, headController);
        }

        private IVR_Controller UpdateController(IVR_Controller[] controllers, IVR_Controller lastActiveController) {
            if (controllers != null) {
                int lastIndex = 0, newIndex = 0;

                IVR_Controller activeController = null;
                for (int i = 0; i < controllers.Length; i++) {
                    if (controllers[i] != null) {
                        controllers[i].UpdateController();
                        if (activeController == null && controllers[i].isTracking()) {
                            activeController = controllers[i];
                            controllers[i].SetSelection(true);
                            newIndex = i;
                        } else
                            controllers[i].SetSelection(false);

                        if (controllers[i] == lastActiveController)
                            lastIndex = i;
                    }
                }

                if (lastIndex < newIndex && lastActiveController != null) { // we are degrading
                    activeController.TransferCalibration(lastActiveController);
                }

                return activeController;
            } else
                return null;
        }

        private void UpdateMovements() {
            if (headMovements && headMovements.enabled)
                headMovements.UpdateMovements();
            if (leftHandMovements && leftHandMovements.enabled)
                leftHandMovements.UpdateMovements();
            if (rightHandMovements && rightHandMovements.enabled)
                rightHandMovements.UpdateMovements();
            if (bodyMovements != null)
                bodyMovements.UpdateBodyMovements();

            CalculateSpeed();
            PlaceBodyCapsule();
            DetermineCollision();
            if (useGravity)
                CheckGrounded();
        }

        private void CheckGrounded() {
            if (!GrabbedStaticObject()) {
                RaycastHit hit;
                Vector3 middleFootPosition = (leftFootTarget.transform.position + rightFootTarget.transform.position) / 2;
                Vector3 rayStart = new Vector3(middleFootPosition.x, transform.position.y + stepOffset, middleFootPosition.z);
                if (Physics.Raycast(rayStart, Vector3.down, out hit, stepOffset + 0.1F)) {
                //if (Physics.SphereCast(rayStart, 0.1F, Vector3.down, out hit, stepOffset)) {
                    if (hit.distance < stepOffset + 0.1F && !hit.collider.isTrigger) {
                        transform.position += Vector3.up * (stepOffset - hit.distance);
                        return;
                    }
                }
                transform.position -= Vector3.up * 0.02f; // should be 'falling'
            }
        }
        
        private bool GrabbedStaticObject() {
            if (leftHandMovements != null &&
                leftHandMovements.grabbedObject != null &&
                leftHandMovements.grabbedObject.isStatic == true) {
                return true;
            } else
            if (rightHandMovements != null &&
                rightHandMovements.grabbedObject != null &&
                rightHandMovements.grabbedObject.isStatic == true) {
                return true;
            }            
            return false;
        }
        

        public void Calibrate() {
            foreach (Transform t in headTarget.parent) {
                t.gameObject.SendMessage("OnTargetReset", SendMessageOptions.DontRequireReceiver);
            }
        }

        protected void AddRigidbody(GameObject gameObject) {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            if (rb != null) {
                rb.mass = 75;
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        public float walkingSpeed = 1;
        public float rotationSpeed = 60;

        public bool proximitySpeed = true;
        [Range(0.1F, 1)]
        public float proximitySpeedRate = 0.8f;
        private const float proximitySpeedStep = 0.05f;

        [HideInInspector]
        private CapsuleCollider bodyCapsule;
        [HideInInspector]
        private CapsuleCollider bodyCollider;

        public void Move(float x, float y, float z) {
            Move(new Vector3(x, y, z));
        }

        public void Move(Vector3 translationVector) {
            Move(translationVector, false);
        }

        public void Move(Vector3 translationVector, bool allowUp) {
            translationVector = CheckMovement(translationVector);// does not support body pull
            if (translationVector.magnitude > 0) {
                //Rigidbody thisRigidbody = GetComponent<Rigidbody>();
                Vector3 translation = translationVector * Time.deltaTime;
                if (allowUp) {
                    transform.position += translation;
                    //thisRigidbody.MovePosition(transform.position + translation);
                } else {
                    transform.position += new Vector3(translation.x, 0, translation.z);
                    //thisRigidbody.MovePosition(transform.position + new Vector3(translation.x, 0, translation.z));
                }
            }
        }

        public void Rotate(float angle) {
            transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
        }

        private float curProximitySpeed = 1;
        private Vector3 directionVector = Vector3.zero;

        public Vector3 CheckMovement(Vector3 inputMovement) {
            float maxAcceleration = 0;
            float sidewardSpeed = 0;
            float forwardSpeed = inputMovement.z;

            if (proximitySpeed)
                curProximitySpeed = CalculateProximitySpeed(bodyCapsule, curProximitySpeed);

            if (forwardSpeed != 0 || directionVector.z != 0) {
                if (inputMovement.z < 0)
                    forwardSpeed *= 0.6f;

                forwardSpeed *= walkingSpeed;

                if (proximitySpeed)
                    forwardSpeed *= curProximitySpeed;

                float acceleration = forwardSpeed - directionVector.z;
                maxAcceleration = 1f * Time.deltaTime;
                acceleration = Mathf.Clamp(acceleration, -maxAcceleration, maxAcceleration);
                forwardSpeed = directionVector.z + acceleration;
            }

            sidewardSpeed = inputMovement.x;

            if (sidewardSpeed != 0 || directionVector.x != 0) {

                sidewardSpeed *= walkingSpeed;

                if (proximitySpeed)
                    sidewardSpeed *= curProximitySpeed;

                float acceleration = sidewardSpeed - directionVector.x;
                maxAcceleration = 1f * Time.deltaTime;
                acceleration = Mathf.Clamp(acceleration, -maxAcceleration, maxAcceleration);
                sidewardSpeed = directionVector.x + acceleration;
            }

            directionVector = new Vector3(sidewardSpeed, 0, forwardSpeed);
            Vector3 worldDirectionVector = hipTarget.TransformDirection(directionVector);
            inputDirection = worldDirectionVector;

            if (collisions && (collided || (!proximitySpeed && triggerEntered))) {
                Debug.DrawRay(transform.position, hitNormal);
                float angle = Vector3.Angle(worldDirectionVector, hitNormal);
                if (angle > 90.1) {
                    directionVector = Vector3.zero;
                    worldDirectionVector = Vector3.zero;
                }
            }

            return worldDirectionVector;
        }

        private Vector3 curSpeed;
        private Vector3 lastRootPosition;

        private void CalculateSpeed() {
            if (lastRootPosition.magnitude > 0) {
                Vector3 deltaHip = hipTarget.position - lastRootPosition;
                curSpeed = new Vector3(deltaHip.x, 0, deltaHip.z) / Time.deltaTime;
            }
            lastRootPosition = hipTarget.position;
        }

        private float CalculateProximitySpeed(CapsuleCollider cc, float curProximitySpeed) {
            float proximitySpeedStep = 0.05F / proximitySpeedRate;

            if (triggerEntered) {
                if (cc.radius > 0.25f) {
                    // Do we need to decrease the proximity speed?
                    cc.radius -= proximitySpeedStep;
                    cc.height += proximitySpeedStep;
                    curProximitySpeed = GetProximitySpeed(cc);
                }
            } else if (curProximitySpeed < 1) {
                // Can we increase the proximity speed?
#if UNITY_5_3
                Vector3 extents = new Vector3(cc.radius + proximitySpeedStep, (cc.height - proximitySpeedStep) / 2, cc.radius + proximitySpeedStep);
                Collider[] results = Physics.OverlapBox(hipTarget.position + cc.center, extents);
#else
                Vector3 point1 = hipTarget.position + (cc.radius - 0.75F - proximitySpeedStep) * Vector3.up;
                Vector3 point2 = hipTarget.position - (cc.radius - 1.15F - proximitySpeedStep) * Vector3.up;
                Collider[] results = Physics.OverlapCapsule(point1, point2, cc.radius + proximitySpeedStep);
#endif
                bool staticCollision = false;
                for (int i = 0; i < results.Length; i++) {
                    if (results[i].attachedRigidbody == null) {
                        staticCollision = true;
                    }
                }

                if (!staticCollision) {
                    //collided = false;
                    cc.radius += proximitySpeedStep;
                    cc.height -= proximitySpeedStep;
                    curProximitySpeed = GetProximitySpeed(cc);
                }
            }

            return curProximitySpeed;
        }

        private static float GetProximitySpeed(CapsuleCollider cc) {
            return EaseIn(1, (-0.80f), 1 - cc.radius, 0.75f);
        }

        private static float EaseIn(float start, float distance, float elapsedTime, float duration) {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            //return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
            return distance * elapsedTime * elapsedTime + start;
        }

        static public CapsuleCollider AddCharacterColliders(InstantVR ivr, bool proximitySpeed) {
            Rigidbody rb = ivr.gameObject.AddComponent<Rigidbody>();
            if (rb != null) {
                rb.mass = 1;
                rb.useGravity = false;
                rb.isKinematic = true;
            }


            float avatarHeight = (ivr.headTarget.position.y - ivr.transform.position.y) + 0.3F;
            Vector3 colliderCenter = Vector3.up * (avatarHeight + ivr.stepOffset) / 2; //  (ivr.hipTarget.position - ivr.transform.position) + Vector3.up * (ivr.stepOffset / 2);
            
            ivr.bodyCollider = ivr.gameObject.AddComponent<CapsuleCollider>();
            if (ivr.bodyCollider != null) {
                ivr.bodyCollider.isTrigger = false;
                ivr.bodyCollider.height = avatarHeight - ivr.stepOffset;
                ivr.bodyCollider.radius = 0.2F;
                ivr.bodyCollider.center = colliderCenter; // new Vector3(-ivr.hipTarget.localPosition.x, 0.2F, -ivr.hipTarget.localPosition.z);
            }
            
            CapsuleCollider triggerCollider = ivr.gameObject.AddComponent<CapsuleCollider>();
            if (triggerCollider != null) {
                triggerCollider.isTrigger = true;
                if (proximitySpeed) {
                    triggerCollider.height = 0.80F;
                    triggerCollider.radius = 1F;
                } else {
                    triggerCollider.height = avatarHeight - ivr.stepOffset;
                    triggerCollider.radius = 0.4F;
                }
                triggerCollider.center = colliderCenter; // new Vector3(-ivr.hipTarget.transform.localPosition.x, 0.2F, -ivr.hipTarget.localPosition.z);
            }

            return triggerCollider;
        }

        private void PlaceBodyCapsule() {
            if (collisions) {
                Vector3 colliderPosition = Quaternion.Inverse(transform.rotation) * (hipTarget.position - transform.position);
                bodyCapsule.center = new Vector3(colliderPosition.x, bodyCapsule.center.y, colliderPosition.z);
                bodyCollider.center = new Vector3(colliderPosition.x, bodyCollider.center.y, colliderPosition.z);
            }
        }

        private void DetermineCollision() {
            if (proximitySpeed) {
                if (triggerEntered && bodyCapsule.radius <= 0.25f) {
                    collided = true;
                    hitNormal = -curSpeed.normalized;
                } else {
                    collided = false;
                }
            } else {
                if (triggerEntered) {
                    collided = true;
                    hitNormal = -curSpeed.normalized;
                } else {
                    collided = false;
                }
            }
        }


        void OnCollisionEnter(Collision collision) { 
            if (collision.rigidbody == null) {
                collided = true;
                hitNormal = -curSpeed.normalized;
                /*
                Rigidbody thisRigidbody = GetComponent<Rigidbody>();
                Debug.Log(thisRigidbody.velocity);
                for (int i = 0; i < collision.contacts.Length; i++) {
                    if (collision.contacts[i].normal.magnitude > 0)
                        hitNormal = collision.contacts[0].normal;
                }
                */
            }
        }

        void OnCollisionExit() {
            collided = false;
        }

        void OnTriggerStay(Collider otherCollider) {
            Rigidbody rigidbody = otherCollider.attachedRigidbody;
            if (rigidbody == null && !otherCollider.isTrigger)
                triggerEntered = true;
        }

        void OnTriggerEnter(Collider otherCollider) {
            Rigidbody rigidbody = otherCollider.attachedRigidbody;
            if (rigidbody == null && !otherCollider.isTrigger)
                triggerEntered = true;
            /*
            if (!collided && !triggerEntered) {
                triggerEntered = false;
                Vector3 dir = inputDirection;
                if (dir.magnitude > 0) {
                    CapsuleCollider cc = gameObject.GetComponent<CapsuleCollider>();
                    Vector3 capsuleCenter = hipTarget.position + cc.center;
                    Vector3 capsuleOffset = ((cc.height - cc.radius) / 2) * Vector3.up;

                    
                    Vector3 top = capsuleCenter + capsuleOffset;
                    Vector3 bottom = capsuleCenter - capsuleOffset;
                    RaycastHit[] hits = Physics.CapsuleCastAll(top, bottom, cc.radius, inputDirection, 0.05F);
                    for (int i = 0; i < hits.Length && triggerEntered == false; i++) {
                        if (hits[i].rigidbody == null && !hits[i].collider.isTrigger) {
                            triggerEntered = true;
                            hitNormal = hits[i].normal;
                        }
                    }
                    
                }
            }
            */
        }

        void OnTriggerExit() {
            triggerEntered = false;
        }
    }
}