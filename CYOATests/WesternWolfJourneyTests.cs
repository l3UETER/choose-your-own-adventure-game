[TestClass]
[DoNotParallelize]
public class WesternWolfJourneyTests
{
    private static string Journey(string travel, params int[] rolls)
    {
        var random = new RepeatableCombatTests.ScriptedRandom(
            [10, 10, 20, 14, 1, 10, 19, .. rolls]);
        string transcript = RunJourney(new Game(new Die(random)), travel);
        random.AssertExhausted();
        return transcript;
    }

    private static string RunJourney(Game game, string travel)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var originalColor = Console.ForegroundColor;
        using var input = new BoundedReader(
            "1\n1\nhuman\nHero\nfighter\nroll\nroll\nroll\n2\n" + travel);
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
        return output.ToString();
    }

    private sealed class RangeRandom(params (int sides, int roll)[] script) : Random
    {
        private readonly Queue<(int sides, int roll)> rolls = new(script);
        public override int Next(int minValue, int maxValue)
        {
            Assert.IsTrue(rolls.Count > 0, "Unexpected roll.");
            var expected = rolls.Dequeue();
            Assert.AreEqual(1, minValue);
            Assert.AreEqual(expected.sides + 1, maxValue, "Incorrect die range.");
            return expected.roll;
        }
        public void AssertExhausted() => Assert.AreEqual(0, rolls.Count);
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(15, 6)]
    public void WolfStatsAndBite_UseSpecifiedDiceRanges(int stat, int damage)
    {
        var random = new RangeRandom((20, 20), (20, 20), (20, 20),
            (20, 14), (4, 1), (20, 10), (20, 19),
            (15, stat), (15, stat), (15, stat),
            (20, 20), (20, 1), (20, 11), (6, damage));
        string transcript = RunJourney(new Game(new Die(random)), "w\na\nr\ne\n");
        StringAssert.Contains(transcript, $"Strength: {stat}");
        StringAssert.Contains(transcript, $"Agility: {stat}");
        StringAssert.Contains(transcript, $"Health Points: {stat}");
        StringAssert.Contains(transcript, $"Hero's health points are now {20 - damage}.");
        random.AssertExhausted();
    }

    [TestMethod]
    public void NewAdventureOnSameGame_HasFreshWolfState()
    {
        var random = new RepeatableCombatTests.ScriptedRandom(
            10, 10, 20, 14, 1, 10, 19, 8, 9, 1, 15, 10, 1,
            10, 10, 20, 14, 1, 10, 19, 3, 4, 15);
        var game = new Game(new Die(random));
        StringAssert.Contains(RunJourney(game, "w\na\ne\n"), "The Wolf falls defeated.");
        string second = RunJourney(game, "w\nr\ne\n");
        StringAssert.Contains(second, "Strength: 3");
        StringAssert.Contains(second, "Agility: 4");
        StringAssert.Contains(second, "Health Points: 15");
        Assert.IsFalse(second.Contains("The western route is clear."));
        random.AssertExhausted();
    }

    [TestMethod]
    [DataRow("English", "West")]
    [DataRow("Spanish", "Oeste")]
    [DataRow("French", "Ouest")]
    [DataRow("Italian", "Ovest")]
    public void WolfMessages_AreLocalizedWithMatchingPlaceholders(string language, string west)
    {
        using var json = System.Text.Json.JsonDocument.Parse(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "language_data.json")));
        var english = json.RootElement.GetProperty("English").GetProperty("dictionary");
        var translated = json.RootElement.GetProperty(language).GetProperty("dictionary");
        foreach (var entry in english.EnumerateObject())
        {
            Assert.IsTrue(translated.TryGetProperty(entry.Name, out var value), $"Missing {language}/{entry.Name}");
            string text = value.GetString()!;
            Assert.IsFalse(string.IsNullOrWhiteSpace(text), $"Empty {language}/{entry.Name}");
            string[] expected = System.Text.RegularExpressions.Regex.Matches(entry.Value.GetString()!, @"\{[^{}]+\}")
                .Select(match => match.Value).OrderBy(value => value).ToArray();
            string[] actual = System.Text.RegularExpressions.Regex.Matches(text, @"\{[^{}]+\}")
                .Select(match => match.Value).OrderBy(value => value).ToArray();
            CollectionAssert.AreEqual(expected, actual, $"Placeholders in {language}/{entry.Name}");
            string.Format(text, "Hero", "Hero", "Hero");
        }
        StringAssert.Contains(translated.GetProperty("path_prompt_full").GetString()!, west);
        StringAssert.Contains(translated.GetProperty("path_invalid").GetString()!, west);
    }

    [TestMethod]
    public void WestCommands_DisplayIndependentStatsBeforeChoice()
    {
        string transcript = Journey("w\nr\nwest\nr\ne\n", 1, 15, 12);
        Assert.IsTrue(new Game().IsValidPathChoice(" W "));
        Assert.IsTrue(new Game().IsValidPathChoice("WEST"));
        StringAssert.Contains(transcript, "'w'");
        StringAssert.Contains(transcript, "Strength: 1");
        StringAssert.Contains(transcript, "Agility: 15");
        StringAssert.Contains(transcript, "Health Points: 12");
        StringAssert.Contains(transcript, "Bite damage: 1-6");
        Assert.IsTrue(transcript.IndexOf("Bite damage: 1-6") < transcript.IndexOf("Enter 'a'"));
    }

    private sealed class BoundedReader(string script) : StringReader(script)
    {
        private int remaining = 40;

        public override string? ReadLine()
        {
            Assert.IsTrue(remaining-- > 0, "The western journey did not terminate after scripted input.");
            return base.ReadLine();
        }
    }

    [TestMethod]
    public void AttackAndRetreat_WoundsWolfAndPreservesItOnReturn()
    {
        // Hit at strength, defense fails above agility, five damage; wolf misses.
        string transcript = Journey("w\na\nr\nw\nr\ne\n", 8, 9, 12, 15, 10, 5, 20);
        StringAssert.Contains(transcript, "The Wolf has 7 health points remaining.");
        Assert.AreEqual(1, transcript.Split("Health Points: 7").Length - 1);
        Assert.AreEqual(2, transcript.Split("Strength: 8").Length - 1);
        Assert.IsFalse(transcript.Contains("Smolderfang"));
        Assert.IsFalse(transcript.Contains("Dragon"));
    }

    [TestMethod]
    [DataRow("", 6, 14)]
    [DataRow("s\n1\n3\n", 6, 17)]
    [DataRow("s\n1\n3\n", 1, 20)]
    public void WolfBite_UsesStrengthDefenseAndArmorAndPreservesPlayerHealth(string shopping, int damage, int health)
    {
        // Player misses, wolf hits exactly at strength, defense fails above agility.
        string transcript = Journey(shopping + "w\na\nr\nn\na\nr\n",
            8, 9, 12, 20, 8, 11, damage,
            20, 20); // Northern round: both miss, displaying retained player health.
        StringAssert.Contains(transcript, $"Hero's health points are now {health}.");
        StringAssert.Contains(transcript, $"Health Points: {health}");
        StringAssert.Contains(transcript, "The Wolf bites at Hero.");
    }

    [TestMethod]
    public void WolfVictory_IsPlayerFirstAndPermanentlyClearsRouteWithoutReward()
    {
        // Enchantment makes five damage lethal against eight health. No retaliation roll.
        string transcript = Journey("s\n2\n3\nw\na\nw\ns\n3\ne\n",
            8, 9, 8, 15, 10, 5);
        StringAssert.Contains(transcript, "The Wolf falls defeated.");
        StringAssert.Contains(transcript, "The western route is clear.");
        Assert.AreEqual(1, transcript.Split("Bite damage: 1-6").Length - 1);
        Assert.IsFalse(transcript.Contains("The Wolf bites"));
        StringAssert.Contains(transcript, "Gold: 5");
        Assert.IsFalse(transcript.Contains("Gold: 15"));
    }

    [TestMethod]
    public void EnchantmentAndWeapon_PersistAcrossWolfRetreatsAndIntoDragonCombat()
    {
        string transcript = Journey("s\n2\n3\nw\na\nr\nw\na\nr\ns\n3\nn\na\nr\n",
            8, 9, 15, 15, 10, 4, 20, 15, 10, 4, 20,
            15, 11, 4, 1, 20);
        StringAssert.Contains(transcript, "The Wolf has 8 health points remaining.");
        StringAssert.Contains(transcript, "The Wolf has 1 health points remaining.");
        StringAssert.Contains(transcript, "Weapon damage bonus: 3");
        StringAssert.Contains(transcript, "Smolderfang's health points are now 13.");
        Assert.AreEqual(2, transcript.Split("Gold: 5").Length - 1);
        StringAssert.Contains(transcript, "The Wolf yelps, taking 7 damage.");
    }

    [TestMethod]
    public void ArmorAndHealth_PersistAcrossRetreatVictoryAndReturnToTravel()
    {
        string transcript = Journey("s\n1\n3\nw\na\nr\nw\na\ns\n3\nn\na\nr\n",
            8, 9, 5, 20, 8, 11, 6, // First round: armor reduces six to three.
            15, 10, 5, // Return and kill the wolf before it retaliates.
            20, 20); // Both miss in northern combat; stats show retained equipment.
        StringAssert.Contains(transcript, "Hero's health points are now 17.");
        StringAssert.Contains(transcript, "Health Points: 17");
        StringAssert.Contains(transcript, "Armor protection: 3");
        StringAssert.Contains(transcript, "Weapon: long sword");
        StringAssert.Contains(transcript, "The Wolf falls defeated.");
        Assert.AreEqual(2, transcript.Split("Gold: 5").Length - 1);
        Assert.IsFalse(transcript.Contains("Gold: 15"));
    }

    [TestMethod]
    public void WolfDefeat_EndsAdventureWithoutReturningToTravel()
    {
        string transcript = Journey("w\na\na\na\na\n",
            15, 9, 12,
            20, 15, 11, 6, 20, 15, 11, 6, 20, 15, 11, 6, 20, 15, 11, 6);
        StringAssert.Contains(transcript, "Hero's health points are now 0.");
        var messages = new Messages();
        messages.ReadDictionary();
        StringAssert.Contains(transcript, "The Wolf overcomes you. Your adventure ends.");
        Assert.IsFalse(transcript.Contains("dragon"));
        Assert.AreEqual(1, transcript.Split(messages.GetMessage("path_prompt_full")).Length - 1);
    }

    [TestMethod]
    public void InvalidWolfChoices_RepeatPromptWithoutRoundsOrHealthChanges()
    {
        string transcript = Journey("w\nwrong\n\n Attack \nwrong\n\nattack\nretreat\ne\n",
            8, 9, 12, 15, 10, 2, 20, 15, 10, 2, 20);
        StringAssert.Contains(transcript, "The Wolf has 8 health points remaining.");
        Assert.AreEqual(2, transcript.Split("The Wolf bites at Hero.").Length - 1);
        var messages = new Messages();
        messages.ReadDictionary();
        Assert.AreEqual(4, transcript.Split(messages.GetMessage("invalid")).Length - 1);
        Assert.AreEqual(7, transcript.Split(messages.GetMessage("attack_prompt")).Length - 1);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ClosedWolfInput_RetreatsThenClosedTravelExits(bool inCombat)
    {
        int[] rounds = inCombat ? [20, 20] : [];
        string transcript = Journey("w\n" + (inCombat ? "a\n" : ""), [8, 9, 12, .. rounds]);
        var messages = new Messages();
        messages.ReadDictionary();
        StringAssert.Contains(transcript, messages.GetMessage("adventure_ends"));
        Assert.AreEqual(2, transcript.Split(messages.GetMessage("path_prompt_full")).Length - 1);
        Assert.IsFalse(transcript.Contains(messages.GetMessage("invalid")));
    }

    [TestMethod]
    public void DefendedAndMissedAttacks_PreserveHealthAndUseAnimalNarration()
    {
        // First round: both defend at agility. Second round: both miss above strength.
        string transcript = Journey("w\na\na\nr\nw\nr\ne\n",
            8, 9, 12, 15, 9, 8, 10, 16, 9);
        StringAssert.Contains(transcript, "The Wolf dodges your attack.");
        StringAssert.Contains(transcript, "Hero avoids the Wolf's bite.");
        StringAssert.Contains(transcript, "Your attack misses the Wolf.");
        StringAssert.Contains(transcript, "The Wolf's bite misses Hero.");
        Assert.AreEqual(2, transcript.Split("Health Points: 12").Length - 1);
        Assert.IsFalse(transcript.Contains("Smolderfang"));
        Assert.IsFalse(transcript.Contains("Dragon"));
    }

    [TestMethod]
    public void WestInitialRetreat_ShowsWolfAndReturnsToTravel()
    {
        // No combat rolls are available: initial retreat must not start a round.
        string transcript = Journey("west\nr\ne\n", 8, 9, 12);
        var messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();
        StringAssert.Contains(transcript, "Wolf");
        Assert.IsFalse(transcript.Contains(messages.GetMessage("path_invalid")),
            "West must be accepted, and Retreat must be read by the wolf encounter.");
        Assert.AreEqual(2, transcript.Split(messages.GetMessage("path_prompt_full")).Length - 1,
            "Initial retreat must return to travel before exit.");
        StringAssert.Contains(transcript, messages.GetMessage("adventure_ends"));
        Assert.IsFalse(transcript.Contains(messages.GetMessage("retreat")),
            "Wolf retreat must not use the dragon's adventure-ending outcome.");
    }
}
