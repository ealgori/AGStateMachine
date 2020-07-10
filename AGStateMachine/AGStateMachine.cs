using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Transactions;

namespace AGStateMachine
{
    public abstract class AGStateMachine<TState, TEvent, TInstance>
        where TState : Enum
        where TEvent : Enum
        where TInstance : class, IInstance<TState>

    {
        private ConcurrentDictionary<(TState,TEvent),Func<TInstance,Task>> _transtions = new ConcurrentDictionary<(TState, TEvent), Func<TInstance, Task>>();

        public void  AddTransition(TState state, TEvent @event, Func<TInstance, Task> function)
        {
            if(function==null)
                return;
            _transtions[(state, @event)] = function;
        }

        public Task RaiseEvent(TEvent @event, TInstance instance)
        {
            if (!_transtions.ContainsKey((instance.CurrentState, @event)))
                return Task.CompletedTask;

            return _transtions[(instance.CurrentState, @event)].Invoke(instance);
        }
    }
}