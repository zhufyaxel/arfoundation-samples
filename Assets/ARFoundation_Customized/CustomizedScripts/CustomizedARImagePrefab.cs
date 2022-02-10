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

[RequireComponent(typeof(ARTrackedImageManager))]
public class CustomizedARImagePrefab : MonoBehaviour, ISerializationCallbackReceiver
{
    // This is a structure that copy paste from PrefabImagePairManager. Use this first for usage
    [Serializable]
    struct NamedPrefab
    {
        // System.Guid isn't serializable, so we store the Guid as a string. At runtime, this is converted back to a System.Guid
        public string imageGuid;
        public GameObject imagePrefab;

        public NamedPrefab(Guid guid, GameObject prefab)
        {
            imageGuid = guid.ToString();
            imagePrefab = prefab;
        }
    }

    [SerializeField]
    List<NamedPrefab> m_PrefabsList = new List<NamedPrefab>();
    //Please notice that the Guid here is something in system.guid
    Dictionary<Guid, GameObject> m_PrefabsDictionary = new Dictionary<Guid, GameObject>();
    Dictionary<Guid, GameObject> m_Instantiated = new Dictionary<Guid, GameObject>();
    ARTrackedImageManager m_TrackedImageManager;

    [SerializeField]
    XRReferenceImageLibrary m_ImageLibrary;

    public XRReferenceImageLibrary imageLibrary
    {
        get => m_ImageLibrary;
        set => m_ImageLibrary = value;
    }
    #region InterfaceRelated
    public void OnBeforeSerialize()
    {
        m_PrefabsList.Clear();
        foreach (var kvp in m_PrefabsDictionary)
        {
            m_PrefabsList.Add(new NamedPrefab(kvp.Key, kvp.Value));
        }
    }

    public void OnAfterDeserialize()
    {
        m_PrefabsDictionary = new Dictionary<Guid, GameObject>();
        foreach (var entry in m_PrefabsList)
        {
            m_PrefabsDictionary.Add(Guid.Parse(entry.imageGuid), entry.imagePrefab);
        }
    }
    #endregion
    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // Give the initial image a reasonable default scale
            var minLocalScalar = Mathf.Min(trackedImage.size.x, trackedImage.size.y) / 2;
            trackedImage.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);
            AssignPrefab(trackedImage);
        }
    }

    void AssignPrefab(ARTrackedImage trackedImage)
    {
        if (m_PrefabsDictionary.TryGetValue(trackedImage.referenceImage.guid, out var prefab))
            m_Instantiated[trackedImage.referenceImage.guid] = Instantiate(prefab, trackedImage.transform);
    }
    public GameObject GetPrefabForReferenceImage(XRReferenceImage referenceImage)
           => m_PrefabsDictionary.TryGetValue(referenceImage.guid, out var prefab) ? prefab : null;

    public void SetPrefabForReferenceImage(XRReferenceImage referenceImage, GameObject alternativePrefab)
    {
        m_PrefabsDictionary[referenceImage.guid] = alternativePrefab;
        if (m_Instantiated.TryGetValue(referenceImage.guid, out var instantiatedPrefab))
        {
            m_Instantiated[referenceImage.guid] = Instantiate(alternativePrefab, instantiatedPrefab.transform.parent);
            Destroy(instantiatedPrefab);
        }
    }

}
