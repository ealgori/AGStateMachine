using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace AGStateMachine.Tests
{
    public class BehaviouralTests
    {
        [Fact]
        public async Task ShouldInvokeFuncOneTime()
        {
            var sm = TestSM1.Create();
            var stateInstance = new TestSM1.TestStateInstance();
            var mockFunc = new Mock<Func<TestSM1.TestStateInstance, Task>>();

            sm.AddTransition(
                TestSM1.StateSM1.First,
                TestSM1.EventSM1.FirstEvent,
                mockFunc.Object
            );

            await sm.RaiseEvent(TestSM1.EventSM1.FirstEvent, stateInstance);

            mockFunc.Verify(x => x(stateInstance), Times.Once);
        }

        [Fact]
        public async Task ShouldNotInvokeFunc()
        {
            var sm = TestSM1.Create();
            var stateInstance = new TestSM1.TestStateInstance();
            var mockFunc = new Mock<Func<TestSM1.TestStateInstance, Task>>();

            sm.AddTransition(
                TestSM1.StateSM1.Second,
                TestSM1.EventSM1.FirstEvent,
                mockFunc.Object
            );

            await sm.RaiseEvent(TestSM1.EventSM1.FirstEvent, stateInstance);

            mockFunc.Verify(x => x(stateInstance), Times.Never);
        }


        [Fact]
        public async Task ShouldSaveInvokeSequence()
        {
            var sm = TestSM1.Create();
            var stateInstance = new Mock<TestSM1.TestStateInstance>(MockBehavior.Strict);
            var sequence = new MockSequence();
            stateInstance.InSequence(sequence).Setup(i => i.Invoke(1));
            stateInstance.InSequence(sequence).Setup(i => i.Invoke(2));
            var typedEvent1 = new TestSM1.TypedEvent(){Counter = 1};
            var typedEvent2 = new TestSM1.TypedEvent(){Counter = 2};
            sm.AddTransition<TestSM1.TypedEvent>(
                TestSM1.StateSM1.First,
                (inst, ev) =>
                {
                    inst.Invoke(ev.Counter);
                    return Task.CompletedTask;
                }
            );

            await sm.RaiseEvent(typedEvent1, stateInstance.Object);
            await sm.RaiseEvent(typedEvent2, stateInstance.Object);
            
            stateInstance.Verify(i=>i.Invoke(1));
            stateInstance.Verify(i=>i.Invoke(2));
        }
    }
}