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

        protected static readonly Task CompletedTask = Task.CompletedTask; 


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

        /// <summary>
        /// Waits till event will be processed by event processing queue
        /// </summary>
        /// <param name="event"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Task RaiseEvent(TEvent @event, TInstance instance) 
        {
            if(!_transitions.TryGetValue((instance.CurrentState,@event), out var del))
                return CompletedTask;
            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            return mutator.ScheduleAsync(instance, instance.CurrentState, del);

        }


        /// <summary>
        /// Wait till event processing queue accept event
        /// </summary>
        /// <param name="event"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Task ScheduleEvent(TEvent @event, TInstance instance)
        {
            if(!_transitions.TryGetValue((instance.CurrentState,@event), out var del))
                return CompletedTask;
            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            return mutator.ScheduleAsync(instance, instance.CurrentState, del);
        }

      
        /// <summary>
        /// Waits till event will be processed by event processing queue
        /// </summary>
        /// <param name="event"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Task RaiseEvent<TCEvent>(TCEvent @event, TInstance instance)
        {
            if (!_typedTransitions.TryGetValue((instance.CurrentState, typeof(TCEvent)), out var del))
                return CompletedTask;
            ;
            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            return mutator.ProcessAsync(instance, @event, instance.CurrentState,
                del as Func<TInstance, TCEvent, Task>);
        }

        /// <summary>
        /// Wait till event processing queue accept event
        /// </summary>
        /// <param name="event"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Task ScheduleEvent<TCEvent>(TCEvent @event, TInstance instance)
        {
            if (!_typedTransitions.TryGetValue((instance.CurrentState, typeof(TCEvent)), out var del))
                return CompletedTask;

            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            return mutator.ScheduleAsync(instance, @event, instance.CurrentState,
                del as Func<TInstance, TCEvent, Task>);
        }

       
    }
}