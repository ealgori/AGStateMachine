using System;
using System.Threading.Tasks;
using AGStateMachine.StateMutation;

namespace AGStateMachine.MutatorsStore
{
    public interface IMutatorsStore<TInstance, TState> 
        where TState:Enum
        where TInstance: IInstance<TState>
    {
        bool TryGetMutator(TInstance instance, out IStateMutator<TInstance,TState> mutator);
        IStateMutator<TInstance,TState> GetOrCreateMutator(TInstance instance, Func<IStateMutator<TInstance,TState>> builder);
    }
}