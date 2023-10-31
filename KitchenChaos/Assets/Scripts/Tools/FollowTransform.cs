using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform targetTransform;

    public void SetTargetTransform(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
    }

    private void LateUpdate()
    {
        if(targetTransform == null)
            return;

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    }
    public void ResetTarget()
    {
        targetTransform = null;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
