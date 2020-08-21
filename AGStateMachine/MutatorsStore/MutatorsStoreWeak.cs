using System;
using System.Runtime.CompilerServices;
using AGStateMachine.StateMutation;

namespace AGStateMachine.MutatorsStore
{
    public class MutatorsStoreWeak<TInstance, TState> : IMutatorsStore<TInstance, TState>
        where TState : Enum
        where TInstance : class, IInstance<TState>
    {
        private readonly ConditionalWeakTable<TInstance, IStateMutator<TInstance, TState>> _mutators;
        private readonly object _lock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary">Threadsafe dictionary</param>
        public MutatorsStoreWeak()
        {
            _mutators = new ConditionalWeakTable<TInstance, IStateMutator<TInstance, TState>>();
        }


        public bool TryGetMutator(TInstance instance, out IStateMutator<TInstance, TState> mutator)
        {
            mutator = null;
            return _mutators.TryGetValue(instance, out mutator);
        }

        public IStateMutator<TInstance, TState> GetOrCreateMutator(TInstance instance,
            Func<IStateMutator<TInstance, TState>> builder)
        {
            if (TryGetMutator(instance, out var mutator))
                return mutator;
            lock (_lock)
            {
                if (TryGetMutator(instance, out mutator))
                    return mutator;

                return _mutators.GetValue(instance, (_) => builder());
            }
        }
    }
}