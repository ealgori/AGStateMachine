using System;
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
            var mockFunc = new Mock<Func<TestSM1.TestStateInstance,Task>>();
            
            sm.AddTransition(
                TestSM1.StateSM1.First, 
                TestSM1.EventSM1.FirstEvent,
                mockFunc.Object
            );

            await sm.RaiseEvent(TestSM1.EventSM1.FirstEvent, stateInstance);

            mockFunc.Verify(x=>x(stateInstance), Times.Once);
            
        } 
        [Fact]
        public async Task ShouldNotInvokeFunc()
        {
            var sm = TestSM1.Create();
            var stateInstance = new TestSM1.TestStateInstance();
            var mockFunc = new Mock<Func<TestSM1.TestStateInstance,Task>>();
            
            sm.AddTransition(
                TestSM1.StateSM1.Second, 
                TestSM1.EventSM1.FirstEvent,
                mockFunc.Object
            );

            await sm.RaiseEvent(TestSM1.EventSM1.FirstEvent, stateInstance);

            mockFunc.Verify(x=>x(stateInstance), Times.Never);
        }
       
    }
}