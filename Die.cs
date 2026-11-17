using System;

public class Die
{
    private readonly Random _random;

    public Die() : this(new Random()) { }

    public Die(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        _random = random;
    }

    public int Roll(int sides)
    {
        return _random.Next(1, sides + 1);
    }
}
