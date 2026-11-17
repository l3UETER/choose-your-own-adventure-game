[TestClass]
public class WeaponEnchantmentTests
{
    [TestMethod]
    [DataRow("Fighter", "long sword", 12, true, 8)]
    [DataRow("Magician", "lightning bolt spell", 12, true, 8)]
    [DataRow("Thief", "dagger", 6, true, 8)]
    [DataRow("Archer", "long bow", 8, true, 8)]
    [DataRow("Fighter", "long sword", 12, false, 5)]
    [DataRow("Magician", "lightning bolt spell", 12, false, 5)]
    [DataRow("Thief", "dagger", 6, false, 5)]
    [DataRow("Archer", "long bow", 8, false, 5)]
    public void Hit_AddsBonusAfterOrdinaryRoll(string occupation, string type, int sides, bool enchanted, int damage)
    {
        var random = new RepeatableCombatTests.ScriptedRandom(10, 10, 20, 15, 11, 5, 1);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var weapon = new Weapon(type, sides, "weapon");
        var player = new Player(die) { Occupation = occupation, Strength = 15, Weapon = weapon, HasWeaponEnchantment = enchanted };
        var messages = new Messages();
        messages.ReadDictionary();

        string output = player.Attack(dragon, messages);

        Assert.AreEqual(20 - damage, dragon.HealthPoints);
        StringAssert.Contains(output, string.Format(messages.GetMessage("player_damage_dealt"), damage));
        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_health_now"), dragon.Name, 20 - damage));
        Assert.AreSame(weapon, player.Weapon);
        Assert.AreEqual(type, weapon.Type);
        Assert.AreEqual(sides, weapon.MaxDamage);
        Assert.AreEqual(occupation, player.Occupation);
        random.AssertExhausted();
    }

    [TestMethod]
    [DataRow(16)]
    [DataRow(15)]
    public void EnchantedMissOrDefense_DealsNoDamage(int attack)
    {
        int[] rolls = attack == 16 ? [10, 10, 20, 16] : [10, 10, 20, 15, 10, 1];
        var random = new RepeatableCombatTests.ScriptedRandom(rolls);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var player = new Player(die) { Strength = 15, Weapon = new Weapon("dagger", 6, ""), HasWeaponEnchantment = true };
        var messages = new Messages();
        messages.ReadDictionary();
        player.Attack(dragon, messages);
        Assert.AreEqual(20, dragon.HealthPoints);
        random.AssertExhausted();
    }
}
