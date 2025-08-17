using UnityEngine;
using DG.Tweening;

public class TransformChanger : MonoBehaviour
{
    public event UpdateScaleEventHandler OnTransformChanged;

    [Header("Transform Settings")]
    [SerializeField] private float scaleChangeAmount;
    [SerializeField] private float scaleChangeDuration;
    [SerializeField] private float minScaleChangeDuration;

    void Awake()
    {
        OnTransformChanged += ChangeScale;
    }

    private void ChangeScale(float changeAmount, float changeDuration, bool isNeedsToBeGetBigger)
    {
        transform.DOKill();
        changeAmount = scaleChangeAmount;
        changeDuration = scaleChangeDuration;
        Vector3 targetScale = isNeedsToBeGetBigger ? transform.localScale * changeAmount : transform.localScale / changeAmount;
        transform.DOScale(targetScale, changeDuration).SetEase(isNeedsToBeGetBigger ? Ease.OutBack : Ease.InOutBack);
        scaleChangeDuration -= 0.1f;
        if (scaleChangeDuration <= minScaleChangeDuration)
            scaleChangeDuration = minScaleChangeDuration;
    }

    public void TriggerTransformChangedEvent(bool isNeedsToBeGetBigger) { OnTransformChanged?.Invoke(scaleChangeAmount, scaleChangeDuration, isNeedsToBeGetBigger); }

    public delegate void UpdateScaleEventHandler(float changeAmount, float changeDuration, bool isNeedsToBeGetBigger);
}
