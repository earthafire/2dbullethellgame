using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityObject : MonoBehaviour
{
    public float duration = 0;
    public float speed = 0;
    public float damage = 0;
    public GameObject player;

    //public Action<Enemy> onEnemyHit;

    private void Awake()
    {
        player = GlobalReferences.player;
    }
    public void Initialize(AbilityObject _obj)
    {
        _obj.duration = CalculateModifiedDuration(_obj.duration);
        _obj.speed = CalculateModifiedBulletSpeed(_obj.speed);
        _obj.transform.localScale = CalculateModifiedSize(_obj.transform.localScale);
        StartCoroutine(CountDuration(_obj.duration));
    }

   /* private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy Layer OR Flying Enemy Layer
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            onEnemyHit?.Invoke(enemy);
        }
    }
    public void Hit(Enemy enemy)
    {
        enemy.TakeDamage((int)damage);
    }*/

    public IEnumerator CountDuration(float _duration)
    {
        yield return new WaitForSeconds(_duration); 
        Destroy(gameObject);
    }

    public static float CalculateModifiedDuration(float baseValue)
    {
        float durationFactor = PlayerAttributes.stats[Attribute.duration];
        float modifiedDuration = baseValue * durationFactor;
        //print("Duration: " + modifiedDuration);
        return modifiedDuration;
    }
    public static Vector3 CalculateModifiedSize(Vector3 baseSize)
    {
        float scaleFactor = PlayerAttributes.stats[Attribute.size];
        Vector3 modifiedSize = new Vector3(baseSize.x * scaleFactor, baseSize.y * scaleFactor, baseSize.z);
        return modifiedSize;
    }
    public static float CalculateModifiedBulletSpeed(float baseValue)
    {
        float scaleFactor = PlayerAttributes.stats[Attribute.bulletSpeed];
        float modifiedSpeed = baseValue * scaleFactor;
        //print("Speed: " + modifiedSpeed);
        return modifiedSpeed;
    }
}
