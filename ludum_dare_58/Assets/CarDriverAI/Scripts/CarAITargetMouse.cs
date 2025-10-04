using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAITargetMouse : MonoBehaviour {

    [SerializeField] private Transform targetTransform;

    private bool isFollowing = false;
    public  GameObject target;
    

    private void Update() {
        if (isFollowing) {
            targetTransform.position = target.transform.position;
        }

        if (Input.GetMouseButtonDown(0)) {
            isFollowing = !isFollowing;
        }
    }

}
