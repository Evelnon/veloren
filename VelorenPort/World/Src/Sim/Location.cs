using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.World.Sim
{
    /// <summary>
    /// Representation of a world location with a name and neighbouring links.
    /// Ported from <c>sim/location.rs</c>.
    /// </summary>
    [Serializable]
    public class Location
    {
        public string Name { get; private set; } = string.Empty;
        public int2 Center { get; private set; }
        public Kingdom? Kingdom { get; set; }
        private readonly HashSet<ulong> _neighbours = new();

        public IEnumerable<ulong> Neighbours => _neighbours;

        private Location(string name, int2 center)
        {
            Name = name;
            Center = center;
        }

        /// <summary>Create a new random location.</summary>
        public static Location Generate(int2 center, System.Random rng)
        {
            string name = GenerateName(rng);
            return new Location(name, center);
        }

        public void AddNeighbour(ulong id) => _neighbours.Add(id);

        private static readonly string[] FirstSyl = new[] {
            "Eri","Val","Gla","Wilde","Cold","Deep","Dura","Ester","Fay","Dark","West",
            "East","North","South","Ray","Eri","Dal","Som","Sommer","Black","Iron","Grey",
            "Hel","Gal","Mor","Lo","Nil","Bel","Lor","Gold","Red","Marble","Mana","Gar",
            "Mountain","Red","Cheo","Far","High"
        };

        private static readonly string[] Mid = new[] { "ka", "se", "au", "da", "di" };

        private static readonly string[] Tails = new[] {
            "ben","sel","dori","theas","dar","bur","to","vis","ten",
            "stone","tiva","id","and","or","el","ond","ia","eld","ald","aft","ift","ity",
            "well","oll","ill","all","wyn","light"," Hill","lin","mont","mor","cliff","rok",
            "den","mi","rock","glenn","rovi","lea","gate","view","ley","wood","ovia",
            "cliff","marsh","kor","ice","acre","venn","crest","field",
            "vale","spring"," Vale","grasp","fel","fall","grove","wyn","edge"
        };

        private static string GenerateName(System.Random rng)
        {
            string name = string.Empty;
            if (rng.NextDouble() < 0.5)
            {
                name += FirstSyl[rng.Next(FirstSyl.Length)];
                name += Mid[rng.Next(Mid.Length)];
                name += Tails[rng.Next(Tails.Length)];
            }
            else
            {
                name += FirstSyl[rng.Next(FirstSyl.Length)];
                name += Tails[rng.Next(Tails.Length)];
            }
            return name;
        }
    }

    /// <summary>Placeholder kingdom information.</summary>
    [Serializable]
    public class Kingdom
    {
        public string RegionName { get; set; } = string.Empty;
    }
}
