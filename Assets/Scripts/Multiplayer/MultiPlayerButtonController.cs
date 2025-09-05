using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiplayerButtonController : NetworkBehaviour
{
    public event Action<Button> OnButtonClicked;

    [SerializeField] private List<Button> buttonList = new();
    [SerializeField] private Material lightMaterial;

    public override void OnNetworkSpawn()
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
        buttonToHighlight.image.material = isHighlighted ? lightMaterial : null;
    }

    public void ClearHighlight()
    {
        foreach (var button in buttonList)
        {
            SetButtonHighlight(button, false);
        }
    }

    public int GetRandomButtonIndex()
    {
        return UnityEngine.Random.Range(0, buttonList.Count);
    }

    public List<Button> GetButtonList()
    {
        return buttonList;
    }

    private void OnDestroy()
    {
        foreach (var button in buttonList)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
}