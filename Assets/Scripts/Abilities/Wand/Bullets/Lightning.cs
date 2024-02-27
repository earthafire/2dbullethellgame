using UnityEngine;

public class Lightning : Bullet
{
    public override float pierce { get => base.pierce; set => base.pierce = 10; }

    float detectionRange = .5f;
    Collider2D[] detections;


    void Start()
    {
        base.Start();
        detections = new Collider2D[32];
    }

    public override void OnHit(Enemy enemy)
    {
        base.OnHit(enemy);

        if (DetectClosestEnemy() != null)
        {
            RotateTowards(DetectClosestEnemy().transform.position);
        }
    }

    private Enemy DetectClosestEnemy()
    {
        float closestDistance = Mathf.Infinity;
        Enemy closestEnemy = null;

        detections = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        foreach (var col in detections)
        {
            if (col.gameObject.layer == 7 || col.gameObject.layer == 9) // Enemy layer & Flying Enemy Layer
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);

                if (distance < closestDistance)
                {
                    closestEnemy = col.gameObject.GetComponent<Enemy>();
                    closestDistance = distance;
                }
            }
        }
        return closestEnemy;
    }
    private void RotateTowards(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * direction;
        Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 45);
    }
}
