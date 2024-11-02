using System;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that locks the camera's Y co-ordinate
/// </summary>
[SaveDuringPlay] [AddComponentMenu("")] // Hide in menu
public class LockCameraZ : CinemachineExtension
{
    [Tooltip("Lock the camera's Z position to this value")]
    private float m_ZPosition = -13.59043f;

    public void Start()
    {
        m_ZPosition = transform.localPosition.z;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize)
        {
            var pos = state.RawPosition;
            pos.z = m_ZPosition;
            state.RawPosition = pos;
        }
    }
}