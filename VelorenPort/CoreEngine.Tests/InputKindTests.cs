using VelorenPort.CoreEngine;
using Xunit;

namespace CoreEngine.Tests
{
    public class InputKindTests
    {
        [Fact]
        public void AbilityRecognition()
        {
            var a = InputKind.AbilitySlot(1);
            Assert.True(a.IsAbility);
            Assert.Equal(InputKind.Kind.Ability, a.Type);
            Assert.Equal(1, a.Index);
            Assert.Equal("Ability(1)", a.ToString());
        }

        [Fact]
        public void NonAbilityRecognition()
        {
            Assert.False(InputKind.Roll.IsAbility);
            Assert.False(InputKind.Jump.IsAbility);
        }
    }
}
