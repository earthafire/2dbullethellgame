using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUIController : MonoBehaviour
{
    [SerializeField] GameObject _inventoryUI;
    [SerializeField] GameObject _equipmentUI;
    Animator _inventoryAnimator;
    Animator _equipmentAnimator;

    public bool onScreen = true;
    
    private void Start()
    {
        _inventoryAnimator = _inventoryUI.GetComponent<Animator>();
        _equipmentAnimator = _equipmentUI.GetComponent<Animator>();
    }
    public void ToggleInventory()
    {
        if (onScreen)
        {
            _inventoryAnimator.SetTrigger("Exit");
            _equipmentAnimator.SetTrigger("Exit");
            onScreen = false;
        }
        else
        {
            _inventoryAnimator.SetTrigger("Enter");
            _equipmentAnimator.SetTrigger("Enter");
            onScreen = true;
        }
    }
}
