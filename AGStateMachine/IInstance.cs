using System;

namespace AGStateMachine
{
    public interface IInstance<TState> where TState:Enum
    {
        TState CurrentState { get; set; }
    }
}