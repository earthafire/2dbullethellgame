using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextMeshPro : MonoBehaviour 
{
private PlayerAttributes player_attributes;
public Attributes attributes;

private gameSupervisorController game_supervisor_controller;

public float timer;

            
public int player_level, current_experience, currentHealthValue, maxHealthValue;

[SerializeField] public TMP_Text player_level_tmp, current_experience_tmp, timer_tmp, healthValue_tmp;

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.Find("Player");
        player_attributes = player.GetComponent<PlayerAttributes>();

        GameObject gameSupervisor = GameObject.Find("gameSupervisor");
        game_supervisor_controller = gameSupervisor.GetComponent<gameSupervisorController>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Link to PlayerAttributes script
        player_level = player_attributes.player_level;
        current_experience = player_attributes.current_experience;
        timer = game_supervisor_controller.gameTimer;
        currentHealthValue = (int)player_attributes.attributes.current_attributes[Attribute.health];
        maxHealthValue = (int)player_attributes.attributes.default_attributes[Attribute.health];

        float currentHealth = attributes.GetAttribute(Attribute.health);

        // Not sure if we need/want this
        // float maxhealth = attributes.default_attributes[Attribute.health];
        

        //Convert int to string 
        string player_level_string = player_level.ToString();
        string current_experience_string = current_experience.ToString();

        // Set TextMeshPro SetText();
        player_level_tmp.SetText("Level: " + player_level_string);
        current_experience_tmp.SetText("XP: " + current_experience_string);
        timer_tmp.SetText("{0:2}", timer);
        healthValue_tmp.SetText(currentHealth.ToString());
    }
}
