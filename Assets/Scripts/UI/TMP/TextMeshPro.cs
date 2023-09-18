using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextMeshPro : MonoBehaviour 
{
public PlayerAttributes _playerAttributes;
private gameSupervisorController _gameSupervisorController;
public float _timer;
[SerializeField] public TMP_Text _playerLevelTMP, _currentExperienceTMP, _timerTMP, _healthValueTMP;

    // Start is called before the first frame update
    void Start()
    {
        GameObject gameSupervisor = GameObject.Find("gameSupervisor");
        _gameSupervisorController = gameSupervisor.GetComponent<gameSupervisorController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        _timer = _gameSupervisorController.gameTimer;

        // Set TextMeshPro SetText();
        _playerLevelTMP.SetText("Level: " +_playerAttributes._localAttributes[Attribute.level].ToString());
        _currentExperienceTMP.SetText("XP: " + _playerAttributes._localAttributes[Attribute.experience].ToString() + " / " + _playerAttributes._experienceUntilLevelUp);
        _timerTMP.SetText("{0:2}", _timer);
        _healthValueTMP.SetText((int)_playerAttributes._localAttributes[Attribute.health] + " / " + (int)_playerAttributes._playerAttributes.GetAttribute(Attribute.health));
    }
}
