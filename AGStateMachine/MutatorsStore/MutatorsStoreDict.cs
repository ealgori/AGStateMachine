using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AGStateMachine.StateMutation;

namespace AGStateMachine.MutatorsStore
{
    public class MutatorsStoreDict<TInstance, TState> : IMutatorsStore<TInstance, TState>
        where TState : Enum
        where TInstance : class, IInstance<TState>
    {
        private readonly ConcurrentDictionary<TInstance, IStateMutator<TInstance, TState>> _mutators;
        private readonly object _lock = new object();

        public MutatorsStoreDict()
        {
            _mutators = new ConcurrentDictionary<TInstance, IStateMutator<TInstance, TState>>();
        }

        public bool TryGetMutator(TInstance instance, out IStateMutator<TInstance, TState> mutator)
        {
            return _mutators.TryGetValue(instance, out mutator);
        }

        public IStateMutator<TInstance, TState> GetOrCreateMutator(TInstance instance,
            Func<IStateMutator<TInstance, TState>> builder)
        {
            if (_mutators.TryGetValue(instance, out var mutator))
                return mutator;
            lock (_lock)
            {
                if (_mutators.TryGetValue(instance, out mutator))
                    return mutator;
                return _mutators.GetOrAdd(instance, (_) => builder());
            }
        }
    }
}