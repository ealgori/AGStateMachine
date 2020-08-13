using System;
using System.Threading.Tasks;
using Xunit;

namespace AGStateMachine.Tests
{
    public class AGStateMachineTests
    {
        [Fact]
        public async Task ShouldChangeStateOnEvent()
        {
            var sm = TestSM1.Create();
            var stateInstance = new TestSM1.TestStateInstance();
            sm.AddTransition(
                TestSM1.StateSM1.First, 
                TestSM1.EventSM1.FirstEvent,
                 (inst) =>
                {
                    inst.CurrentState = TestSM1.StateSM1.Second;
                    return Task.CompletedTask;
                }
            );

            await sm.RaiseEvent(TestSM1.EventSM1.FirstEvent, stateInstance);

            Assert.Equal(TestSM1.StateSM1.Second, stateInstance.CurrentState);
        }
        [Fact]
        public async Task ShouldNotChangeCounterSecondTimeOnEvent()
        {
            var sm = TestSM1.Create();
            var stateInstance = new TestSM1.TestStateInstance();
            sm.AddTransition(
                TestSM1.StateSM1.First, 
                TestSM1.EventSM1.FirstEvent,
                (inst) =>
                {
                    inst.CurrentState = TestSM1.StateSM1.Second;
                    inst.Counter++;
                    return Task.CompletedTask;
                }
            );

            await sm.RaiseEvent(TestSM1.EventSM1.FirstEvent, stateInstance);
            await sm.RaiseEvent(TestSM1.EventSM1.FirstEvent, stateInstance);

            Assert.Equal(TestSM1.StateSM1.Second, stateInstance.CurrentState);
            Assert.Equal(1, stateInstance.Counter);
        }


    }
}