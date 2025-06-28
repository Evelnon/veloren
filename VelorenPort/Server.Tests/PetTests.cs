using System;
using Unity.Entities;
using VelorenPort.Server;

namespace Server.Tests;

public class PetTests {
    [Fact]
    public void TamePet_DoesNotThrow() {
        var pet = Entity.Null;
        var owner = Entity.Null;
        Pet.TamePet(pet, owner);
    }
}
