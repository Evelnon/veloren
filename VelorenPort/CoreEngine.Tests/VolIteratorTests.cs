using Xunit;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests {
    public class VolIteratorTests {
        [Fact]
        public void EnumeratorsCoverVolume() {
            var vol = new Vol<int>(new int3(2,2,2));
            int count = 0;
            foreach (var pos in new DefaultPosEnumerator(int3.zero, vol.Size)) {
                vol.Set(pos, count++);
            }
            int sum = 0;
            foreach (var (p,v) in new DefaultVolEnumerator<int>(vol, int3.zero, vol.Size))
                sum += v;
            Assert.Equal(0+1+2+3+4+5+6+7, sum);
        }

        [Fact]
        public void MapUpdatesVoxel() {
            var vol = new Vol<int>(new int3(1,1,1));
            vol.Set(new int3(0,0,0), 2);
            vol.Map(new int3(0,0,0), v => v*3);
            Assert.Equal(6, vol.Get(new int3(0,0,0)));
        }
    }
}
