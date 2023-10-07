using System;

[Serializable]
public class ItemBuffBase
{
    public int max;
    public int min;
    public Modifiers modifiers;
    public int value;

    public void AddValue(ref int baseValue)
    {
        baseValue += value;
    }

    public void GenerateValue()
    {
        value = UnityEngine.Random.Range(min, max);
    }
}