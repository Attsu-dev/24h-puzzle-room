using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtController : MonoBehaviour {

    Animator animator;
    Vector3 targetPos;

    void Start () {
        this.animator = GetComponent<Animator> ();    
        this.targetPos = Camera.main.transform.position;
    }
    
    void Update () {
        Vector3 touchPos = Camera.main.ScreenPointToRay (Input.mousePosition).origin;
        touchPos.z = -0.5f;
        targetPos = touchPos;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        this.animator.SetLookAtWeight(1.0f, 0.8f, 1.0f, 0.0f, 0f);
        this.animator.SetLookAtPosition(this.targetPos);
    }
}