using System.Collections.Generic;
using VelorenPort.Server;

namespace Server.Tests;

public class WiringTests {
    [Fact]
    public void OutputFormula_Computation_Works() {
        var inputs = new Dictionary<string,float> { {"a", 2f}, {"b", 3f} };
        var f = OutputFormula.Logic(new Logic(LogicKind.Sum,
            OutputFormula.Input("a"), OutputFormula.Input("b")));
        var val = f.Compute(inputs);
        Assert.Equal(5f, val);
    }
}
