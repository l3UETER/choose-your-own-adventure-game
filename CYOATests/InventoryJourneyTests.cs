[TestClass]
[DoNotParallelize]
public class InventoryJourneyTests
{
    private sealed class BoundedReader(string script) : StringReader(script)
    {
        private int remainingReads = 40;

        public override string? ReadLine()
        {
            Assert.IsTrue(remainingReads-- > 0, "The journey did not terminate after scripted input.");
            return base.ReadLine();
        }
    }

    [TestMethod]
    public void TravelInventoryShortCommand_DisplaysCurrentEquipmentAndGoldThenAcceptsExit()
    {
        var messages = English();
        string transcript = Journey("i\ne\n");
        string travelPrompt = messages.GetMessage("path_prompt_full");
        string[] travelVisits = transcript.Split(travelPrompt);
        Assert.AreEqual(3, travelVisits.Length, "Inventory must immediately return to travel selection.");
        StringAssert.Contains(travelVisits[2], messages.GetMessage("adventure_ends"),
            "Exit directly after inventory must end the adventure without an acknowledgment.");
        Assert.IsFalse(transcript.Contains(messages.GetMessage("path_invalid")),
            "The travel menu must accept 'i' as inventory instead of reporting an invalid choice.");
        // Inspect only the output between travel prompts, excluding character creation stats.
        StringAssert.Contains(travelVisits[1], "long sword");
        StringAssert.Contains(travelVisits[1], "Gold: 20");
    }

    [TestMethod]
    public void AfterBuyingArmor_InventoryShowsCurrentEquipmentAndRemainingGold()
    {
        string transcript = Journey("s\n1\n3\ni\ne\n");
        string inventory = transcript.Split(English().GetMessage("inventory_header"))[1]
            .Split(English().GetMessage("path_prompt_full"))[0];
        StringAssert.Contains(inventory, "Weapon: long sword");
        StringAssert.Contains(inventory, "Maximum base weapon damage: 12");
        StringAssert.Contains(inventory, "Magical armor: Owned; damage reduction: 3");
        StringAssert.Contains(inventory, "Weapon enchantment: Not owned; damage bonus: 0");
        StringAssert.Contains(inventory, "Gold: 5");
        Assert.IsFalse(transcript.Contains(English().GetMessage("path_invalid")));
        StringAssert.Contains(transcript, English().GetMessage("adventure_ends"));
    }

    private static string[] InventoryDisplays(string transcript, Messages messages) =>
        transcript.Split(messages.GetMessage("inventory_header")).Skip(1)
            .Select(block => block.Split(messages.GetMessage("path_prompt_full"))[0].Trim()).ToArray();

    private static void AssertInventory(string inventory, Messages messages, bool armor, bool enchantment, int gold)
    {
        StringAssert.Contains(inventory, messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay("long sword"));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("inventory_base_damage"), 12));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("inventory_armor"),
            messages.GetMessage(armor ? "merchant_owned" : "merchant_not_owned"), armor ? 3 : 0));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("inventory_enchantment"),
            messages.GetMessage(enchantment ? "merchant_owned" : "merchant_not_owned"), enchantment ? 3 : 0));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("merchant_gold"), gold));
        Assert.IsFalse(inventory.Contains("["));
        Assert.AreEqual(1, inventory.Split(messages.GetMessage("stats_weapon")).Length - 1,
            "Inspection must show a single active weapon.");
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void AfterBuyingEnchantment_BothAliasesShowSeparateBonusAndUnchangedBaseDamage(string language)
    {
        var messages = InLanguage(language);
        string transcript = Journey("s\n2\n3\ni\ninventory\ne\n", language: language);
        string[] inventories = InventoryDisplays(transcript, messages);
        Assert.AreEqual(2, inventories.Length);
        foreach (string inventory in inventories)
            AssertInventory(inventory, messages, false, true, 5);
        Assert.AreEqual(inventories[0], inventories[1]);
        Assert.IsFalse(transcript.Contains(messages.GetMessage("path_invalid")));
        StringAssert.Contains(transcript, messages.GetMessage("adventure_ends"));
    }

    [TestMethod]
    [DataRow("English", "1", "2")]
    [DataRow("Spanish", "2", "1")]
    [DataRow("French", "1", "2")]
    [DataRow("Italian", "2", "1")]
    public void FailedPurchases_LeaveInventoryIdenticalToBeforeTheAttempt(string language, string purchased, string other)
    {
        var messages = InLanguage(language);
        string transcript = Journey($"s\n{purchased}\n3\ni\ns\n{other}\n3\ninventory\ns\n{purchased}\n3\ni\ne\n",
            language: language);
        string[] inventories = InventoryDisplays(transcript, messages);
        Assert.AreEqual(3, inventories.Length);
        AssertInventory(inventories[0], messages, purchased == "1", purchased == "2", 5);
        Assert.AreEqual(inventories[0], inventories[1], "Insufficient funds must not change inventory.");
        Assert.AreEqual(inventories[0], inventories[2], "Buying an owned upgrade must not change inventory.");
        StringAssert.Contains(transcript, messages.GetMessage("merchant_insufficient_gold"));
        StringAssert.Contains(transcript, messages.GetMessage(purchased == "1" ? "merchant_armor_owned" : "merchant_enchantment_owned"));
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void WolfReward_FundsBothUpgradesAndInventoryCannotRestoreRewardEligibility(string language)
    {
        var messages = InLanguage(language);
        // A real starting balance of 20 plus one ten-gold reward pays both fifteen-gold prices.
        string transcript = Journey("i\nw\na\ninventory\ns\n1\n2\n3\ni\ninventory\nw\ni\nw\ninventory\ne\n",
            language: language, combatRolls: [8, 9, 5, 15, 10, 5]);
        string[] inventories = InventoryDisplays(transcript, messages);
        Assert.AreEqual(6, inventories.Length);
        AssertInventory(inventories[0], messages, false, false, 20);
        AssertInventory(inventories[1], messages, false, false, 30);
        foreach (string inventory in inventories.Skip(2))
            AssertInventory(inventory, messages, true, true, 0);
        Assert.AreEqual(1, transcript.Split(string.Format(messages.GetMessage("wolf_reward"), 10)).Length - 1);
        Assert.AreEqual(2, transcript.Split(messages.GetMessage("wolf_route_clear")).Length - 1);
        Assert.IsFalse(transcript.Contains(messages.GetMessage("path_invalid")));
        StringAssert.Contains(transcript, messages.GetMessage("adventure_ends"));
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void RepeatedInventory_PreservesWoundsEncountersUpgradesRewardsAndSubsequentCombat(string language)
    {
        var messages = InLanguage(language);
        string[] legs = ["s\n1\n3\n", "w\na\nr\n", "w\na\n", "s\n2\n3\n", "w\n", "n\na\nr\n"];
        // Wound both combatants, retreat, finish the wolf, buy the second upgrade, then fight the dragon.
        int[] rolls = [8, 9, 12, 15, 10, 4, 8, 11, 6, 15, 10, 8, 15, 11, 4, 1, 10, 11, 6];
        string baseline = Journey(string.Concat(legs), language: language, combatRolls: rolls);
        string inspected = Journey("i\ninventory\n" + string.Join("i\ninventory\n", legs),
            language: language, combatRolls: rolls);

        string prompt = messages.GetMessage("path_prompt_full");
        string header = messages.GetMessage("inventory_header");
        string withoutInspections = string.Join(prompt, inspected.Split(prompt).Where(visit => !visit.Contains(header)));
        Assert.AreEqual(baseline, withoutInspections,
            "With identical dice, removing inspection visits must leave the complete observable journey unchanged.");
        string[] inventories = InventoryDisplays(inspected, messages);
        Assert.AreEqual(12, inventories.Length);
        (bool armor, bool enchantment, int gold)[] states =
            [(false, false, 20), (true, false, 5), (true, false, 5), (true, false, 15), (true, true, 0), (true, true, 0)];
        for (int visit = 0; visit < states.Length; visit++)
        {
            var state = states[visit];
            AssertInventory(inventories[visit * 2], messages, state.armor, state.enchantment, state.gold);
            Assert.AreEqual(inventories[visit * 2], inventories[visit * 2 + 1]);
        }
        StringAssert.Contains(inspected, string.Format(messages.GetMessage("wolf_health_now"), 8));
        StringAssert.Contains(inspected, messages.GetMessage("stats_health") + 8); // Wolf health retained on return.
        StringAssert.Contains(inspected, string.Format(messages.GetMessage("player_health_now"), "Hero", 17));
        StringAssert.Contains(inspected, messages.GetMessage("stats_health") + 17); // Wounds retained into dragon combat.
        StringAssert.Contains(inspected, string.Format(messages.GetMessage("dragon_health_now"), "Smolderfang", 13));
        StringAssert.Contains(inspected, string.Format(messages.GetMessage("dragon_damage_dealt"), "Smolderfang", 3));
        StringAssert.Contains(inspected, string.Format(messages.GetMessage("player_health_now"), "Hero", 14));
        StringAssert.Contains(inspected, messages.GetMessage("retreat_combat"));
        Assert.AreEqual(1, inspected.Split(string.Format(messages.GetMessage("wolf_reward"), 10)).Length - 1);
    }

    [TestMethod]
    public void InventoryVisits_DoNotCreateSaveFilesOrModifyExistingFilesInWorkingDirectory()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        var directory = Directory.CreateTempSubdirectory("cyoa-inventory-");
        string existingFile = Path.Combine(directory.FullName, "existing-save.json");
        File.WriteAllText(existingFile, "existing save must remain untouched");
        DateTime originalWriteTime = File.GetLastWriteTimeUtc(existingFile);
        try
        {
            Directory.SetCurrentDirectory(directory.FullName);
            Journey("i\ninventory\ns\n1\n3\ni\ninventory\ne\n");
            CollectionAssert.AreEquivalent(new[] { existingFile }, Directory.GetFiles(directory.FullName, "*", SearchOption.AllDirectories));
            Assert.AreEqual("existing save must remain untouched", File.ReadAllText(existingFile));
            Assert.AreEqual(originalWriteTime, File.GetLastWriteTimeUtc(existingFile));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            directory.Delete(recursive: true);
        }
    }

    private static Messages English() => InLanguage("English");

    private static Messages InLanguage(string language)
    {
        var messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    private static string Journey(string travel, string occupation = "fighter", string language = "English",
        params int[] combatRolls)
    {
        var messages = InLanguage(language);
        string languageChoice = language switch { "Spanish" => "2", "French" => "3", "Italian" => "4", _ => "1" };
        string profession = messages.TranslateOccupationForDisplay(char.ToUpperInvariant(occupation[0]) + occupation[1..]);
        var random = new RepeatableCombatTests.ScriptedRandom([10, 10, 20, 14, 1, 10, 19, .. combatRolls]);
        using var input = new BoundedReader(
            languageChoice + "\n1\n" + messages.TranslateRaceForDisplay("Human") + "\nHero\n" +
            profession + "\nroll\nroll\nroll\n2\n" + travel);
        using var output = new StringWriter();
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var originalColor = Console.ForegroundColor;
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
    [DataRow("i")]
    [DataRow("inventory")]
    [DataRow("I")]
    [DataRow("INVENTORY")]
    [DataRow("InVeNtOrY")]
    [DataRow("  i  ")]
    [DataRow(" \tInVeNtOrY\t ")]
    public void InventoryAliases_AreAcceptedAndValidatedWithCaseAndSurroundingWhitespace(string command)
    {
        string transcript = Journey(command + "\ne\n");
        Assert.IsFalse(transcript.Contains(English().GetMessage("path_invalid")),
            $"Travel must accept '{command}'.");
        Assert.AreEqual(1, transcript.Split("Gold: 20").Length - 1);
        Assert.IsTrue(new Game().IsValidPathChoice(command), "Validation must agree with travel dispatch.");
    }

    [TestMethod]
    [DataRow("fighter", "long sword", 12)]
    [DataRow("magician", "lightning bolt spell", 12)]
    [DataRow("thief", "dagger", 6)]
    [DataRow("archer", "long bow", 8)]
    public void StartingInventory_ShowsProfessionWeaponAndSeparateUpgradeEffects(
        string occupation, string weapon, int damage)
    {
        string inventory = Journey("i\ne\n", occupation).Split(English().GetMessage("path_prompt_full"))[1];
        StringAssert.Contains(inventory, "Weapon: " + weapon);
        StringAssert.Contains(inventory, $"Maximum base weapon damage: {damage}");
        StringAssert.Contains(inventory, "Magical armor: Not owned; damage reduction: 0");
        StringAssert.Contains(inventory, "Weapon enchantment: Not owned; damage bonus: 0");
        StringAssert.Contains(inventory, "Gold: 20");
    }

    [TestMethod]
    [DataRow("English", "Weapon: None")]
    [DataRow("Spanish", "Arma: Ninguna")]
    [DataRow("French", "Arme: Aucune")]
    [DataRow("Italian", "Arma: Nessuna")]
    public void MissingWeapon_ShowsExplicitAbsenceAndActualUpgradesWithoutInventingBaseDamage(
        string language, string absence)
    {
        var messages = InLanguage(language);
        var random = new RepeatableCombatTests.ScriptedRandom();
        var player = new Player(new Die(random))
        {
            Gold = 7, HasMagicalArmor = true, HasWeaponEnchantment = true
        };
        var originalOut = Console.Out;
        using var output = new StringWriter();
        try
        {
            Console.SetOut(output);
            player.DisplayInventory(messages);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        string inventory = output.ToString();
        StringAssert.Contains(inventory, absence);
        Assert.IsFalse(inventory.Contains(messages.GetMessage("inventory_base_damage").Split("{0}")[0]));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("inventory_armor"), messages.GetMessage("merchant_owned"), 3));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("inventory_enchantment"), messages.GetMessage("merchant_owned"), 3));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("merchant_gold"), 7));
        Assert.IsFalse(inventory.Contains("["));
        Assert.IsNull(player.Weapon);
        Assert.AreEqual(7, player.Gold);
        Assert.IsTrue(player.HasMagicalArmor);
        Assert.IsTrue(player.HasWeaponEnchantment);
        random.AssertExhausted();
    }
    [TestMethod]
    [DataRow("English", "Weapon: long sword", "Maximum base weapon damage: 12",
        "Magical armor: Not owned; damage reduction: 0", "Weapon enchantment: Not owned; damage bonus: 0")]
    [DataRow("Spanish", "Arma: espada larga", "Daño base máximo del arma: 12",
        "Armadura mágica: Sin adquirir; reducción de daño: 0", "Encantamiento del arma: Sin adquirir; daño adicional: 0")]
    [DataRow("French", "Arme: epee longue", "Dégâts de base maximaux de l'arme : 12",
        "Armure magique : Non acquis; réduction des dégâts : 0", "Enchantement de l'arme : Non acquis; bonus de dégâts : 0")]
    [DataRow("Italian", "Arma: spada lunga", "Danno base massimo dell'arma: 12",
        "Armatura magica: Non posseduto; riduzione del danno: 0", "Incantamento dell'arma: Non posseduto; danno aggiuntivo: 0")]
    public void BothInventoryCommands_DisplayLocalizedEquipmentAndEffects(
        string language, string weapon, string damage, string armor, string enchantment)
    {
        var messages = InLanguage(language);
        string? previousInventory = null;
        foreach (string command in new[] { "i", "inventory" })
        {
            string transcript = Journey(command + "\ne\n", language: language);
            string[] visits = transcript.Split(messages.GetMessage("path_prompt_full"));
            Assert.AreEqual(3, visits.Length);
            string inventory = visits[1];
            StringAssert.Contains(inventory, weapon);
            StringAssert.Contains(inventory, damage);
            StringAssert.Contains(inventory, armor);
            StringAssert.Contains(inventory, enchantment);
            StringAssert.Contains(inventory, string.Format(messages.GetMessage("merchant_gold"), 20));
            Assert.IsFalse(transcript.Contains("["), "No missing localization keys may appear.");
            Assert.IsFalse(transcript.Contains(messages.GetMessage("path_invalid")));
            StringAssert.Contains(visits[2], messages.GetMessage("adventure_ends"));
            if (previousInventory != null)
                Assert.AreEqual(previousInventory, inventory, "Both aliases must display equivalent information.");
            previousInventory = inventory;
        }
    }
    [TestMethod]
    [DataRow("English", "Inventory")]
    [DataRow("Spanish", "Inventario")]
    [DataRow("French", "Inventaire")]
    [DataRow("Italian", "Inventario")]
    public void TravelPrompt_AdvertisesBothInventoryTokensAndRetainsRoutes(string language, string label)
    {
        string prompt = InLanguage(language).GetMessage("path_prompt_full");
        StringAssert.Contains(prompt, label);
        StringAssert.Contains(prompt, "'i'");
        StringAssert.Contains(prompt, "'inventory'");
        foreach (string token in new[] { "n", "north", "s", "south", "w", "west", "e", "exit" })
            StringAssert.Contains(prompt, "'" + token + "'");
        StringAssert.Contains(Journey("e\n", language: language), prompt);
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void InventoryMessages_AreNonemptyWithValidFormattingAndMatchingPlaceholders(string language)
    {
        string[] keys = ["inventory_header", "inventory_none", "inventory_base_damage", "inventory_armor",
            "inventory_enchantment", "stats_weapon", "merchant_gold", "merchant_owned", "merchant_not_owned",
            "path_prompt_full", "path_invalid"];
        var messages = InLanguage(language);
        var english = English();
        foreach (string key in keys)
        {
            string message = messages.GetMessage(key);
            Assert.AreNotEqual($"[{key}]", message);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message), $"Empty {language}/{key}");
            string[] expected = System.Text.RegularExpressions.Regex.Matches(english.GetMessage(key), @"\{[^{}]+\}")
                .Select(match => match.Value).OrderBy(value => value).ToArray();
            string[] actual = System.Text.RegularExpressions.Regex.Matches(message, @"\{[^{}]+\}")
                .Select(match => match.Value).OrderBy(value => value).ToArray();
            CollectionAssert.AreEqual(expected, actual, $"Placeholder mismatch: {language}/{key}");
            string.Format(message, messages.GetMessage("merchant_not_owned"), 0);
        }
    }

    [TestMethod]
    [DataRow("n")]
    [DataRow("north")]
    [DataRow("s")]
    [DataRow("south")]
    [DataRow("w")]
    [DataRow("west")]
    [DataRow("e")]
    [DataRow("exit")]
    public void Inventory_ReturnsImmediatelyAndPreservesFollowingRoute(string route)
    {
        bool west = route is "w" or "west";
        string continuation = route is "s" or "south" ? "3\ne\n" :
            route is "e" or "exit" ? "" : "r\ne\n";
        int[] rolls = west ? [8, 9, 12] : [];
        string baseline = Journey(route + "\n" + continuation, combatRolls: rolls);
        string inspected = Journey("i\ninventory\n" + route + "\n" + continuation, combatRolls: rolls);
        string prompt = English().GetMessage("path_prompt_full");
        // After the two inspections, all subsequent route output must be identical.
        Assert.AreEqual(baseline[baseline.IndexOf(prompt, StringComparison.Ordinal)..],
            prompt + string.Join(prompt, inspected.Split(prompt).Skip(3)));
        Assert.AreEqual(2, inspected.Split(English().GetMessage("inventory_header")).Length - 1);
        Assert.IsTrue(new Game().IsValidPathChoice(route));
    }

    [TestMethod]
    [DataRow("in ventory")]
    [DataRow("in\tventory")]
    [DataRow("ii")]
    [DataRow("nonsense")]
    [DataRow("")]
    public void InvalidTravelInput_RemainsInvalidAndDoesNotOpenInventory(string command)
    {
        string transcript = Journey(command + "\ne\n");
        Assert.AreEqual(1, transcript.Split(English().GetMessage("path_invalid")).Length - 1);
        Assert.IsFalse(transcript.Contains(English().GetMessage("inventory_header")));
        Assert.IsFalse(new Game().IsValidPathChoice(command));
        StringAssert.Contains(transcript, English().GetMessage("adventure_ends"));
    }

    [TestMethod]
    public void Inventory_ThenEndOfInput_EndsAdventureWithoutAnAcknowledgmentOrInvalidChoice()
    {
        string transcript = Journey("inventory\n");
        Assert.AreEqual(2, transcript.Split(English().GetMessage("path_prompt_full")).Length - 1);
        Assert.AreEqual(1, transcript.Split(English().GetMessage("inventory_header")).Length - 1);
        Assert.IsFalse(transcript.Contains(English().GetMessage("path_invalid")));
        StringAssert.Contains(transcript, English().GetMessage("adventure_ends"));
    }

    [TestMethod]
    [DataRow("s\ni\ninventory\n3\ne\n", "merchant_invalid")]
    [DataRow("n\ni\ninventory\nr\n", "invalid")]
    public void Inventory_IsNotAddedToMerchantOrCombatMenus(string travel, string invalidKey)
    {
        string transcript = Journey(travel);
        Assert.AreEqual(2, transcript.Split(English().GetMessage(invalidKey)).Length - 1);
        Assert.IsFalse(transcript.Contains(English().GetMessage("inventory_header")));
        Assert.IsFalse(new Game().IsValidMenuChoice("i"));
        Assert.IsFalse(new Game().IsValidMenuChoice("inventory"));
        Assert.IsFalse(new Game().IsValidCombatChoice("i"));
        Assert.IsFalse(new Game().IsValidCombatChoice("inventory"));
    }

    [TestMethod]
    [DataRow("English", false, false)]
    [DataRow("Spanish", true, false)]
    [DataRow("French", false, true)]
    [DataRow("Italian", true, true)]
    public void CurrentInventory_ReadsLiveStateWithoutMutationsOrRandomRolls(string language, bool armor, bool enchantment)
    {
        var random = new RepeatableCombatTests.ScriptedRandom();
        var weapon = new Weapon("dagger", 9, "custom art");
        var player = new Player(new Die(random))
        {
            Name = "Scout", Race = "Elf", Occupation = "Thief", Strength = 8, Agility = 16,
            HealthPoints = 4, Weapon = weapon, Gold = 7, HasMagicalArmor = armor, HasWeaponEnchantment = enchantment
        };
        string before = System.Text.Json.JsonSerializer.Serialize(player);
        var messages = InLanguage(language);
        var originalOut = Console.Out;
        using var output = new StringWriter();
        try
        {
            Console.SetOut(output);
            player.DisplayInventory(messages);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        string inventory = output.ToString();
        StringAssert.Contains(inventory, messages.TranslateWeaponForDisplay("dagger"));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("inventory_base_damage"), 9));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("inventory_armor"),
            messages.GetMessage(armor ? "merchant_owned" : "merchant_not_owned"), armor ? 3 : 0));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("inventory_enchantment"),
            messages.GetMessage(enchantment ? "merchant_owned" : "merchant_not_owned"), enchantment ? 3 : 0));
        StringAssert.Contains(inventory, string.Format(messages.GetMessage("merchant_gold"), 7));
        Assert.AreEqual(before, System.Text.Json.JsonSerializer.Serialize(player), "Inspection must preserve all public player and weapon state.");
        Assert.AreSame(weapon, player.Weapon);
        random.AssertExhausted();
    }

    [TestMethod]
    public void InventoryTokens_IgnoreCaseEvenUnderTurkishCulture()
    {
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
            string transcript = Journey("I\nINVENTORY\ne\n");
            Assert.IsFalse(transcript.Contains(English().GetMessage("path_invalid")));
            Assert.AreEqual(2, transcript.Split(English().GetMessage("inventory_header")).Length - 1);
            Assert.IsTrue(new Game().IsValidPathChoice("I"));
            Assert.IsTrue(new Game().IsValidPathChoice("INVENTORY"));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
