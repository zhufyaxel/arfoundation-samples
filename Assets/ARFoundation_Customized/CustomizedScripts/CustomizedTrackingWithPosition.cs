using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using Logger = UnityEngine.XR.ARFoundation.Samples.Logger;

[RequireComponent(typeof(ARTrackedImageManager))]
[RequireComponent(typeof(ARAnchorManager))]
public class CustomizedTrackingWithPosition : MonoBehaviour
{
    //This will be a naive solution to put the target components into the desired location
    public GameObject imgTrackingIndicator;
    public GameObject anchorTrackingIndicator;
    public GameObject rotationIndicator;
    public GameObject ARCameraObject;
    public GameObject childObjectForCalculation;
    public string targetImageName;
    //public float offsetDegre = 0;
    [SerializeField]
    [Tooltip("Reference Image Library")]
    XRReferenceImageLibrary m_ImageLibrary;
    public bool debugOn = true;
    public bool isTrackingOn = false;
    public Text infoText;

    public List<ARAnchor> anchorList = new List<ARAnchor>(); 

    /// <summary>
    /// Get the <c>XRReferenceImageLibrary</c>
    /// </summary>
    public XRReferenceImageLibrary imageLibrary
    {
        get => m_ImageLibrary;
        set => m_ImageLibrary = value;
    }

    ARTrackedImageManager m_TrackedImageManager;
    ARAnchorManager m_ARAnchorManager;

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        m_ARAnchorManager = GetComponent<ARAnchorManager>();
        anchorList = new List<ARAnchor>();
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    public void CreateAnchorBasedOnImagePos(Transform trackedImgPos)
    {
        var newAnchorObject = Instantiate(anchorTrackingIndicator, trackedImgPos.position, trackedImgPos.rotation);
        newAnchorObject.AddComponent<ARAnchor>();
        if (newAnchorObject.GetComponent<ARAnchor>())
        {
            anchorList.Add(newAnchorObject.GetComponent<ARAnchor>());
            Logger.Log(string.Format("Successfully Added the Anchor, Position: x:{0}, y:{1}, z:{2}",
                newAnchorObject.transform.position.x, newAnchorObject.transform.position.y, newAnchorObject.transform.position.z));
        }
        else
        {
            Logger.Log("Something Wrong when add the anchor");
        }
    }

    public void CreateAnchorBasedOnImagePos()
    {
        Logger.Log("Start creating anchors");
        Transform trackedImgPos = imgTrackingIndicator.transform;
        var newAnchorObject = Instantiate(anchorTrackingIndicator, trackedImgPos.position, trackedImgPos.rotation);
        newAnchorObject.AddComponent<ARAnchor>();
        if (newAnchorObject.GetComponent<ARAnchor>())
        {
            anchorList.Add(newAnchorObject.GetComponent<ARAnchor>());
            Logger.Log(string.Format("Successfully Added the Anchor, Position: x:{0}, y:{1}, z:{2}",
                newAnchorObject.transform.position.x, newAnchorObject.transform.position.y, newAnchorObject.transform.position.z));
        }
        else
        {
            Logger.Log("Something Wrong when add the anchor");
        }
    }

    public void LogPositionAndDistance()
    {
        float distance = Vector3.Distance(ARCameraObject.transform.position, imgTrackingIndicator.transform.position);
        childObjectForCalculation.transform.SetParent(ARCameraObject.transform);
        childObjectForCalculation.transform.localRotation = Quaternion.identity;
        childObjectForCalculation.transform.localPosition = Vector3.zero;
        childObjectForCalculation.transform.SetParent(imgTrackingIndicator.transform);
        var phoneShadow = Instantiate(rotationIndicator, childObjectForCalculation.transform.position, childObjectForCalculation.transform.rotation);
        Logger.Log(string.Format("Distance: {0}, x:{1}, y:{2}, z{3}", distance, 
            childObjectForCalculation.transform.localPosition.x, childObjectForCalculation.transform.localPosition.y, childObjectForCalculation.transform.localPosition.z));
          
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            Logger.Log(string.Format("Image Added, name: {0}", trackedImage.name));
            if (trackedImage.referenceImage.name == targetImageName)
            {
                isTrackingOn = true;
                Logger.Log("Tracking On");
            }
            
        }
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.referenceImage.name == targetImageName)
            {
                imgTrackingIndicator.transform.position = trackedImage.transform.position;
                imgTrackingIndicator.transform.rotation = trackedImage.transform.rotation;
            }
        }
        foreach (var trackedImage in eventArgs.removed)
        {
            if (trackedImage.referenceImage.name == targetImageName)
            {
                isTrackingOn = false;
                Logger.Log("Tracking Off");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            foreach (var image in m_ImageLibrary)
            {
                Debug.Log(image.name);
            }
        }
        if (isTrackingOn)
        {
            childObjectForCalculation.transform.SetParent(ARCameraObject.transform);
            childObjectForCalculation.transform.localRotation = Quaternion.identity;
            childObjectForCalculation.transform.localPosition = Vector3.zero;
            childObjectForCalculation.transform.SetParent(imgTrackingIndicator.transform);
            //Vector3 midPoint = childObjectForCalculation.transform.localPosition / 2 + imgTrackingIndicator.transform.position;
            //Quaternion rot = childObjectForCalculation.transform.localRotation
        }

        

    }




}
