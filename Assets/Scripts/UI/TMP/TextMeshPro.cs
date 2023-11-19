using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextMeshPro : MonoBehaviour 
{
    public HealthBar healthbar;
    private gameSupervisorController _gameSupervisorController;
    public float _timer;
    private PlayerAttributes _playerAttributes;
    [SerializeField] public TMP_Text _playerLevelTMP, _currentExperienceTMP, _timerTMP, _healthValueTMP;

    // Start is called before the first frame update
    void Start()
    {
        _playerAttributes = GameObject.Find("Player").GetComponent<PlayerAttributes>();
        GameObject gameSupervisor = GameObject.Find("gameSupervisor");
        _gameSupervisorController = gameSupervisor.GetComponent<gameSupervisorController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        _timer = _gameSupervisorController.gameTimer;

        // Set TextMeshPro SetText();
        _playerLevelTMP.SetText("Level: " + PlayerAttributes.totalStats[Attribute.level].ToString());
        _currentExperienceTMP.SetText("XP: " + PlayerAttributes.totalStats[Attribute.experience].ToString() + " / " + _playerAttributes._experienceUntilLevelUp);
        _timerTMP.SetText("{0:2}", _timer);
        _healthValueTMP.SetText((int)_playerAttributes.currentHealth + " / " + (int)healthbar.slider.maxValue);
    }
}
