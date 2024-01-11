using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class PlayerXPManager : MonoBehaviour
{
    private int level;
    public int Level {  get { return level; } }
    public int currentXp { get; private set; }
    public int xpUntilLevelUp { get; private set; }

    GameObject player;

    public LevelUp levelUp;
    [SerializeField] ParticleSystem xpParticles;

    public event Action<Attributes, UpgradeAttribute> upgradeApplied;

    public event Action<GameObject> LevelUpEvent;
    public event Action<GameObject> XPGainedEvent;

    public void Awake()
    {
        Initialize();
        GlobalReferences.xpManager = this;
    }

    public void Start()
    {
        player = GlobalReferences.player;
    }

    public void Initialize()
    {
        level = 1;
        currentXp = 0;
        xpUntilLevelUp = 100;
    }

    public void addExperience(int experienceToAdd)
    {
        currentXp += experienceToAdd;

        if (currentXp  > xpUntilLevelUp)
        {
            DoLevelUp();
        }

        XPGainedEvent?.Invoke(player);
    }
    public void DoLevelUp()
    {
        level += 1;

        LevelUpEvent?.Invoke(player);

        xpParticles.Emit(xpUntilLevelUp);

        xpUntilLevelUp = (int)(xpUntilLevelUp * 1.1f);

        levelUp.HandleLevelUp();

        currentXp = 0;
        XPGainedEvent?.Invoke(player);
    }

}
