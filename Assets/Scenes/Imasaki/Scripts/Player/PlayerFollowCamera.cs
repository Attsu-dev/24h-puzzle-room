using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float high = 1.5f;
    [SerializeField] private float distance = 0;
    [SerializeField] private Quaternion verticalRotation;
    [SerializeField] public Quaternion horizontalRotation;

    public Transform Player
    {
        get => player;
        set
        {
            player = value;
            Initialize();
        }
    }

    void Start()
    {
        if (player != null)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        verticalRotation = Quaternion.Euler(0, 0, 0);
        horizontalRotation = Quaternion.identity;

        UpdateRotationPosition();
    }


    void LateUpdate()
    {
        if (player == null) return;

        UpdateRotationPosition();
    }

    private void UpdateRotationPosition()
    {
        var transform1 = transform;
        transform1.rotation = player.rotation;
        transform1.position = player.position + new Vector3(0, high, 0) - transform1.rotation * Vector3.forward * distance;
    }
}