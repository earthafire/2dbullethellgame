using UnityEngine;

public class XpPickUpRange : MonoBehaviour
{
    CircleCollider2D _circleCollider;

    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
    }
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(_circleCollider.transform.position, Vector3.back, _circleCollider.radius);
    }
}
