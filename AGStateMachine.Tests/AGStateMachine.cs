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
            var sm = new TestStateMachine();
            var stateInstance = new TestStateInstance();
            sm.AddTransition(
                State.First, 
                Event.FirstEvent,
                 (inst) =>
                {
                    inst.CurrentState = State.Second;
                    return Task.CompletedTask;
                }
            );

            await sm.RaiseEvent(Event.FirstEvent, stateInstance);

            Assert.Equal(State.Second, stateInstance.CurrentState);
        }

        private enum State
        {
            First,
            Second
        }

        private enum Event
        {
            FirstEvent
        }

        private class TestStateInstance : IInstance<State>
        {
            public State CurrentState { get; set; }
        }

        private class TestStateMachine : AGStateMachine<State, Event, TestStateInstance>
        {
        }
    }
}