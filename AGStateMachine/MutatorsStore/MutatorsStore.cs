using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AGStateMachine.StateMutation;

namespace AGStateMachine.MutatorsStore
{
    public class MutatorsStore<TInstance, TState> : IMutatorsStore<TInstance, TState>
        where TState : Enum
        where TInstance : IInstance<TState>
    {
        private readonly IDictionary<TInstance, IStateMutator<TInstance, TState>> _mutators;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary">Threadsafe dictionary</param>
        public MutatorsStore(IDictionary<TInstance, IStateMutator<TInstance, TState>> dictionary)
        {
            _mutators = dictionary;
        }


        public bool TryGetMutator(TInstance instance, out IStateMutator<TInstance, TState> mutator)
        {
            mutator = null;
            return _mutators.TryGetValue(instance, out mutator);
        }

        public IStateMutator<TInstance, TState> GetOrCreateMutator(TInstance instance,
            Func<IStateMutator<TInstance, TState>> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (!TryGetMutator(instance, out var mutator))
            {
                mutator = builder();
                _mutators.Add(instance, mutator);
            }

            return mutator;
        }
    }
}