using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public event Action<Button> OnButtonClicked;

    [SerializeField] private List<Button> ButtonList;
    [SerializeField] private Material lightMaterial;

    void Awake()
    {
        ButtonList.AddRange(GetComponentsInChildren<Button>());
        foreach (var button in ButtonList)
        {
            button.onClick.AddListener(() => HandleClick(button));
        }
    }

    public void SetButtonHighlight(Button buttonToHighlight, bool IsNeedsToBeHighlighted)
    {
        if (IsNeedsToBeHighlighted) Debug.Log($"{buttonToHighlight.name} highlighted");
        buttonToHighlight.image.material = IsNeedsToBeHighlighted ? lightMaterial : null;
    }

    public void ClearHighlight()
    {
        Debug.Log("button unhighlighted");
        foreach (var button in ButtonList) SetButtonHighlight(button, false);
    }

    private void HandleClick(Button clickedButton)
    {
        OnButtonClicked?.Invoke(clickedButton);
    }

    public Button GetRandomButton()
    {
        return ButtonList[UnityEngine.Random.Range(0, ButtonList.Count)];
    }
}