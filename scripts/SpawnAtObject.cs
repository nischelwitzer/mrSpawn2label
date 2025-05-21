using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Meta.XR.MRUtilityKit;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Drawing;

// https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-gs
// XR_Room.cs und XR_RoomMap.cs
// C:\Data\1_dev\1_unity\2025_habitat\CoSA_MR_Habitat\Assets\Scripts\XR\Details


// Author: AK Nischelwitzer
// Date: 2025-05-18
//
// Nomanklatur: 
// unityCenter - (0,0,0) = UnityZero = MRUK StartPoint
// realRoomCenter - from FloorAnchor, used for calculation of the roomZero (farest corner from the 4 floor SCREEN Anchor points)
// roomZero - farest corner from the SCREEN Anchor, used for the MRUKZeroTransform

namespace Meta.XR.MRUtilityKit
{
    public class TransformUnityZero2MRUK : MonoBehaviour
    {
        [SerializeField, Tooltip("Prefab to be placed into the scene, or object in the scene to be moved around.")]
        public GameObject gizmoSceenCenterObject;  
        public GameObject cornerScreenObject; // 8 SCREEN corner objects 
        public GameObject centerObject; // for floorCenter and SCREENcenter(on ground)

        [SerializeField, Tooltip("When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.")]
        public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;

        // MRUK Anchors
        public MRUKAnchor screenAnchor { get; private set; }

        // MRUK Positions
        public float FloorLevel { get; private set; }
        public float CeilingLevel { get; private set; }

        public LineRenderer lineRendererObject;

        // Parameters for the translation and rotation
        private Vector2 transformPosition = new Vector2(0, 0); // translation vector
        private float transformRotation = 0;                   // rotation angle in degrees

        void Start()
        {
            Debug.Log("***** MRUKZERO: waiting for MRUK Scene to be ready...");
            Debug.Log("***** MRUK Version: " + typeof(MRUK).Assembly.GetName().Version);
        }

        public void StartSpawn()
        {
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            if (room == null)
            {
                Debug.LogError("***** MRUKZERO: No room found to spawn in.");
            }

            Vector3 floorZero = MRUK.Instance.GetCurrentRoom().FloorAnchor.transform.position;
            FloorLevel = floorZero.y;
            CeilingLevel = MRUK.Instance.GetCurrentRoom().CeilingAnchor.transform.position.y;
            Debug.Log("***** MRUKZERO: FloorZero: " + floorZero);
            Debug.Log("***** MRUKZERO: FloorLevel: " + FloorLevel + " CeilingLevel: " + CeilingLevel + "  RoomHeight: " + (CeilingLevel- FloorLevel));

            // build a list of all anchors with the label SCREEN
            List<MRUKAnchor> allScreensAnchors = new();
            foreach (MRUKAnchor anchor in MRUK.Instance.GetCurrentRoom().Anchors)
            {
                if (anchor.Label == MRUKAnchor.SceneLabels.SCREEN) allScreensAnchors.Add(anchor);
            }

            // Calculate validation data
            if (allScreensAnchors.Count == 1) // only one SCREEN Anchor is allowed 
            {
                // screenAnchor = MRUK.Instance.GetCurrentRoom().FindLargestSurface(MRUKAnchor.SceneLabels.SCREEN);

                // ------------------------------------------------------------------------------------------------------
                // SCREEN Anchor
                // get MAIN postion INFOS

                Vector3 screenPositionZero    = allScreensAnchors[0].transform.position; // center for roomZero calculation
                Quaternion screenRotationZero = allScreensAnchors[0].transform.rotation;
                Vector3 screenCenterZero      = allScreensAnchors[0].GetAnchorCenter();      
                Vector3 screenExtents         = allScreensAnchors[0].VolumeBounds.Value.extents; // for finding the 8 corners of the screen

                // SpawnObject.transform.localPosition = ScreenCenter;
                // Vector3 worldPoint = ScreenAnchor.transform.TransformPoint(localPoint);
                // ------------------------------------------------------------------------------------------------------
                // set GIZMO to SCREEN Center

                gizmoSceenCenterObject.transform.position = screenCenterZero;
                GameObject.Find("TransformInScreen").transform.position = screenCenterZero;
                GameObject.Find("RotationInScreen").transform.rotation = screenRotationZero;
                Debug.Log("***** MRUKZERO: SCREEN-Anchor position " + screenPositionZero + " " + screenCenterZero); // second smaller on Y
                Debug.Log("***** MRUKZERO: SCREEN-Anchor rotation " + screenRotationZero + " " + screenRotationZero + " Euler:" + screenRotationZero.eulerAngles);

                // ------------------------------------------------------------------------------------------------------
                // DRAW CORNER
                // draw the center floor object "basket ball"

                Transform cornerParentRotation    = GameObject.Find("zeroScreenRotationCornerPoints").transform;
                Transform cornerParentTranslation = GameObject.Find("zeroScreenTransformCornerPoints").transform;
                Vector3 screenCenterFloor = new(screenCenterZero.x, FloorLevel, screenCenterZero.z);
                Instantiate(centerObject, Vector3.zero, Quaternion.identity, cornerParentTranslation);
                
                Debug.Log("***** MRUKZERO: ScreenCenterFloor " + screenCenterFloor);
                // CORNER OBJECTS calc corners
                Debug.Log("***** MRUKZERO: SCREEN-Extents: " + screenExtents + " Vol:" + screenAnchor.VolumeBounds.Value.center);

                // create 8 corners of the screen and center
                // MRUK XZY 
                GameObject[] cornerPoints = new GameObject[8];
                cornerPoints[0] = Instantiate(cornerScreenObject, new(-screenExtents.x, 0, -screenExtents.y), Quaternion.identity, cornerParentRotation);
                cornerPoints[1] = Instantiate(cornerScreenObject, new(screenExtents.x, 0, -screenExtents.y), Quaternion.identity, cornerParentRotation);
                cornerPoints[2] = Instantiate(cornerScreenObject, new(-screenExtents.x, 0, screenExtents.y), Quaternion.identity, cornerParentRotation);
                cornerPoints[3] = Instantiate(cornerScreenObject, new(screenExtents.x, 0, screenExtents.y), Quaternion.identity, cornerParentRotation);
                cornerPoints[4] = Instantiate(cornerScreenObject, new(-screenExtents.x, 2 * screenExtents.z, -screenExtents.y), Quaternion.identity, cornerParentRotation);
                cornerPoints[5] = Instantiate(cornerScreenObject, new(screenExtents.x, 2 * screenExtents.z, -screenExtents.y), Quaternion.identity, cornerParentRotation);
                cornerPoints[6] = Instantiate(cornerScreenObject, new(-screenExtents.x, 2 * screenExtents.z, screenExtents.y), Quaternion.identity, cornerParentRotation);
                cornerPoints[7] = Instantiate(cornerScreenObject, new(screenExtents.x, 2 * screenExtents.z, screenExtents.y), Quaternion.identity, cornerParentRotation);
                cornerPoints[0].name = "cornerDown1"; // down/low corner points (on the floor)
                cornerPoints[1].name = "cornerDown2";
                cornerPoints[2].name = "cornerDown3";
                cornerPoints[3].name = "cornerDown4";
                cornerPoints[4].name = "cornerUp1"; // high corner  points
                cornerPoints[5].name = "cornerUp2";
                cornerPoints[6].name = "cornerUp3";
                cornerPoints[7].name = "cornerUp4";

                // set positions of the corners
                cornerParentTranslation.position = screenCenterFloor;
                cornerParentRotation.rotation = Quaternion.Euler(0, screenRotationZero.eulerAngles.y, 0);

                // FIND ROOM CENTER
                // find the largest distance to the unity zero point
                float[] dist = { 0, 0, 0, 0 };
                dist[0] = Vector3.Distance(floorZero, cornerPoints[0].transform.position);
                dist[1] = Vector3.Distance(floorZero, cornerPoints[1].transform.position);
                dist[2] = Vector3.Distance(floorZero, cornerPoints[2].transform.position);
                dist[3] = Vector3.Distance(floorZero, cornerPoints[3].transform.position);
                int maxIndex = 0;
                int minIndex = 0;
                float maxDist = dist[0];
                float minDist  = dist[0];
                for (int i = 1; i < dist.Length; i++)
                {
                    if (dist[i] > maxDist)
                    {
                        maxDist = dist[i];
                        maxIndex = i;
                    }
                    if (dist[i] < minDist)
                    {
                        minDist = dist[i];
                        minIndex = i;
                    }
                }

                // FOUND Room Zero ;) 
                Transform roomZero = cornerPoints[maxIndex].transform;  // farest corner from the SCREEN found
                Debug.Log("***** MRUKZERO: RoomZero found max-dist: " + maxDist + " at index: " + maxIndex + " position: " + roomZero);

                // ------------------------------------------------------------------------------------------------------
                // draw line from unityZero (MRUKZero) to new roomZero
                // show line between the two points yellow-to-red
                LineRenderer line2unityZeroPoint = Instantiate(lineRendererObject, Vector3.zero, Quaternion.identity);
                line2unityZeroPoint.positionCount = 2;
                line2unityZeroPoint.SetPosition(0, roomZero.position); // red point
                line2unityZeroPoint.SetPosition(1, Vector3.zero); // to UnityZero (0,0,0) point MRUK startPoint

                Instantiate(centerObject, new Vector3(floorZero.x, 0, floorZero.z), Quaternion.identity);
                LineRenderer line2realRoomCenter = Instantiate(lineRendererObject, Vector3.zero, Quaternion.identity); 
                line2realRoomCenter.positionCount = 2;
                line2realRoomCenter.SetPosition(0, roomZero.position); // red point
                line2realRoomCenter.SetPosition(1, new Vector3(floorZero.x, 0, floorZero.z)); // real roomCenterPoint inside Room and used for roomZeroPoint Calculation 
                                                                                              // not Vector3.zero !!! because it can also be outside from the room!

                // ======================================================================================================
                // ------------------------------------------------------------------------------------------------------
                // TRANSFORMATION -here it happens
                // move the gizmo to the room zero position (farest corner from the SCREEN) for easy unity construction

                Transform positionMRUK2Room = GameObject.Find("Position2Room").transform;
                Transform rotationMRUK2Room = GameObject.Find("Rotation2Room").transform; 
                positionMRUK2Room.position = roomZero.position;                                          // 1) move to position, farest corner from "SCREEN"
                rotationMRUK2Room.rotation = Quaternion.Euler(0, screenRotationZero.eulerAngles.y, 0);   // 2) rotation to MR_UK

                // ------------------------------------------------------------------------------------------------------
                // ======================================================================================================
                // RED small REFERENZ POINT
                // check damit linie RoomZero zu UnityZero im ersten Quadranten ist

                Transform referenzPoint = GameObject.Find("ReferenzPoint").transform;
                LineRenderer line2RefPoint = Instantiate(lineRendererObject, Vector3.zero, Quaternion.identity);
                line2RefPoint.positionCount = 2;
                line2RefPoint.SetPosition(0, roomZero.position); // red point
                line2RefPoint.SetPosition(1, referenzPoint.position);

                Debug.Log("***** MRUKZERO: RefPoint: " + referenzPoint.position + referenzPoint.eulerAngles); // immer 45 Grad   
                // ------------------------------------------------------------------------------------------------------
                // ------------------------------------------------------------------------------------------------------
                // KORREKTUR Quadranten Berechnung
                // float angleSigned = Vector2.SignedAngle(a, b);

                float angleKorrektur = 0;
                // float unityZeroAngle = Mathf.Atan2(0-roomZero.position.z, 0-roomZero.position.x) * Mathf.Rad2Deg;
                float realFloorCenterAngle = Mathf.Atan2(floorZero.z-roomZero.position.z, floorZero.x- roomZero.position.x) * Mathf.Rad2Deg;
                float referenzPointAngle = Mathf.Atan2(referenzPoint.position.z-roomZero.position.z, referenzPoint.position.x- roomZero.position.x) * Mathf.Rad2Deg;
                // Debug.Log("***** MRUKZERO: realRoomCenter: " + floorZero + " refPoint: " + referenzPoint.position);
                float checkAngle = realFloorCenterAngle - referenzPointAngle;
                if (checkAngle < 0f) checkAngle += 360f; // angle between 0 and 360 degrees

                if (checkAngle >= 0f && checkAngle < 90f) angleKorrektur     = 270;
                if (checkAngle >= 90f && checkAngle < 180f) angleKorrektur   = 180;
                if (checkAngle >= 180f && checkAngle < 270f) angleKorrektur  = 90;
                if (checkAngle >= 270f && checkAngle <= 360f) angleKorrektur = 0;

                // ...and USE the CORRECTION ANGLE 
                transformPosition = referenzPoint.position;
                transformRotation = screenRotationZero.eulerAngles.y + angleKorrektur;
                rotationMRUK2Room.rotation = Quaternion.Euler(0, screenRotationZero.eulerAngles.y + angleKorrektur, 0);
                Debug.Log("***** MRUKZERO: realFloorCenterAngle: " + realFloorCenterAngle + " refPointAngle: " + referenzPointAngle + " >> AngleKorrektur: " + angleKorrektur + " checkAngle: "+checkAngle);
            }
            else
            {
                Debug.LogError("***** MRUKZERO: Room is invalid: Only exactly 1 SCREEN (for UnityZeroTransform) allowed, found " + allScreensAnchors.Count);
            }
        }

        // ------------------------------------------------------------------------------------------------------
        // HELPER

        // Transform a Point to the "OWN ZERO" coordinate System
        Vector2 ZeroTransformation(Vector2 point)
        {
            float rad = transformRotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            point = point + transformPosition;
            Vector2 rotatedPoint = new Vector2(
                point.x * cos - point.y * sin,
                point.x * sin + point.y * cos
            );

            return rotatedPoint;
        }

    }
}
