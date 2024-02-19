using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityObject : MonoBehaviour
{
    public float duration;
    public float speed;

    // Start is called before the first frame update
    void OnEnable()
    {
        duration = ActivatableAbility.CalculateModifiedDuration(duration);
        speed = ActivatableAbility.CalculateModifiedBulletSpeed(speed);
        this.transform.localScale = ActivatableAbility.CalculateModifiedSize(this.transform.localScale);
        StartCoroutine(CountDuration());
    }

    public IEnumerator CountDuration()
    {
        yield return new WaitForSeconds(duration); 
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
