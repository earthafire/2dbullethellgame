using System.Collections.Generic;
using UnityEngine;

public class AbilityTickManager : MonoBehaviour
{
    public static AbilityTickManager Instance { get; private set; }

    private readonly List<AbilityObject> _abilities = new List<AbilityObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        Destroy(this);
    }

    public static void CreateInstanceIfNeeded()
    {
        if (Instance != null)
            return;

        var managerObject = new GameObject("AbilityTickManager");
        Instance = managerObject.AddComponent<AbilityTickManager>();
        DontDestroyOnLoad(managerObject);
    }

    private void Update()
    {
        int count = _abilities.Count;
        for (int i = 0; i < count; i++)
        {
            _abilities[i].Tick();
        }
    }

    public void RegisterAbility(AbilityObject ability)
    {
        if (ability == null || _abilities.Contains(ability))
            return;

        _abilities.Add(ability);
    }

    public void UnregisterAbility(AbilityObject ability)
    {
        if (ability == null)
            return;

        _abilities.Remove(ability);
    }
}
