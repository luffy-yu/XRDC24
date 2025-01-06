using Fragilem17.MirrorsAndPortals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.XR;
#endif
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class PortalGunExample : MonoBehaviour
{
    public LayerMask layerMask;
    public Portal portal1;
    public Portal portal2;

    private Dictionary<Portal, Vector3> _portalSizes;

    private Portal _targetPortal;

    public new Light light;

    private bool _shooting = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    public InputAction ShootAction1;
    public InputAction ShootAction2;

    private void OnEnable()
    {
        ShootAction1.Enable();
        ShootAction2.Enable();
    }

    private void OnDisable()
    {
        ShootAction1.Disable();
        ShootAction2.Disable();
    }
#endif


#if ENABLE_LEGACY_INPUT_MANAGER
    private UnityEngine.XR.InputDevice _rightHandDevice;

    void OnEnable()
    {
        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);
        foreach (InputDevice device in allDevices)
            InputDevices_deviceConnected(device);

        InputDevices.deviceConnected += InputDevices_deviceConnected;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        bool discardedValue;
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out discardedValue))
        {
            if ((device.characteristics & InputDeviceCharacteristics.Right) == InputDeviceCharacteristics.Right)
            {
                _rightHandDevice = device;
            }
        }
    }
#endif

    private void Start()
    {
        _portalSizes = new Dictionary<Portal, Vector3>();
		if (portal1 && portal2)
		{
            _portalSizes[portal1] = portal1.transform.localScale;
            _portalSizes[portal2] = portal2.transform.localScale;
		}
    }

    // Update is called once per frame
    void Update()
    {
        _targetPortal = portal1;

        bool shouldShoot = false;


#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (!_shooting && ShootAction1.ReadValue<float>() > 0)
        {
            shouldShoot = true;
            _shooting = true;
        }
        if (!_shooting && ShootAction2.ReadValue<float>() > 0)
        {
            shouldShoot = true;
            _shooting = true;
            _targetPortal = portal2;
        }

        if (_shooting && ShootAction1.ReadValue<float>() == 0 && ShootAction2.ReadValue<float>() == 0)
        {
            _shooting = false;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER

        bool buttonValue;
        _rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue);
        //Debug.Log("buttonValue: " + buttonValue);

        if (!_shooting && (Input.GetMouseButton(0) || (_rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue)))
        {
            shouldShoot = true;
            _shooting = true;
        }
        if (!_shooting && (Input.GetMouseButton(1) || (_rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonValue) && buttonValue)))
        {
            shouldShoot = true;
            _shooting = true;
            _targetPortal = portal2;
        }

        if (_shooting && !(Input.GetMouseButton(0) || Input.GetMouseButton(1) || (_rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue) || (_rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonValue) && buttonValue)))
        {
            _shooting = false;
        }
#endif

        if (shouldShoot)
        {
            Debug.DrawRay(transform.position, transform.forward);
            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out hit, 40, layerMask))
            {
                if (hit.collider)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeInOut());

                    _targetPortal.transform.position = hit.point;
                    _targetPortal.transform.rotation = Quaternion.FromToRotation(Vector3.back, hit.normal);
                    _targetPortal.transform.localScale = Vector3.zero;
                    _targetPortal.wallCollider = hit.collider.gameObject.GetComponentInChildren<Collider>();

                    Vector3 euler = _targetPortal.transform.rotation.eulerAngles;
                    _targetPortal.transform.rotation = Quaternion.Euler(euler.x, euler.y, 0);

                    _targetPortal.transform.localScale = Vector3.zero;
                    StartCoroutine(Scale(_targetPortal, 1));
                }
            }
        }
    }

    IEnumerator FadeInOut()
    {
        if (light != null)
        {
            light.enabled = true;
            yield return StartCoroutine(Fade(5f));
            yield return StartCoroutine(Fade(0));
            light.enabled = false;
        }
    }
    IEnumerator Fade(float to)
    {
        float diff = (to - light.intensity);
        while (Mathf.Abs(diff) > 0.01f)
        {
            //Debug.Log("diff " + diff);
            light.intensity += diff * 0.5f;
            diff = (to - light.intensity);
            yield return null;
        }
    }

    IEnumerator Scale(Portal portal, float to)
    {
        float diff = ((to * _portalSizes[portal].x) - portal.transform.localScale.x);
        while (Mathf.Abs(diff) > 0.01f)
        {
            //Debug.Log("diff " + diff);
            portal.transform.localScale += _portalSizes[portal] * diff * 0.1f;
            diff = ((to * _portalSizes[portal].x) - portal.transform.localScale.x);
            yield return null;
        }
    }
}
