using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;
using AGStateMachine.StateMutation;
using AGStateMachine.MutatorsStore;

namespace AGStateMachine
{
    public abstract class AGStateMachine<TState, TEvent, TInstance>
        where TState : Enum
        where TEvent : Enum
        where TInstance : class, IInstance<TState>

    {
        private readonly ConcurrentDictionary<(TState, TEvent), Func<TInstance, Task>> _transitions =
            new ConcurrentDictionary<(TState, TEvent), Func<TInstance, Task>>();

        private readonly ConcurrentDictionary<(TState, Type ), Delegate> _typedTransitions =
            new ConcurrentDictionary<(TState, Type), Delegate>();


        private IMutatorsStore<TInstance, TState> _mutatorsStore;

      
        protected AGStateMachine(IMutatorsStore<TInstance, TState> mutatorsStore = null)
        {
            _mutatorsStore = mutatorsStore ?? new MutatorsStoreWeak<TInstance, TState>();
        }

        public void AddTransition(TState state, TEvent @event, Func<TInstance, Task> function)
        {
            _transitions[(state, @event)] = function ?? throw new ArgumentNullException(nameof(function));
        }

        public void AddTransition<TCEvent>(TState state, Func<TInstance, TCEvent, Task> function)
        {
            _typedTransitions[(state, typeof(TCEvent))] = function ?? throw new ArgumentNullException(nameof(function));
        }

        public Task RaiseEvent(TEvent @event, TInstance instance)
        {
            if (!_transitions.TryGetValue((instance.CurrentState, @event), out var func))
                return Task.CompletedTask;
            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            return mutator.SendAsync(
                instance,
                instance.CurrentState,
                func
            );
        }

        public Task RaiseEvent<TCEvent>(TCEvent @event, TInstance instance)
        {
            if (!_typedTransitions.TryGetValue((instance.CurrentState, typeof(TCEvent)), out var func))
            {
                return Task.CompletedTask;
            }

            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            return mutator.SendAsync(
                instance,
                @event,
                instance.CurrentState,
                func as Func<TInstance, TCEvent, Task>
            );
        }
    }
}