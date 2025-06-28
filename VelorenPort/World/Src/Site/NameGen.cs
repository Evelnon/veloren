using System;
using System.Linq;

namespace VelorenPort.World.Site {
    /// <summary>
    /// Procedural name generator roughly based on the Rust implementation.
    /// Produces simple pseudo-English location names.
    /// </summary>
    public static class NameGen {
        private static readonly string[] Cons = new[] {
            "d","f","ph","r","st","t","s","p","sh","th","br","tr","m","k","w","y","cr","fr","dr","pl","wr","sn","g","qu","l"
        };

        private static readonly string[] Start = Cons.Concat(new[] {
            "cr","thr","str","br","iv","est","ost","ing","kr","in","on","tr","tw","wh","eld","ar","or","ear","irr","mi","en","ed","et","ow","fr","shr","wr","gr","pr"
        }).ToArray();

        private static readonly string[] Middle = Cons.Concat(new[]{"tt"}).ToArray();
        private static readonly string[] Vowel = new[]{"o","e","a","i","u","au","ee","ow","ay","ey","oe"};
        private static readonly string[] End = new[] {
            "et","ige","age","ist","en","on","og","end","ind","ock","een","edge","ist","ed","est","eed","ast","olt","ey","ean","ead","onk","ink","eon","er","ow","ot","in","on","id","ir","or","ig","en"
        };

        /// <summary>
        /// Generate a new random location name.
        /// </summary>
        public static string Generate(Random rng) {
            int syllables = rng.Next(1,4);
            string name = Start[rng.Next(Start.Length)];
            for (int i = 0; i < Math.Max(0, syllables - 2); i++) {
                name += Vowel[rng.Next(Vowel.Length)];
                name += Middle[rng.Next(Middle.Length)];
            }
            name += End[rng.Next(End.Length)];
            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }
    }
}
