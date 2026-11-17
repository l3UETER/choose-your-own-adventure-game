[TestClass]
[DoNotParallelize]
public class MagicalArmorTests
{
    private static Messages English()
    {
        var messages = new Messages();
        messages.ReadDictionary();
        return messages;
    }

    [TestMethod]
    [DataRow(true, 1, 20, 20, 0)]
    [DataRow(true, 3, 20, 20, 0)]
    [DataRow(true, 8, 20, 15, 5)]
    [DataRow(true, 12, 5, 0, 5)]
    [DataRow(false, 8, 20, 12, 8)]
    [DataRow(false, 12, 5, 0, 5)]
    public void DragonHit_ReportsExactHealthLostAfterProtection(
        bool armored, int rolledDamage, int initialHealth, int expectedHealth, int reportedDamage)
    {
        var random = new RepeatableCombatTests.ScriptedRandom(10, 10, 20, 10, 11, rolledDamage);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var player = new Player(die)
        {
            Name = "Hero", Agility = 10, HealthPoints = initialHealth, HasMagicalArmor = armored
        };
        var messages = English();

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(expectedHealth, player.HealthPoints);
        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_damage_dealt"), dragon.Name, reportedDamage));
        StringAssert.Contains(output, string.Format(messages.GetMessage("player_health_now"), player.Name, expectedHealth));
        random.AssertExhausted();
    }

    [TestMethod]
    [DataRow(11, 11)]
    [DataRow(10, 10)]
    public void ArmoredMissOrDefense_LeavesHealthUnchanged(int attack, int defense)
    {
        int[] rolls = attack == 11 ? [10, 10, 20, attack] : [10, 10, 20, attack, defense];
        var random = new RepeatableCombatTests.ScriptedRandom(rolls);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var player = new Player(die) { Agility = 10, HealthPoints = 20, HasMagicalArmor = true };
        var messages = English();

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(20, player.HealthPoints);
        StringAssert.Contains(output, attack == 11
            ? string.Format(messages.GetMessage("dragon_missed_player"), dragon.Name, player.Name)
            : string.Format(messages.GetMessage("player_defended_dragon_attack"), player.Name));
        random.AssertExhausted();
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(true, 3)]
    public void PlayerAndCombatStats_ShowArmorProtection(bool armored, int protection)
    {
        var player = new Player { HasMagicalArmor = armored };
        var messages = English();
        var originalOut = Console.Out;
        using var output = new StringWriter();
        try
        {
            Console.SetOut(output);
            player.DisplayStats(messages);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        string expected = $"Armor protection: {protection}";
        StringAssert.Contains(output.ToString(), expected);
        var random = new RepeatableCombatTests.ScriptedRandom(10, 10, 20);
        var combat = new Combat(player, new Dragon(new Die(random)), messages);
        StringAssert.Contains(combat.GetCombatStatsDisplay(), expected);
        random.AssertExhausted();
    }
}
