using System;
using UnityEngine;

namespace LegTracking.FootTracking
{
    public class FootTracker : MonoBehaviour
    {
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform trackerTransform;
        [SerializeField] private Transform footTransform;
        [SerializeField] private Transform bodyLegTransform;
        [SerializeField] private float footDistance = 0.05f;
        [SerializeField] private float rootOffsetZ;
        [SerializeField] private float rootOffsetX;
        [SerializeField] private float rootOffsetY;

        private Vector3 _footLocalPosition;
        private float _rootOffset;
        private bool _isInitialized = false;
        
        private void Start()
        {
            Invoke(nameof(SetupFoot), 2f);
        }

        private void SetupFoot()
        {
            var footDistanceToBodyLeg = bodyLegTransform.position.y - footTransform.position.y;
            
            var bodyLegPos = headTransform.position;
            bodyLegPos.y = footDistanceToBodyLeg + footDistance;
            
            _rootOffset = headTransform.position.y - bodyLegPos.y;
            
            bodyLegTransform.position = bodyLegPos;
            
            var footTransformRawPos = footTransform.position;
            footTransformRawPos.y = footDistance;
            footTransform.position = footTransformRawPos;
            
            _footLocalPosition = trackerTransform.InverseTransformPoint(footTransform.position);
            
            _isInitialized = true;
        }
        private void Update()
        {
            if(!_isInitialized) return;
            var forward = headTransform.forward;
            forward.y = 0;
            forward.Normalize();
            var right = headTransform.right;
            right.y = 0;
            right.Normalize();
            var up = Vector3.Cross(forward, right);
            var bodyLegPos = headTransform.position;
            bodyLegPos.y -= _rootOffset;
            var offset = forward * rootOffsetZ + right * rootOffsetX + up * rootOffsetY;
            bodyLegPos += offset;
            bodyLegTransform.position = bodyLegPos;
            
            bodyLegTransform.eulerAngles = new Vector3(bodyLegTransform.eulerAngles.x, headTransform.eulerAngles.y,  bodyLegTransform.eulerAngles.z);
            
            var footForward = footTransform.forward;
            footForward.y = 0;
            footForward.Normalize();
            var footRight = footTransform.right;
            footRight.y = 0;
            footRight.Normalize();
            var footUp = Vector3.Cross(footForward, footRight);
            var footOffset = footForward * rootOffsetZ + footRight * rootOffsetX + footUp * rootOffsetY;
            footTransform.position = trackerTransform.TransformPoint(_footLocalPosition) + footOffset;
            

        }
    }
}