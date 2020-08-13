using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;
using DefaultNamespace;

namespace AGStateMachine
{
    public abstract class AGStateMachine<TState, TEvent, TInstance>
        where TState : Enum
        where TEvent : Enum
        where TInstance : class, IInstance<TState>

    {
        private readonly ConcurrentDictionary<(TState, TEvent), Func<TInstance, Task>> _transtions =
            new ConcurrentDictionary<(TState, TEvent), Func<TInstance, Task>>();

        private readonly ConditionalWeakTable<TInstance, StateMutator<TInstance, TState>> _table =
            new ConditionalWeakTable<TInstance, StateMutator<TInstance, TState>>();

        public void AddTransition(TState state, TEvent @event, Func<TInstance, Task> function)
        {
            if (function == null)
                return;
            _transtions[(state, @event)] = function;
        }

        public Task RaiseEvent(TEvent @event, TInstance instance)
        {
            if (!_transtions.ContainsKey((instance.CurrentState, @event)))
                return Task.CompletedTask;
            
            var mutator = _table.GetOrCreateValue(instance);
            return mutator.SendAsync(
                instance,
                instance.CurrentState,
                _transtions[(instance.CurrentState, @event)]
            );
        }
    }
}