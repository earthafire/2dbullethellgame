using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributesManager : MonoBehaviour
{
        [SerializeField] private List<Attributes> attributesList;
    private const string attributesPath = "Assets/Scripts/Attributes/Attributes.cs";

    private void OnApplicationQuit()
    {
        attributesList = HelperFunctions.GetScriptableObjects<Attributes>(attributesPath);

        foreach (var attribute in attributesList)
        {
            attribute.ResetAppliedUpgrades();
        }
    }
}
