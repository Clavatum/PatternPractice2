using UnityEngine;
using DG.Tweening;
using System;

public class TransformChanger : MonoBehaviour
{
    public event UpdateScaleEventHandler OnTransformChanged;
    public event Action OnBalloonReachedMaxSize;
    public event Action OnBalloonReachedMinSize;

    [Header("Transform Settings")]
    [SerializeField] private float scaleChangeAmount;
    [SerializeField] private float scaleChangeDuration;
    [SerializeField] private float minScaleChangeDuration;
    [SerializeField] private Vector3 maxScale;
    [SerializeField] private Vector3 minScale;

    private bool isReachedMaxSize = false;
    private bool isReachedBoundaries = false;

    private void ChangeScale(float changeAmount, float changeDuration, bool isNeedsToBeGetBigger)
    {
        if (isReachedBoundaries) return;
        if (IsBalloonReachedMaxSize())
        {
            OnBalloonReachedMaxSize?.Invoke();
        }
        else
        {
            OnBalloonReachedMinSize?.Invoke();
        }

        transform.DOKill();
        changeAmount = scaleChangeAmount;
        changeDuration = scaleChangeDuration;
        Vector3 targetScale = isNeedsToBeGetBigger ? transform.localScale * changeAmount : transform.localScale / changeAmount;
        transform.DOScale(targetScale, changeDuration).SetEase(isNeedsToBeGetBigger ? Ease.OutBack : Ease.InOutBack);
        scaleChangeDuration -= 0.1f;
        if (scaleChangeDuration <= minScaleChangeDuration)
            scaleChangeDuration = minScaleChangeDuration;
    }

    private bool IsBalloonReachedMaxSize()
    {
        if (transform.localScale.x >= maxScale.x) //since the balloon gets equally smaller or bigger on every axis, 
        {                                         //checking only one of the axis would be enough
            isReachedMaxSize = true;
            isReachedBoundaries = true;
        }
        if (transform.localScale.x <= minScale.x)
        {
            isReachedMaxSize = false;
            isReachedBoundaries = true;
        }
        return isReachedMaxSize;
    }

    public void TriggerTransformChangedEvent(bool isNeedsToBeGetBigger) { OnTransformChanged?.Invoke(scaleChangeAmount, scaleChangeDuration, isNeedsToBeGetBigger); }

    public delegate void UpdateScaleEventHandler(float changeAmount, float changeDuration, bool isNeedsToBeGetBigger);

    void OnEnable()
    {
        OnTransformChanged += ChangeScale;
    }

    void OnDisable()
    {
        OnTransformChanged -= ChangeScale;
    }
}
