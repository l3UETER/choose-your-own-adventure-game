[TestClass]
[DoNotParallelize]
public class SafeDragonInputTests
{
    private sealed class BoundedReader(string script) : StringReader(script)
    {
        private int remaining = 40;
        public override string? ReadLine()
        {
            Assert.IsTrue(remaining-- > 0, "Dragon input loop did not terminate after closed input.");
            return base.ReadLine();
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ClosedDragonInput_RetreatsWithoutExtraAttacks(bool inCombat)
    {
        string transcript = Journey(inCombat ? "a\n" : "", inCombat);
        var messages = new Messages();
        messages.ReadDictionary();
        StringAssert.Contains(transcript, messages.GetMessage(inCombat ? "retreat_combat" : "retreat"));
        Assert.IsFalse(transcript.Contains(messages.GetMessage("invalid")));
        if (inCombat)
            StringAssert.Contains(transcript, "Hero's health points are now 14.");
        else
            Assert.IsFalse(transcript.Contains(messages.GetMessage("combat_stats_header")));
    }

    private static string Journey(string choices, bool inCombat)
    {
        int[] combatRolls = inCombat ? [20, 10, 11, 6] : [];
        var random = new RepeatableCombatTests.ScriptedRandom(
            [10, 10, 20, 14, 1, 10, 19, .. combatRolls]);
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var originalColor = Console.ForegroundColor;
        using var input = new BoundedReader(
            "1\n1\nhuman\nHero\nfighter\nroll\nroll\nroll\n2\nn\n" + choices);
        using var output = new StringWriter();
        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            new Game(new Die(random)).StartGame();
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
            Console.ForegroundColor = originalColor;
        }
        random.AssertExhausted();
        return output.ToString();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void InvalidInitialChoices_RepeatPromptWithoutCombatUntilValidChoice(bool attack)
    {
        string transcript = Journey("nonsense\n\n" + (attack ? " Attack \nr\n" : " Retreat \n"), attack);
        var messages = new Messages();
        messages.ReadDictionary();
        Assert.AreEqual(3, transcript.Split(messages.GetMessage("dragon_encounter_prompt")).Length - 1);
        Assert.AreEqual(2, transcript.Split(messages.GetMessage("invalid")).Length - 1);
        StringAssert.Contains(transcript, messages.GetMessage(attack ? "retreat_combat" : "retreat"));
        if (attack)
            StringAssert.Contains(transcript, "Hero's health points are now 14.");
        else
            Assert.IsFalse(transcript.Contains(messages.GetMessage("combat_stats_header")));
    }

    [TestMethod]
    public void InvalidCombatChoices_RepeatPromptWithoutAttacksUntilValidAttack()
    {
        // Initialization, then two rounds: player misses, dragon deals six damage.
        var random = new RepeatableCombatTests.ScriptedRandom(
            10, 10, 20, 20, 10, 11, 6, 20, 10, 11, 6);
        var die = new Die(random);
        var dragon = new Dragon(die);
        var player = new Player(die)
        {
            Name = "Hero", Strength = 15, Agility = 10, HealthPoints = 20,
            Weapon = new Weapon("long sword", 12, "sword")
        };
        var messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();
        var combat = new Combat(player, dragon, messages);
        var originalIn = Console.In;
        var originalOut = Console.Out;
        using var input = new StringReader("nonsense\n\n Attack \nr\n");
        using var output = new StringWriter();
        try
        {
            Console.SetIn(input);
            Console.SetOut(output);

            Assert.IsFalse(combat.StartCombat());
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }

        Assert.IsTrue(combat.PlayerRetreated);
        Assert.AreEqual(8, player.HealthPoints);
        Assert.AreEqual(20, dragon.HealthPoints);
        string transcript = output.ToString();
        Assert.AreEqual(4, transcript.Split(messages.GetMessage("attack_prompt")).Length - 1);
        Assert.AreEqual(2, transcript.Split(messages.GetMessage("invalid")).Length - 1);
        StringAssert.Contains(transcript, messages.GetMessage("retreat_combat"));
        random.AssertExhausted();
    }
}
