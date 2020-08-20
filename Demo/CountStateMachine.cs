using AGStateMachine;

namespace Demo
{
        public enum States
        {
                A,B,C
        }
        
        public enum Events
        {
                M,N
        }

        public class StateInstance:IInstance<States>
        {
                public int Counter { get; set; }
                public States CurrentState { get; set; }
        }

        public class TypedEvent
        {
                public int Count { get; }

                public TypedEvent(int count)
                {
                        Count = count;
                }
        }
        public class CountStateMachine:AGStateMachine<States,Events,StateInstance>{}
}