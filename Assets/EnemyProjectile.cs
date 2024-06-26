using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    bool _isMoving = false;
    private int damage = 5;
    private float speed = 1.5f;
    private SpriteRenderer _renderer;
    public void Start()
    {
        StartCoroutine(StartMovingDelay(1f));
        _renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
    private void FixedUpdate()
    {
        if (_isMoving)
        {
            Move();
        }
    }
    public void Update()
    {
        IncreaseOpacity(.0025f);
    }
    public void Move()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.right);
    }
    public IEnumerator CountDuration(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        Destroy(gameObject);
    }
    public IEnumerator StartMovingDelay(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        transform.parent = null;
        _isMoving = true;
        StartCoroutine(CountDuration(1.5f));
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 12) // Player layer
        {
            PlayerAttributes player = collision.gameObject.GetComponent<PlayerAttributes>();
            player.takeDamage(damage);
        }
        print(damage);
    }

    private void IncreaseOpacity(float rate)
    {
        if (_renderer.color.a < 1) 
        {
            _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, _renderer.color.a + rate);
        }
    }
}
