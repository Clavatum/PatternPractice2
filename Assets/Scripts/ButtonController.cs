using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public event Action<Button> OnButtonClicked;

    [SerializeField] private List<Button> buttonList = new List<Button>();
    [SerializeField] private Material lightMaterial;

    void Awake()
    {
        buttonList.AddRange(GetComponentsInChildren<Button>());

        foreach (var button in buttonList)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
        Button clickedButton = clickedObject.GetComponent<Button>();

        if (clickedButton != null)
        {
            ClearHighlight();
            SetButtonHighlight(clickedButton, true);
            OnButtonClicked?.Invoke(clickedButton);
        }
    }

    public void SetButtonHighlight(Button buttonToHighlight, bool isHighlighted)
    {
        if (isHighlighted)
            Debug.Log($"{buttonToHighlight.name} highlighted");

        buttonToHighlight.image.material = isHighlighted ? lightMaterial : null;
    }

    public void ClearHighlight()
    {
        Debug.Log("buttons cleared highlight");
        foreach (var button in buttonList)
        {
            SetButtonHighlight(button, false);
        }
    }

    public Button GetRandomButton()
    {
        return buttonList[UnityEngine.Random.Range(0, buttonList.Count)];
    }

    private void OnDestroy()
    {
        foreach (var button in buttonList)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
}