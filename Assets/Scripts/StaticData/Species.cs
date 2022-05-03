using System.Collections.Generic;
public enum SpeciesNames
{
    // Player skins
    FallenAngel1,
    Golem1,
    Orc,

    // AI
    FallenAngel2,
    FallenAngel3,
    Golem2,
    Golem3,
    Goblin,
    Ogre
}

public class Species
{
    public static readonly Dictionary<SpeciesNames, Dictionary<string, float>> defaultStats =
    new Dictionary<SpeciesNames, Dictionary<string, float>>
    {
        // Orcs
        {SpeciesNames.Orc, new Dictionary<string, float>{{"hp", 12},{"damage", 2.5f},{"speed", 1}}},
        {SpeciesNames.Goblin, new Dictionary<string, float>{{"hp", 12},{"damage", 2.5f},{"speed", 1}}},
        {SpeciesNames.Ogre, new Dictionary<string, float>{{"hp", 12},{"damage", 2.5f},{"speed", 1}}},

        // Golems
        {SpeciesNames.Golem1, new Dictionary<string, float>{{"hp", 48},{"damage", 1.5f},{"speed", 2}}},
        {SpeciesNames.Golem2, new Dictionary<string, float>{{"hp", 48},{"damage", 1.5f},{"speed", 2}}},
        {SpeciesNames.Golem3, new Dictionary<string, float>{{"hp", 48},{"damage", 1.5f},{"speed", 2}}},

        // Angels
        {SpeciesNames.FallenAngel1, new Dictionary<string, float>{{"hp", 24},{"damage", 2f},{"speed", 3}}},
        {SpeciesNames.FallenAngel2, new Dictionary<string, float>{{"hp", 24},{"damage", 2f},{"speed", 3}}},
        {SpeciesNames.FallenAngel2, new Dictionary<string, float>{{"hp", 24},{"damage", 2f},{"speed", 3}}},
    };

    public static readonly Dictionary<SpeciesNames, Dictionary<string, float>> statsPerLevel =
    new Dictionary<SpeciesNames, Dictionary<string, float>>
    {
        // Orcs
        {SpeciesNames.Orc, new Dictionary<string, float>{{"hp", 6},{"damage", 2f},{"speed", 0.5f}}},
        {SpeciesNames.Goblin, new Dictionary<string, float>{{"hp", 6},{"damage", 2f},{"speed", 0.5f}}},
        {SpeciesNames.Ogre, new Dictionary<string, float>{{"hp", 6},{"damage", 2f},{"speed", 0.5f}}},

        // Golems
        {SpeciesNames.Golem1, new Dictionary<string, float>{{"hp", 24},{"damage", 1f},{"speed", 1f}}},
        {SpeciesNames.Golem2, new Dictionary<string, float>{{"hp", 24},{"damage", 1f},{"speed", 1f}}},
        {SpeciesNames.Golem3, new Dictionary<string, float>{{"hp", 24},{"damage", 1f},{"speed", 1f}}},

        // Angels
        {SpeciesNames.FallenAngel1, new Dictionary<string, float>{{"hp", 18},{"damage", 1.4f},{"speed", 1.5f}}},
        {SpeciesNames.FallenAngel2, new Dictionary<string, float>{{"hp", 18},{"damage", 1.4f},{"speed", 1.5f}}},
        {SpeciesNames.FallenAngel2, new Dictionary<string, float>{{"hp", 18},{"damage", 1.4f},{"speed", 1.5f}}},
    };
}