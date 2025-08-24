
public static class AttributeEnum
{

}

public enum Attribute
{
    maxHealth,
    moveSpeed,

    /// <summary>
    /// integer percentage point increase in damage, divide by 100 to find %
    /// <para>10 = 10% increase</para>
    /// <para>45 = 45% increase</para>
    /// </summary>
    damage,
    acceleration,
    experience,
    level,
    pickUpRange,

    /// <summary>
    /// haste based cooldown system, used so you can never
    /// reach 100% cdr, and so "number of spells cast" per second
    /// increases the same amount per cdr (instead of later cdr
    /// being more valuable and incrasing DPS tremendously)
    /// <para>cdr points   |   % reduction</para>
    /// <para>     0       |   0</para>
    /// <para>     10      |  ~10%</para>
    /// <para>     30      |  ~23%</para>
    /// <para>     50      |   33%</para>
    /// <para>     75      |   43%</para>
    /// <para>     100     |   50%</para>
    /// <para>     200     |   66%</para>
    /// <para>     300     |   75%</para>
    /// <para>     400     |   80%</para>
    /// </summary>
    cooldown,
    duration,
    size,
    bulletSpeed
}


