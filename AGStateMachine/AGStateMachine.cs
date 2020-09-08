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
        
        private readonly ConcurrentDictionary<(TState, TEvent), Delegate> _transitions =
            new ConcurrentDictionary<(TState, TEvent), Delegate>();


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
        
        public void AddTransition(TState state, TEvent @event, Action<TInstance> action)
        {
            _transitions[(state, @event)] = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void AddTransition<TCEvent>(TState state, Func<TInstance, TCEvent, Task> function)
        {
            _typedTransitions[(state, typeof(TCEvent))] = function ?? throw new ArgumentNullException(nameof(function));
        }
        public void AddTransition<TCEvent>(TState state, Action<TInstance, TCEvent> action)
        {
            _typedTransitions[(state, typeof(TCEvent))] = action ?? throw new ArgumentNullException(nameof(action));
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
                return Task.CompletedTask;
            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            switch (del)
            {
                case Func<TInstance,Task> aFunc: return mutator.ProcessAsync(instance, instance.CurrentState, aFunc); 
                //TODO: Ambiguity. Unclear for user whats happened when task completed. Is this sync delegate shceduled or already completed?
                // in this context its scheduled, but upper function mean wait untill raising
                case Action<TInstance> sAction:return mutator.ScheduleAsync(instance, instance.CurrentState, sAction);
                default:
                    throw new InvalidOperationException("Not supported transitions delegate");
            }
            
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
                return Task.CompletedTask;
            
            
            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            switch (del)
            {
                case Func<TInstance,Task> aFunc: return mutator.ScheduleAsync(instance, instance.CurrentState, aFunc); 
                case Action<TInstance> sAction:return mutator.ScheduleAsync(instance, instance.CurrentState, sAction);
                default:
                    throw new InvalidOperationException("Not supported transitions delegate");
            }
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
                return Task.CompletedTask;
            ;
            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());

            switch (del)
            {
                case Func<TInstance,TCEvent,Task> aFunc:return mutator.ProcessAsync(instance, @event, instance.CurrentState, aFunc);
                case Action<TInstance,TCEvent> sAction:return mutator.ScheduleAsync(instance, @event, instance.CurrentState, sAction);
                default:
                    throw new InvalidOperationException("Non suported transition delegate");
            }
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
                return Task.CompletedTask;

            var mutator = _mutatorsStore.GetOrCreateMutator(instance, () => new StateMutator<TInstance, TState>());
            
            switch (del)
            {
                case Func<TInstance,TCEvent,Task> aFunc:return mutator.ScheduleAsync(instance, @event, instance.CurrentState, aFunc);
                case Action<TInstance,TCEvent> sAction:return mutator.ScheduleAsync(instance, @event, instance.CurrentState, sAction);
                default:
                    throw new InvalidOperationException("Non suported transition delegate");
            }
        }

       
    }
}