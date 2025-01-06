using UnityEngine;

public class PortalCameraController : MonoBehaviour
{
    public Transform playerCamera; // 玩家主摄像机
    public Transform portal;       // 当前传送门
    public Transform otherPortal;  // 对应的传送门（出口）

    private Camera portalCamera;

    void Start()
    {
        portalCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // 计算相对于传送门的摄像机位置
        Vector3 relativePos = portal.InverseTransformPoint(playerCamera.position);
        portalCamera.transform.position = otherPortal.TransformPoint(relativePos);

        // 计算相对于传送门的摄像机方向
        Quaternion relativeRot = Quaternion.Inverse(portal.rotation) * playerCamera.rotation;
        portalCamera.transform.rotation = otherPortal.rotation * relativeRot;
    }
}
