using Xunit;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests {
    public class DynaTests {
        [Fact]
        public void EnumerateReturnsAllCells() {
            var dyn = new Dyna<int,string>(new int3(2,2,2), 1, "meta");
            int count = 0;
            foreach (var (p,v) in dyn.Cells())
                count += v;
            Assert.Equal(8, count);
            Assert.Equal("meta", dyn.Metadata);
        }

        [Fact]
        public void MapModifiesVoxel() {
            var dyn = new Dyna<int,string>(new int3(1,1,1), 2, "m");
            dyn.Map(new int3(0,0,0), v => v * 3);
            Assert.Equal(6, dyn[new int3(0,0,0)]);
        }

        [Fact]
        public void FromFuncCreatesExpectedValues() {
            var d = Dyna<int,string>.FromFunc(new int3(2,1,1), "f", p => p.x);
            Assert.Equal(0, d[new int3(0,0,0)]);
            Assert.Equal(1, d[new int3(1,0,0)]);
            Assert.Equal("f", d.Metadata);
        }

        [Fact]
        public void PositionsEnumeratesAllCoords() {
            var d = new Dyna<int,string>(new int3(2,2,1), 0, "p");
            int seen = 0;
            foreach (var p in d.Positions())
                seen++;
            Assert.Equal(4, seen);
        }
    }
}
