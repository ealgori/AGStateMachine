using System;
using System.Threading.Tasks;
using AGStateMachine;

namespace AGStateMachine.StateMutation
{
    public interface IStateMutator<TInstance, TState> where TInstance : IInstance<TState> where TState : Enum
    {
        Task SendAsync(TInstance instance, TState currentState, Func<TInstance, Task> func);
        Task SendAsync<TCEvent>(TInstance instance, TCEvent @event, TState currentState, Func<TInstance,TCEvent, Task> func);
    }
}