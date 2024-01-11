using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class TextMeshPro : MonoBehaviour 
{
    public HealthBar healthbar;
    private gameSupervisorController _gameSupervisorController;
    public float _timer;
    public PlayerAttributes attributes;
    [SerializeField] private PlayerXPManager xpManager;
    [SerializeField] public TMP_Text _playerLevelTMP, _currentExperienceTMP, _timerTMP, _healthValueTMP;

    void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        _gameSupervisorController = GlobalReferences.gameSupervisorController;

        xpManager.XPGainedEvent += SetXPText;
        xpManager.LevelUpEvent += SetLevelText;

        SetLevelText(GlobalReferences.player);
        SetXPText(GlobalReferences.player);
    }

    private void SetLevelText(GameObject @object)
    {
        _playerLevelTMP.SetText("Level: " + xpManager.Level);

    }

    private void SetXPText(GameObject @object)
    {
        _currentExperienceTMP.SetText("XP: " + xpManager.currentXp + " / " + xpManager.xpUntilLevelUp);
    }

    void FixedUpdate()
    {
        _timer = _gameSupervisorController.gameTimer;

        _timerTMP.SetText("{0:2}", _timer);

        _healthValueTMP.SetText((int)attributes.currentHealth + " / " + (int)healthbar.slider.maxValue);
    }
}
