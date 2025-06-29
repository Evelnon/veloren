using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.Server;
using VelorenPort.Server.Sys;

namespace Server.Tests;

public class WiringSystemTests
{
    [Fact]
    public void Update_PropagatesWireSignals()
    {
        var em = new EntityManager();
        var e1 = em.CreateEntity();
        var elem1 = new WiringElement();
        elem1.Outputs["sig"] = OutputFormula.Constant(5f);
        em.AddComponentData(e1, elem1);

        var e2 = em.CreateEntity();
        var elem2 = new WiringElement();
        em.AddComponentData(e2, elem2);

        var circuitEntity = em.CreateEntity();
        var circuit = new Circuit();
        circuit.Wires.Add(new Wire(new WireNode(e1, "sig"), new WireNode(e2, "inp")));
        em.AddComponentData(circuitEntity, circuit);

        WiringSystem.Update(em);

        var updated = em.GetComponentData<WiringElement>(e2);
        Assert.True(updated.Inputs.TryGetValue("inp", out var val));
        Assert.Equal(5f, val);
    }
}
