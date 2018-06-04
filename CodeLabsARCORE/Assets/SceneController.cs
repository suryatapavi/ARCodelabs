﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class SceneController : MonoBehaviour {

    public GameObject trackedPlanePrefab;
    public Camera firstPersonCamera;
    public ScoreboardController scoreboard;
    public SnakeController snakeController;

    void QuitOnConnectionErrors()
    {
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            StartCoroutine(CodelabUtils.ToastAndExit("Camera access denied", 5));
        }
        else if (Session.Status.IsError())
        {
            StartCoroutine(CodelabUtils.ToastAndExit("ArCORE not connected",5));
        }
    }



    void ProcessNewPlanes()
    {
        List<TrackedPlane> planes = new List<TrackedPlane>();
        Session.GetTrackables(planes, TrackableQueryFilter.New);

        for (int i = 0; i < planes.Count; i++)
        {
            // Instantiate a plane visualization prefab and set it to track the new plane.
            // The transform is set to the origin with an identity rotation since the mesh
            // for our prefab is updated in Unity World coordinates.
            GameObject planeObject = Instantiate(trackedPlanePrefab, Vector3.zero,
                    Quaternion.identity, transform);
            planeObject.GetComponent<TrackedPlaneController>().SetTrackedPlane(planes[i]);
        }
    }


    void ProcessTouches()
    {
        Touch touch;
        if (Input.touchCount != 1 ||
            (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        TrackableHit hit;
        TrackableHitFlags raycastFilter =
            TrackableHitFlags.PlaneWithinBounds |
            TrackableHitFlags.PlaneWithinPolygon;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            SetSelectedPlane(hit.Trackable as TrackedPlane);
        }
    }

        void SetSelectedPlane(TrackedPlane selectedPlane)
        {
            Debug.Log("Selected plane centered at " + selectedPlane.CenterPose.position);
        // Add to the end of SetSelectedPlane.
           scoreboard.SetSelectedPlane(selectedPlane);
           snakeController.SetPlane(selectedPlane);
        GetComponent<FoodController>().SetSelectedPlane(selectedPlane);
    }

    // Use this for initialization
    void Start () {
        QuitOnConnectionErrors();

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            int lostTrackingSleepTimeOut = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeOut;
            return;
        }
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ProcessNewPlanes();
        ProcessTouches();
        scoreboard.SetScore(snakeController.GetLength());
    }
}
