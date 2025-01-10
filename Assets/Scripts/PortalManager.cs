using UnityEngine;
using System.Collections.Generic;

public struct PortalSet
{
    public GameObject portal;
    public Camera camera;
    public RenderTexture rt;

    public PortalSet(GameObject _portal, Camera _camera, RenderTexture _rt)
    {
        portal = _portal;
        camera = _camera;
        rt = _rt;

        // assemble the render texture to the render camera
        if (camera.GetComponent<Camera>().targetTexture != null)
            camera.GetComponent<Camera>().targetTexture.Release();
        camera.GetComponent<Camera>().targetTexture = rt;

        // assemble the render texture to the portal
        MeshRenderer renderer = portal.transform.GetChild(0).GetComponent<MeshRenderer>();
        Material mat = renderer.material;
        mat.SetTexture("_TexRight", rt);
        mat.SetTexture("_TexLeft", rt);
    }
}

public class PortalManager : MonoBehaviour
{
    [SerializeField] Camera m_CenterEyeCamera;
    public GameObject m_PortalPrefab;
    public GameObject m_RenderCamPrefab;

    private List<PortalSet> portals = new List<PortalSet>();
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnPortal(Vector3 pos, Quaternion rot)
    {
        // init render camera with texture
        GameObject cam = Instantiate(m_RenderCamPrefab, m_CenterEyeCamera.transform, false);
        RenderTexture rt = new RenderTexture(Constants.RT_WIDTH, Constants.RT_WIDTH, 24);
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.useMipMap = false;

        // init portal
        GameObject portal = Instantiate(m_PortalPrefab, pos, rot);

        PortalSet portalSet = new PortalSet(portal, cam.GetComponent<Camera>(), rt);
        portals.Add(portalSet);
    }

    public void ClearPortals()
    {
        if (portals.Count <= 0)
            return;

        foreach (PortalSet portal in portals)
        {
            Destroy(portal.portal);
            Destroy(portal.camera.gameObject);
            portal.rt.Release();
        }

        portals.Clear();
    }


}
