using System.Collections.Generic;
using AdaptiveTrafficSystem.Tracking;
using MyBox;
using UnityDevKit.Events;
using UnityEngine;

public abstract class DetectorBase : MonoBehaviour, IDetector
{
    [SerializeField] private bool detect = true;
    [SerializeField] private bool lose = true;

    [SerializeField] private bool useLayerFilter = false;

    [SerializeField] [ConditionalField(nameof(useLayerFilter))]
    private LayerMask detectionLayerMask; // TODO -- physics matrix 

    [SerializeField] private bool useTagFilter = false;

    [SerializeField] [ConditionalField(nameof(useTagFilter))]
    private List<string> allowedTags;

    private readonly EventHolder<GameObject> onDetectEvent = new EventHolder<GameObject>();
    private readonly EventHolder<GameObject> onLoseEvent = new EventHolder<GameObject>();

    public void Detect(GameObject detectedObject)
    {
        if (!detect || !IsValidDetection(detectedObject)) return;
        onDetectEvent.Invoke(detectedObject);
    }

    public void Lose(GameObject detectedObject)
    {
        if (!lose || !IsValidDetection(detectedObject)) return;
        onLoseEvent.Invoke(detectedObject);
    }

    public void SubscribeToDetect(EventHolder<GameObject>.EventHandler listener)
    {
        onDetectEvent.AddListener(listener);
    }

    public void SubscribeToLose(EventHolder<GameObject>.EventHandler listener)
    {
        onLoseEvent.AddListener(listener);
    }
    
    public void UnsubscribeToDetect(EventHolder<GameObject>.EventHandler listener)
    {
        onDetectEvent.RemoveListener(listener);
    }

    public void UnsubscribeToLose(EventHolder<GameObject>.EventHandler listener)
    {
        onLoseEvent.RemoveListener(listener);
    }

    protected virtual bool IsValidDetection(GameObject detectedObject) =>
        (!useLayerFilter || detectionLayerMask == (detectionLayerMask | (1 << detectedObject.layer))) &&
        (!useTagFilter || allowedTags.Contains(detectedObject.tag));

    public void SetLayerMask(LayerMask layerMask)
    {
        detectionLayerMask = layerMask;
    }

    public void SetAllowedTags(List<string> tags)
    {
        allowedTags = tags;
    }
}