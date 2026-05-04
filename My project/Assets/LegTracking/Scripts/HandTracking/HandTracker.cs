using System;
using UnityEngine;

namespace LegTracking.HandTracking
{
    public class HandTracker : MonoBehaviour
    {
        [SerializeField] private Vector3 offset;
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Rigidbody rightHandRigidbody;
        [SerializeField] private Rigidbody leftHandRigidbody;
        [SerializeField] private float positionSpeed = 1f;
        [SerializeField] private float rotationSpeed = 1f;

        private void FixedUpdate()
        {
            var rightLocalOffset = rightHandTransform.TransformDirection(offset);
            var leftLocalOffset = leftHandTransform.TransformDirection(offset);
            FollowTransformVelocity(rightHandRigidbody, rightHandTransform, rightLocalOffset, positionSpeed, rotationSpeed);
            FollowTransformVelocity(leftHandRigidbody, leftHandTransform, leftLocalOffset, positionSpeed, rotationSpeed);
        }
        public static void FollowTransformVelocity(
            Rigidbody rb,
            Transform target,
            Vector3 offset,
            float positionSpeed,
            float rotationSpeed
        )
        {
            var positionError = target.position + offset - rb.position;
            rb.linearVelocity = positionError * positionSpeed;

            Quaternion rotationError = target.rotation * Quaternion.Inverse(rb.rotation);
            rotationError.ToAngleAxis(out float angleDeg, out Vector3 axis);

            if (angleDeg > 180f)
                angleDeg -= 360f;

            rb.angularVelocity = axis.normalized * angleDeg * Mathf.Deg2Rad * rotationSpeed;
        }
    }
}