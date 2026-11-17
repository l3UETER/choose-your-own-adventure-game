[TestClass]
public class RepeatableCombatTests
{
    internal sealed class ScriptedRandom(params int[] rolls) : Random
    {
        private readonly Queue<int> _rolls = new(rolls);

        public override int Next(int minValue, int maxValue)
        {
            Assert.IsTrue(_rolls.Count > 0, "Unexpected random roll.");
            int roll = _rolls.Dequeue();
            Assert.IsTrue(roll >= minValue && roll < maxValue, "Scripted roll is outside the die's range.");
            return roll;
        }

        public void AssertExhausted() => Assert.AreEqual(0, _rolls.Count, "Unused scripted rolls.");
    }

    private static Messages English()
    {
        var messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();
        return messages;
    }

    [TestMethod]
    public void PlayerHit_RemovesExactWeaponDamage()
    {
        // Dragon initialization, attack at strength, failed defense, damage, reply.
        var random = new ScriptedRandom(10, 10, 20, 15, 11, 7, 1);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var player = new Player(die)
        {
            Strength = 15,
            Weapon = new Weapon("long sword", 12, "sword")
        };
        var messages = English();

        string output = player.Attack(dragon, messages);

        Assert.AreEqual(13, dragon.HealthPoints);
        StringAssert.Contains(output, string.Format(messages.GetMessage("player_damage_dealt"), 7));
        random.AssertExhausted();
    }

    [TestMethod]
    [DataRow(16, 11, 20)] // Miss above strength.
    [DataRow(15, 10, 20)] // Defense at agility.
    public void PlayerMissOrDefense_PreservesDragonHealth(int attack, int defense, int health)
    {
        int[] rolls = attack == 16 ? [10, 10, 20, attack] : [10, 10, 20, attack, defense, 1];
        var random = new ScriptedRandom(rolls);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var player = new Player(die) { Strength = 15, Weapon = new Weapon("long sword", 12, "sword") };

        player.Attack(dragon, English());

        Assert.AreEqual(health, dragon.HealthPoints);
        random.AssertExhausted();
    }

    [TestMethod]
    [DataRow(10, 11, 6, 20, 14)]
    [DataRow(10, 11, 12, 5, 0)]
    [DataRow(10, 11, 1, 20, 19)]
    public void DragonHit_RemovesExactDamageAndClampsHealth(int attack, int defense, int damage, int initial, int expected)
    {
        var random = new ScriptedRandom(10, 10, 20, attack, defense, damage);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var player = new Player(die) { Name = "Hero", Agility = 10, HealthPoints = initial };
        var messages = English();

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(expected, player.HealthPoints);
        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_damage_dealt"), dragon.Name, initial - expected));
        random.AssertExhausted();
    }

    [TestMethod]
    [DataRow(11, 11)]
    [DataRow(10, 10)]
    public void DragonMissOrDefense_PreservesPlayerHealth(int attack, int defense)
    {
        int[] rolls = attack == 11 ? [10, 10, 20, attack] : [10, 10, 20, attack, defense];
        var random = new ScriptedRandom(rolls);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var player = new Player(die) { Agility = 10, HealthPoints = 20 };

        dragon.Attack(player, English());

        Assert.AreEqual(20, player.HealthPoints);
        random.AssertExhausted();
    }
}

[TestClass]
[DoNotParallelize]
public class RepeatableJourneyTests
{
    [TestMethod]
    [DataRow("victory")]
    [DataRow("defeat")]
    [DataRow("retreat_combat")]
    [DataRow("retreat")]
    public void NorthJourney_UsesControlledInitializationAndCombat(string ending)
    {
        int dragonHealth = ending == "victory" ? 7 : 20;
        int playerHealthRoll = ending == "defeat" ? 1 : 19;
        var rolls = new List<int> { 10, 10, dragonHealth, 14, 1, 10, playerHealthRoll };
        if (ending != "retreat")
        {
            rolls.AddRange([15, 11, 7]);
            if (ending != "victory")
                rolls.AddRange([1, 10, 11, 6]); // Damage reply and dragon's attack.
        }
        var random = new RepeatableCombatTests.ScriptedRandom(rolls.ToArray());
        var game = new Game(new Die(random));
        var messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var originalColor = Console.ForegroundColor;
        using var input = new StringReader("1\n1\nhuman\nHero\nfighter\nroll\nroll\nroll\n2\ns\n3\nn\n" +
            (ending == "retreat" ? "r\n" : "a\nr\n"));
        using var output = new StringWriter();
        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            game.StartGame();
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
            Console.ForegroundColor = originalColor;
        }

        string transcript = output.ToString();
        StringAssert.Contains(transcript, messages.GetMessage("south_path_narrative"));
        StringAssert.Contains(transcript, messages.GetMessage(ending));
        if (ending != "retreat")
        {
            StringAssert.Contains(transcript, string.Format(messages.GetMessage("dragon_health_now"), "Smolderfang", ending == "victory" ? 0 : 13));
            if (ending != "victory")
                StringAssert.Contains(transcript, string.Format(messages.GetMessage("player_health_now"), "Hero", ending == "defeat" ? 0 : 14));
        }
        random.AssertExhausted();
    }
}
