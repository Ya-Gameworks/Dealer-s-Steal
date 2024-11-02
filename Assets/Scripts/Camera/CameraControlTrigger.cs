using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public enum PanDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObjects customInspectorObjects;
    private Collider2D collider2D;

    private void Start()
    {
        collider2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (customInspectorObjects.PanCameraOnContact)
            {
                CameraManager.Instance.PanCamera(customInspectorObjects.PanDistance,customInspectorObjects.PanTime,customInspectorObjects.panDirection,false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (customInspectorObjects.SwapCameras && customInspectorObjects.cameraOnLeft && customInspectorObjects.cameraOnRight)
            {
                Vector2 exitDirection = (collider.transform.position - collider2D.bounds.center).normalized;
                //swap cameras
                CameraManager.Instance.SwapCamera(customInspectorObjects.cameraOnLeft, customInspectorObjects.cameraOnRight,exitDirection);
            }
            
            if (customInspectorObjects.PanCameraOnContact)
            {
                CameraManager.Instance.PanCamera(customInspectorObjects.PanDistance,customInspectorObjects.PanTime,customInspectorObjects.panDirection,true);
            }
        }
    }
}
[System.Serializable]
public class CustomInspectorObjects
{
    public bool SwapCameras = false;
    public bool PanCameraOnContact = false;
    
    [HideInInspector] public CinemachineCamera cameraOnLeft;
    [HideInInspector] public CinemachineCamera cameraOnRight;
    
    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float PanDistance = 3f;
    [HideInInspector] public float PanTime = .35f;
}
#if UNITY_EDITOR
[CustomEditor(typeof(CameraControlTrigger))]
public class CameraControlTriggerEditor : Editor
{
    CameraControlTrigger CameraControlTrigger;

    private void OnEnable()
    {
        CameraControlTrigger = (CameraControlTrigger)target;
    }

    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (CameraControlTrigger.customInspectorObjects.SwapCameras)
        {
            CameraControlTrigger.customInspectorObjects.cameraOnLeft = EditorGUILayout.ObjectField("Camera On Left", CameraControlTrigger.customInspectorObjects.cameraOnLeft, typeof(CinemachineCamera), true) as CinemachineCamera;
            CameraControlTrigger.customInspectorObjects.cameraOnRight = EditorGUILayout.ObjectField("Camera On Right", CameraControlTrigger.customInspectorObjects.cameraOnRight, typeof(CinemachineCamera), true) as CinemachineCamera;
        }

        if (CameraControlTrigger.customInspectorObjects.PanCameraOnContact)
        {
            CameraControlTrigger.customInspectorObjects.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Pan direction", CameraControlTrigger.customInspectorObjects.panDirection);
            CameraControlTrigger.customInspectorObjects.PanDistance = EditorGUILayout.FloatField("Pan distance", CameraControlTrigger.customInspectorObjects.PanDistance);
            CameraControlTrigger.customInspectorObjects.PanTime = EditorGUILayout.FloatField("Pan duration", CameraControlTrigger.customInspectorObjects.PanTime);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(CameraControlTrigger);
        }
    }
}
#endif
