using UnityEngine;

public class BodyTracker : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform hipsTransform;

    [SerializeField] private float rootOffsetZ = 0f;
    [SerializeField] private float rootOffsetY = -0.9f;

    private float _initialHeadToHipsY;

    private void Start()
    {
        _initialHeadToHipsY = headTransform.position.y - hipsTransform.position.y;
    }

    private void LateUpdate()
    {
        var forward = headTransform.forward;
        forward.y = 0;
        forward.Normalize();

        var targetPos = headTransform.position;
        targetPos += forward * rootOffsetZ;

        targetPos.y = headTransform.position.y - _initialHeadToHipsY + rootOffsetY;

        hipsTransform.position = targetPos;

        hipsTransform.rotation = Quaternion.Euler(
            hipsTransform.eulerAngles.x,
            headTransform.eulerAngles.y,
            hipsTransform.eulerAngles.z
        );
    }
}