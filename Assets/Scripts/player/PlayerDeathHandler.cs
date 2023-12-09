using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathHandler
{
    internal void HandleDeath(PlayerAttributes playerAttributes)
    {
        //Make the player no longer controllable
        SuspendPlayerControl(playerAttributes);
        //suspend enemies' actions
        SuspendSpawnedEnemyActions();
        //stop enemies spawning
        SetEnemiesSpawningSuspended(true);
    }
    private void SuspendPlayerControl(PlayerAttributes playerAttributes)
    {
        //disable movement
        SetPlayerMovementAllowed(false, playerAttributes);
        //disable activatable ability components on player
        SetPlayerAbilitiesAllowed(false, playerAttributes);
    }
    private void SetPlayerMovementAllowed(bool shouldAllow, PlayerAttributes playerAttributes)
    {
        if (playerAttributes.TryGetComponent(out PlayerMovement playerMovement))
        {
            if (shouldAllow)
            {
                playerMovement.isPlayerInControl = true;
            }
            else
            {
                playerMovement.isPlayerInControl = false;
            }
        }
    }
    private void SetPlayerAbilitiesAllowed(bool shouldAllow, PlayerAttributes playerAttributes)
    {
        ActivatableAbility[] abilities = playerAttributes.GetComponents<ActivatableAbility>();
        foreach (var ability in abilities)
        {
            if (shouldAllow)
            {
                ability.enabled = true;
            }
            else
            {
                ability.enabled = false;
            }
        }
        if (playerAttributes.TryGetComponent(out PlayerAbilityManager playerAbilityManager))
        {
            if (shouldAllow)
            {
                playerAbilityManager.suspendAbilities = false;
            }
            else
            {
                playerAbilityManager.suspendAbilities = true;
            }

        }
    }
    private static void SuspendSpawnedEnemyActions()
    {
        if (gameSupervisorController.instance != null)
        {
            if (gameSupervisorController.instance.spawnedEnemiesInScene != null)
            {
                foreach (var spawnedEnemy in gameSupervisorController.instance.spawnedEnemiesInScene)
                {
                    if (spawnedEnemy.TryGetComponent(out Enemy enemy))
                    {
                        enemy.suspendActions = true;
                    }
                }
            }
        }
    }

    private void SetEnemiesSpawningSuspended(bool shouldSuspend)
    {
        if (gameSupervisorController.instance != null)
        {
            if (shouldSuspend)
            {
                gameSupervisorController.instance.suspendSpawning = true;
            }
            else
            {
                gameSupervisorController.instance.suspendSpawning = false;
            }
        }
    }

   
}
