public class Wolf
{
    private readonly Die die;
    public int Strength { get; }
    public int Agility { get; }
    public int HealthPoints { get; private set; }

    public Wolf(Die die)
    {
        ArgumentNullException.ThrowIfNull(die);
        this.die = die;
        Strength = die.Roll(15);
        Agility = die.Roll(15);
        HealthPoints = die.Roll(15);
    }

    // True returns the surviving player to travel; false ends the adventure.
    public bool Encounter(Player player, Messages messages)
    {
        if (HealthPoints <= 0)
        {
            Console.WriteLine(messages.GetMessage("wolf_route_clear"));
            return true;
        }
        Console.WriteLine(messages.GetMessage("wolf_intro"));
        Console.WriteLine(messages.GetMessage("stats_strength") + Strength);
        Console.WriteLine(messages.GetMessage("stats_agility") + Agility);
        Console.WriteLine(messages.GetMessage("stats_health") + HealthPoints);
        Console.WriteLine(messages.GetMessage("wolf_bite_range"));
        while (true)
        {
            Console.WriteLine(messages.GetMessage("attack_prompt"));
            string? choice = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (choice == null || choice == "r" || choice == "retreat")
                return true;
            if (choice != "a" && choice != "attack")
            {
                Console.WriteLine(messages.GetMessage("invalid"));
                continue;
            }

            if (player.AttackHits(player.RollAttack()))
            {
                if (die.Roll(20) > Agility)
                {
                    int damage = player.RollDamage();
                    HealthPoints = Math.Max(0, HealthPoints - damage);
                    Console.WriteLine(string.Format(messages.GetMessage("wolf_hurt"), damage));
                    Console.WriteLine(string.Format(messages.GetMessage("wolf_health_now"), HealthPoints));
                }
                else
                    Console.WriteLine(messages.GetMessage("wolf_dodged"));
            }
            else
                Console.WriteLine(messages.GetMessage("wolf_player_missed"));
            if (HealthPoints <= 0)
            {
                Console.WriteLine(messages.GetMessage("wolf_defeated"));
                return true;
            }
            Console.WriteLine(string.Format(messages.GetMessage("wolf_attack"), player.Name));
            if (die.Roll(20) > Strength)
                Console.WriteLine(string.Format(messages.GetMessage("wolf_missed"), player.Name));
            else if (die.Roll(20) <= player.Agility)
                Console.WriteLine(string.Format(messages.GetMessage("wolf_player_defended"), player.Name));
            else
            {
                int damage = Math.Max(0, die.Roll(6) - player.ArmorProtection);
                player.HealthPoints = Math.Max(0, player.HealthPoints - damage);
                Console.WriteLine(string.Format(messages.GetMessage("player_health_now"), player.Name, player.HealthPoints));
            }
            if (player.HealthPoints <= 0)
            {
                Console.WriteLine(messages.GetMessage("wolf_player_defeated"));
                return false;
            }
        }
    }
}
