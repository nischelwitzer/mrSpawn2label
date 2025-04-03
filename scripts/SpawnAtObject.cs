using Meta.XR.Util;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static OVRPermissionsRequester;
using System.Collections;

// https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-gs

namespace Meta.XR.MRUtilityKit
{
    public class SpawnAtObject : MonoBehaviour
    {
        [SerializeField, Tooltip("Prefab to be placed into the scene, or object in the scene to be moved around.")]
        public GameObject SpawnObject;

        [SerializeField, Tooltip("When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.")]
        public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;

        void Start()
        {
            Debug.Log("***** waiting for MRUK Scene to be ready...");
        }

        public void StartSpawn()
        {
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            if (room == null)
            {
                Debug.LogError("***** No room found to spawn in.");
            }

            List<MRUKAnchor> lamps = new();

            foreach (MRUKAnchor anchor in MRUK.Instance.GetCurrentRoom().Anchors)
            {
                if (anchor.Label == MRUKAnchor.SceneLabels.LAMP) lamps.Add(anchor);

            }

            // Calculate validation data
            if (lamps.Count == 1)
            {
                Vector3 lampPosition = lamps[0].transform.position;
                Quaternion lampRotation = lamps[0].transform.rotation;
                Vector3 lampScale = lamps[0].transform.localScale;

                SpawnObject.transform.position = lampPosition;
                Debug.Log("***** SpawnPosition " + lampPosition + " GO: " + SpawnObject.name);
            }
            else
            {
                Debug.LogError("***** Room is invalid: Only exactly 1 allowed, found " + lamps.Count);
            }
        }

    }
}
