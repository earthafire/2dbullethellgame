using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyXpObjectData")]
public class EnemyXpObjectData : SerializedScriptableObject
{
    private System.Random random = new System.Random();

    public int xpValue;

    [LabelWidth(50)]
    [PreviewField(50)]
    public Sprite[] sprites;
    
    public Sprite GetRandomSprite()
    {
        return sprites[random.Next(sprites.Length)];
    }
}
