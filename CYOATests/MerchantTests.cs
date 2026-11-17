[TestClass]
[DoNotParallelize]
public class MerchantTests
{
    private static Messages English()
    {
        var messages = new Messages();
        messages.ReadDictionary();
        return messages;
    }

    // Fail a broken input loop promptly instead of hanging the test process.
    private sealed class BoundedReader(string script) : StringReader(script)
    {
        private int _remainingReads = 80;
        public override string? ReadLine()
        {
            Assert.IsTrue(_remainingReads-- > 0, "The journey did not terminate after scripted input.");
            return base.ReadLine();
        }
    }

    private static string Journey(string travel, params int[] combatRolls)
    {
        var random = new RepeatableCombatTests.ScriptedRandom([10, 10, 20, 14, 1, 10, 19, .. combatRolls]);
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var originalColor = Console.ForegroundColor;
        using var input = new BoundedReader("1\n1\nhuman\nHero\nfighter\nroll\nroll\nroll\n2\n" + travel);
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
    public void SouthLeaveSouthLeaveNorth_DisplaysBothOffersWithoutSpending()
    {
        string transcript = Journey("s\n3\ns\n3\nn\nr\n");

        Assert.AreEqual(2, transcript.Split("Gold: 20").Length - 1);
        Assert.AreEqual(2, transcript.Split("1. Magical armor - 15 gold").Length - 1);
        Assert.AreEqual(2, transcript.Split("2. Enchant existing weapon - 15 gold").Length - 1);
        StringAssert.Contains(transcript, "reduces incoming damage by 3 (minimum 0)");
        StringAssert.Contains(transcript, "adds 3 damage on successful, undefended hits");
        Assert.AreEqual(4, transcript.Split("Not owned").Length - 1);
        StringAssert.Contains(transcript, "3. Leave");
        StringAssert.Contains(transcript, English().GetMessage("retreat"));
    }

    [TestMethod]
    public void MerchantEndOfInput_ReturnsToTravelAndEndsWithoutLooping()
    {
        string transcript = Journey("s\n");

        Assert.AreEqual(1, transcript.Split("Gold: 20").Length - 1);
        Assert.AreEqual(2, transcript.Split(English().GetMessage("path_prompt_full")).Length - 1);
        StringAssert.Contains(transcript, English().GetMessage("adventure_ends"));
        Assert.IsFalse(transcript.Contains(English().GetMessage("merchant_invalid")));
    }

    [TestMethod]
    [DataRow("nonsense")]
    [DataRow("")]
    [DataRow("0")]
    [DataRow("4")]
    public void InvalidOrDeferredChoice_KeepsMenuOpenAndPreservesStartingState(string choice)
    {
        string transcript = Journey("s\n" + choice + "\n3\ns\n3\nn\nr\n");

        Assert.AreEqual(3, transcript.Split("Gold: 20").Length - 1);
        Assert.AreEqual(6, transcript.Split("Not owned").Length - 1);
        StringAssert.Contains(transcript, English().GetMessage(choice == "1" || choice == "2"
            ? "merchant_purchase_unavailable" : "merchant_invalid"));
        StringAssert.Contains(transcript, English().GetMessage("retreat"));
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void MerchantMessages_AreNonemptyAndPreserveEveryPlaceholderOccurrence(string language)
    {
        string[] keys = ["south_path_narrative", "merchant_header", "merchant_gold", "merchant_armor_offer",
            "merchant_enchantment_offer", "merchant_leave", "merchant_owned", "merchant_not_owned",
            "merchant_invalid", "merchant_purchase_unavailable", "merchant_armor_purchased",
            "merchant_armor_owned", "merchant_insufficient_gold", "stats_armor_protection",
            "merchant_enchantment_owned", "merchant_enchantment_purchased", "stats_weapon_bonus"];
        var english = English();
        var translated = new Messages();
        translated.SetCurrentLanguage(language);
        translated.ReadDictionary();

        foreach (string key in keys)
        {
            string message = translated.GetMessage(key);
            Assert.AreNotEqual($"[{key}]", message, $"Missing {language}/{key}");
            Assert.IsFalse(string.IsNullOrWhiteSpace(message), $"Empty {language}/{key}");
            string[] expected = System.Text.RegularExpressions.Regex.Matches(english.GetMessage(key), @"\{[^{}]+\}")
                .Select(match => match.Value).OrderBy(value => value).ToArray();
            string[] actual = System.Text.RegularExpressions.Regex.Matches(message, @"\{[^{}]+\}")
                .Select(match => match.Value).OrderBy(value => value).ToArray();
            CollectionAssert.AreEqual(expected, actual, $"Placeholder mismatch in {language}/{key}");
            // Also exercise the format parser, including localized ownership labels.
            string.Format(message, translated.GetMessage("merchant_not_owned"));
        }
    }

    [TestMethod]
    public void NewPlayers_HaveIndependentStartingEconomy()
    {
        var first = new Player();
        first.Gold = 5;
        first.HasMagicalArmor = true;
        first.HasWeaponEnchantment = true;

        var second = new Player();

        Assert.AreEqual(20, second.Gold);
        Assert.IsFalse(second.HasMagicalArmor);
        Assert.IsFalse(second.HasWeaponEnchantment);
        Assert.AreEqual(5, first.Gold);
        Assert.IsTrue(first.HasMagicalArmor);
        Assert.IsTrue(first.HasWeaponEnchantment);
    }

    [TestMethod]
    public void BuyArmor_RevisitAndSelectAgain_ChargesOnlyOnceAndKeepsMenuOpen()
    {
        string transcript = Journey("s\n1\n1\n3\ns\n1\n3\nn\nr\n");

        Assert.AreEqual(1, transcript.Split("Gold: 20").Length - 1);
        Assert.AreEqual(4, transcript.Split("Gold: 5").Length - 1);
        Assert.AreEqual(1, transcript.Split("Magical armor equipped.").Length - 1);
        Assert.AreEqual(2, transcript.Split("You already own magical armor.").Length - 1);
        Assert.AreEqual(4, transcript.Split("Status: Owned").Length - 1);
        StringAssert.Contains(transcript, English().GetMessage("retreat"));
    }

    [TestMethod]
    [DataRow(15, 0, true, "merchant_armor_purchased")]
    [DataRow(14, 14, false, "merchant_insufficient_gold")]
    [DataRow(0, 0, false, "merchant_insufficient_gold")]
    public void ArmorPurchase_RespectsAvailableGoldAndRemainsOpen(
        int startingGold, int expectedGold, bool expectedArmor, string feedbackKey)
    {
        var player = new Player { Gold = startingGold };
        var messages = English();
        string transcript = Visit(player, "1\n3\n", messages);

        Assert.AreEqual(expectedGold, player.Gold);
        Assert.AreEqual(expectedArmor, player.HasMagicalArmor);
        Assert.IsFalse(player.HasWeaponEnchantment);
        StringAssert.Contains(transcript, messages.GetMessage(feedbackKey));
        Assert.AreEqual(2, transcript.Split(messages.GetMessage("merchant_header")).Length - 1);
        StringAssert.Contains(transcript, string.Format(messages.GetMessage("merchant_gold"), expectedGold));
    }

    [TestMethod]
    [DataRow(30)]
    [DataRow(0)]
    public void OwnedArmor_TakesPrecedenceOverFundsAndDoesNotCharge(int gold)
    {
        var player = new Player { Gold = gold, HasMagicalArmor = true };
        string transcript = Visit(player, "1\n", English());

        Assert.AreEqual(gold, player.Gold);
        Assert.IsTrue(player.HasMagicalArmor);
        StringAssert.Contains(transcript, English().GetMessage("merchant_armor_owned"));
        Assert.IsFalse(transcript.Contains(English().GetMessage("merchant_insufficient_gold")));
        Assert.AreEqual(2, transcript.Split("Status: Owned").Length - 1);
    }

    [TestMethod]
    public void SouthBuyLeaveRevisitLeaveNorth_ArmorPersistsIntoRepeatedDragonHits()
    {
        // Player misses, dragon deals 6 reduced to 3, then repeat to prove armor lasts.
        string transcript = Journey("s\n1\n3\ns\n3\nn\na\na\nr\n",
            20, 10, 11, 6, 20, 10, 11, 6);

        Assert.AreEqual(1, transcript.Split("Gold: 20").Length - 1);
        Assert.AreEqual(2, transcript.Split("Gold: 5").Length - 1);
        Assert.AreEqual(2, transcript.Split("Status: Owned").Length - 1);
        StringAssert.Contains(transcript, "Armor protection: 3");
        Assert.AreEqual(2, transcript.Split("Smolderfang deals 3 damage.").Length - 1);
        StringAssert.Contains(transcript, "Hero's health points are now 17.");
        StringAssert.Contains(transcript, "Hero's health points are now 14.");
        StringAssert.Contains(transcript, English().GetMessage("retreat_combat"));
    }

    [TestMethod]
    public void BuyArmor_EndOfInput_ReturnsToTravelWithUpdatedBalance()
    {
        string transcript = Journey("s\n1\n");

        StringAssert.Contains(transcript, "Gold: 5");
        StringAssert.Contains(transcript, "Status: Owned");
        StringAssert.Contains(transcript, English().GetMessage("adventure_ends"));
    }

    [TestMethod]
    [DataRow(15, 0, true)]
    [DataRow(14, 14, false)]
    [DataRow(0, 0, false)]
    public void EnchantmentPurchase_RespectsFundsAndKeepsExistingWeapon(int gold, int balance, bool owned)
    {
        var weapon = new Weapon("dagger", 6, "dagger");
        var player = new Player { Gold = gold, Weapon = weapon, Occupation = "Thief" };
        var messages = English();
        string output = Visit(player, "2\n3\n", messages);
        Assert.AreEqual(balance, player.Gold);
        Assert.AreEqual(owned, player.HasWeaponEnchantment);
        Assert.IsFalse(player.HasMagicalArmor);
        Assert.AreSame(weapon, player.Weapon);
        Assert.AreEqual("Thief", player.Occupation);
        Assert.AreEqual(2, output.Split(messages.GetMessage("merchant_header")).Length - 1);
        StringAssert.Contains(output, messages.GetMessage(owned ? "merchant_enchantment_purchased" : "merchant_insufficient_gold"));
        StringAssert.Contains(output, $"Gold: {balance}");
    }

    [TestMethod]
    [DataRow(30)]
    [DataRow(0)]
    public void OwnedEnchantment_NeverChargesOrStacksEvenAcrossVisits(int gold)
    {
        var player = new Player { Gold = gold, HasWeaponEnchantment = true };
        var messages = English();
        string output = Visit(player, "2\n2\n3\n", messages) + Visit(player, "2\n", messages);
        Assert.AreEqual(gold, player.Gold);
        Assert.AreEqual(3, player.WeaponDamageBonus);
        Assert.AreEqual(3, output.Split(messages.GetMessage("merchant_enchantment_owned")).Length - 1);
        Assert.IsFalse(output.Contains(messages.GetMessage("merchant_insufficient_gold")));
        Assert.AreEqual(5, output.Split("Status: Owned").Length - 1);
    }

    [TestMethod]
    [DataRow("1", "2")]
    [DataRow("2", "1")]
    public void EitherPurchaseOrder_AllowsOnlyFirstUpgradeAcrossVisits(string first, string second)
    {
        var player = new Player();
        var messages = English();
        string output = Visit(player, first + "\n" + second + "\n3\n", messages);
        Assert.AreEqual(5, player.Gold);
        Assert.AreEqual(first == "1", player.HasMagicalArmor);
        Assert.AreEqual(first == "2", player.HasWeaponEnchantment);
        output += Visit(player, second + "\n3\n", messages);
        Assert.AreEqual(5, player.Gold);
        Assert.AreEqual(first == "1", player.HasMagicalArmor);
        Assert.AreEqual(first == "2", player.HasWeaponEnchantment);
        Assert.AreEqual(2, output.Split(messages.GetMessage("merchant_insufficient_gold")).Length - 1);
    }

    [TestMethod]
    public void SouthEnchantRevisitNorth_BonusPersistsForRepeatedHits()
    {
        string output = Journey("s\n2\n2\n3\ns\n2\n3\nn\na\na\nr\n",
            15, 11, 5, 1, 20, 15, 11, 5, 1, 20);
        Assert.AreEqual(1, output.Split("Gold: 20").Length - 1);
        Assert.AreEqual(4, output.Split("Gold: 5").Length - 1);
        Assert.AreEqual(2, output.Split("You already own this weapon enchantment.").Length - 1);
        StringAssert.Contains(output, "Weapon damage bonus: 3");
        Assert.AreEqual(2, output.Split(string.Format(English().GetMessage("player_damage_dealt"), 8)).Length - 1);
        StringAssert.Contains(output, string.Format(English().GetMessage("dragon_health_now"), "Smolderfang", 4));
        StringAssert.Contains(output, English().GetMessage("retreat_combat"));
    }

    [TestMethod]
    public void EnchantEndOfInput_ReturnsToTravel()
    {
        string output = Journey("s\n2\n");
        StringAssert.Contains(output, "Gold: 5");
        StringAssert.Contains(output, "Status: Owned");
        StringAssert.Contains(output, English().GetMessage("adventure_ends"));
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(true, 3)]
    public void Stats_ShowFixedWeaponBonus(bool enchanted, int bonus)
    {
        var player = new Player { HasWeaponEnchantment = enchanted };
        var messages = English();
        var original = Console.Out;
        using var output = new StringWriter();
        try
        {
            Console.SetOut(output);
            player.DisplayStats(messages);
        }
        finally { Console.SetOut(original); }
        StringAssert.Contains(output.ToString(), $"Weapon damage bonus: {bonus}");
        var random = new RepeatableCombatTests.ScriptedRandom(10, 10, 20);
        StringAssert.Contains(new Combat(player, new Dragon(new Die(random)), messages).GetCombatStatsDisplay(), $"Weapon damage bonus: {bonus}");
        random.AssertExhausted();
    }

    private static string Visit(Player player, string script, Messages messages)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        using var input = new BoundedReader(script);
        using var output = new StringWriter();
        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            new Game().VisitMerchant(player, messages);
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
        return output.ToString();
    }
}
