using System.Collections.Generic;
using UnityEngine;

namespace LegTracking.HandTracking
{
    public class RagdollHandTracker : MonoBehaviour
    {
        [System.Serializable]
        public class RagdollBoneLink
        {
            public string targetBoneName;
            public Rigidbody ragdollBody;

            [HideInInspector] public Transform targetBone;
            [HideInInspector] public bool isValid;
            [HideInInspector] public Vector3 startLocalPosition;
        }
        
        [Header("Target / XR Hand")]
        [SerializeField] private OVRSkeleton targetSkeleton;
        
        [Header("Ragdoll Bone Links")]
        [SerializeField] private List<RagdollBoneLink> boneLinks = new List<RagdollBoneLink>();
        
        [Header("Root Drive")]
        [SerializeField] private Rigidbody rootBody;
        [SerializeField] private string rootTargetBoneName = "XRHand_Palm";

        [Header("Root Velocity Follow")]
        [SerializeField] private Vector3 rootPositionOffset = Vector3.zero;
        [SerializeField] private float rootPositionSpeed = 80f;
        [SerializeField] private float rootRotationSpeed = 30f;
        
        [Header("Finger Rotation Drive")]
        [SerializeField] private float fingerRotationSpeed = 25f;
        [SerializeField] private float maxFingerAngularVelocity = 40f;

        private Transform rootTargetBone;
        private bool rootTargetReady;

        private bool boneLinksReady;

        private Dictionary<string, Transform> targetBonesByName = new Dictionary<string, Transform>();
        private bool targetBonesReady;
        
        private void Start()
        {
            BuildTargetBoneDictionary();
            BindRagdollBonesToTargets();
            CacheRagdollBoneStartLocalPositions();
            BindRootTargetBone();
            
            foreach (var link in boneLinks)
            {
                if (link.ragdollBody != null)
                    link.ragdollBody.maxAngularVelocity = maxFingerAngularVelocity;
            }
        }
        
        private void FixedUpdate()
        {
            DriveRootToTarget();
            RestoreRagdollBoneLocalPositions();
            DriveRagdollBoneRotations();
        }
        
        private void BuildTargetBoneDictionary()
        {
            targetBonesByName.Clear();
            targetBonesReady = false;

            if (targetSkeleton == null)
            {
                Debug.LogWarning("Target skeleton is null.");
                return;
            }

            if (!targetSkeleton.IsInitialized)
            {
                Debug.LogWarning("Target skeleton is not initialized yet.");
                return;
            }

            foreach (var bone in targetSkeleton.Bones)
            {
                if (bone.Transform == null)
                    continue;

                string boneName = bone.Transform.name;

                if (!targetBonesByName.ContainsKey(boneName))
                {
                    targetBonesByName.Add(boneName, bone.Transform);
                }
            }

            targetBonesReady = targetBonesByName.Count > 0;

            Debug.Log("Target bone dictionary built. Bone count: " + targetBonesByName.Count);
        }
        
        private void BindRagdollBonesToTargets()
        {
            boneLinksReady = false;

            if (!targetBonesReady)
            {
                Debug.LogWarning("Target bones are not ready.");
                return;
            }

            int validCount = 0;

            foreach (var link in boneLinks)
            {
                link.isValid = false;
                link.targetBone = null;

                if (string.IsNullOrEmpty(link.targetBoneName))
                {
                    Debug.LogWarning("Bone link has empty target bone name.");
                    continue;
                }

                if (link.ragdollBody == null)
                {
                    Debug.LogWarning("Bone link has null ragdoll body: " + link.targetBoneName);
                    continue;
                }

                if (!targetBonesByName.TryGetValue(link.targetBoneName, out Transform targetBone))
                {
                    Debug.LogWarning("Target bone not found: " + link.targetBoneName);
                    continue;
                }

                link.targetBone = targetBone;
                link.isValid = true;

                validCount++;
            }

            boneLinksReady = validCount > 0;

            Debug.Log("Ragdoll bone links bound. Valid count: " + validCount);
        }
        
        private void BindRootTargetBone()
        {
            rootTargetReady = false;
            rootTargetBone = null;

            if (!targetBonesReady)
            {
                Debug.LogWarning("Target bones are not ready for root binding.");
                return;
            }

            if (rootBody == null)
            {
                Debug.LogWarning("Root body is null.");
                return;
            }

            if (!targetBonesByName.TryGetValue(rootTargetBoneName, out rootTargetBone))
            {
                Debug.LogWarning("Root target bone not found: " + rootTargetBoneName);
                return;
            }

            rootTargetReady = true;

            Debug.Log("Root target bone bound: " + rootTargetBoneName);
        }
        
        private void CacheRagdollBoneStartLocalPositions()
        {
            if (!boneLinksReady)
                return;

            foreach (var link in boneLinks)
            {
                if (!link.isValid)
                    continue;

                link.startLocalPosition = link.ragdollBody.transform.localPosition;
            }

            Debug.Log("Ragdoll bone start local positions cached.");
        }
        
        private void RestoreRagdollBoneLocalPositions()
        {
            if (!boneLinksReady)
                return;

            foreach (var link in boneLinks)
            {
                if (!link.isValid)
                    continue;

                link.ragdollBody.transform.localPosition = link.startLocalPosition;
                //link.ragdollBody.linearVelocity = Vector3.zero;
            }
        }
        
        private void DriveRootToTarget()
        {
            if (!rootTargetReady)
                return;

            FollowTransformVelocity(
                rootBody,
                rootTargetBone,
                rootPositionOffset,
                rootPositionSpeed,
                rootRotationSpeed
            );
        }
        
        private void DriveRagdollBoneRotations()
        {
            if (!boneLinksReady)
                return;

            foreach (var link in boneLinks)
            {
                if (!link.isValid)
                    continue;

                Rigidbody rb = link.ragdollBody;

                Quaternion targetRotation =
                    link.targetBone.rotation;

                FollowRotationVelocity(
                    rb,
                    targetRotation,
                    fingerRotationSpeed
                );
            }
        }
        
        private static void FollowRotationVelocity(
            Rigidbody rb,
            Quaternion targetRotation,
            float rotationSpeed
        )
        {
            Quaternion rotationError = targetRotation * Quaternion.Inverse(rb.rotation);

            if (rotationError.w < 0f)
            {
                rotationError.x = -rotationError.x;
                rotationError.y = -rotationError.y;
                rotationError.z = -rotationError.z;
                rotationError.w = -rotationError.w;
            }

            rotationError.ToAngleAxis(out float angleDeg, out Vector3 axis);

            if (angleDeg > 180f)
                angleDeg -= 360f;

            if (axis.sqrMagnitude < 0.0001f || Mathf.Abs(angleDeg) < 0.01f)
            {
                rb.angularVelocity = Vector3.zero;
                return;
            }

            rb.angularVelocity =
                axis.normalized * (angleDeg * Mathf.Deg2Rad * rotationSpeed);
        }
        public static void FollowTransformVelocity(
            Rigidbody rb,
            Transform target,
            Vector3 offset,
            float positionSpeed,
            float rotationSpeed
        )
        {
            var worldOffset = target.TransformDirection(offset);
            var positionError = target.position + worldOffset - rb.position;
            rb.linearVelocity = positionError * positionSpeed;

            Quaternion rotationError = target.rotation * Quaternion.Inverse(rb.rotation);
            rotationError.ToAngleAxis(out float angleDeg, out Vector3 axis);

            if (angleDeg > 180f)
                angleDeg -= 360f;

            rb.angularVelocity = axis.normalized * (angleDeg * Mathf.Deg2Rad * rotationSpeed);
        }
    }
    
    // 0- Body_Start XRHand_Palm
    // 1- Body_Hips XRHand_Wrist
    // 2- Body_SpineLower XRHand_ThumbMetacarpal
    // 3- Body_SpineMiddle XRHand_ThumbProximal
    // 4- Hand_Thumb2 XRHand_ThumbDistal
    // 5- Body_Chest XRHand_ThumbTip
    // 6- Body_Neck XRHand_IndexMetacarpal
    // 7- Hand_Index2 XRHand_IndexProximal
    // 8- Hand_Index3 XRHand_IndexIntermediate
    // 9- Hand_Middle1 XRHand_IndexDistal
    // 10- FullBody_LeftArmUpper XRHand_IndexTip
    // 11- XRHand_MiddleMetacarpal XRHand_MiddleMetacarpal
    // 12- Hand_Ring1 XRHand_MiddleProximal
    // 13- Hand_Ring2 XRHand_MiddleIntermediate
    // 14- XRHand_MiddleDistal XRHand_MiddleDistal
    // 15- XRHand_MiddleTip XRHand_MiddleTip
    // 16- Hand_Pinky1 XRHand_RingMetacarpal
    // 17- Body_RightHandWristTwist XRHand_RingProximal
    // 18- FullBody_LeftHandPalm XRHand_RingIntermediate
    // 19- XRHand_RingDistal XRHand_RingDistal
    // 20- Body_LeftHandThumbMetacarpal XRHand_RingTip
    // 21- Hand_MiddleTip XRHand_LittleMetacarpal
    // 22- FullBody_LeftHandThumbDistal XRHand_LittleProximal
    // 23- Body_LeftHandThumbTip XRHand_LittleIntermediate
    // 24- Body_LeftHandIndexMetacarpal XRHand_LittleDistal
    // 25- XRHand_LittleTip XRHand_LittleTip
}