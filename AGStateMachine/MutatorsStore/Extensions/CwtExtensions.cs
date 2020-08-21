using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AGStateMachine.MutatorsStore.Wrappers;

namespace AGStateMachine.MutatorsStore.Extensions
{
    public static class CwtExtensions
    {
        public static IDictionary<TKey,TValue> ToDictionaryWrapped<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> cwt)
        where TKey:class
        where TValue:class
        {
            return new CwtDictionaryWrapper<TKey,TValue>(cwt);
        }
    }
}