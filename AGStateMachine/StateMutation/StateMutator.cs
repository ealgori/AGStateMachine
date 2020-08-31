using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using AGStateMachine;

namespace AGStateMachine.StateMutation
{
   
    public class StateMutator<TInstance, TState> : IStateMutator<TInstance, TState> where TInstance : IInstance<TState>
        where TState : Enum
    {
        private readonly ActionBlock<Func<Task>> _block;
        private readonly CancellationTokenSource _cts;
        protected readonly Task CompletedTask = Task.CompletedTask;
        public StateMutator()
        {
            _cts = new CancellationTokenSource();
            var option = new ExecutionDataflowBlockOptions {CancellationToken = _cts.Token};
            _block = new ActionBlock<Func<Task>>(f => f(), option);
            _block.Completion.ContinueWith(t => Console.WriteLine("Block was stopped"));
        }


        public Task ScheduleAsync(TInstance instance, TState currentState, Func<TInstance, Task> func)
        {
            return _block.SendAsync(() =>
            {
                if (!Equals(currentState, instance.CurrentState))
                    return CompletedTask;
                return func(instance);
            });
        }

        public Task ProcessAsync(TInstance instance, TState currentState, Func<TInstance, Task> func)
        {
            var tcs = new TaskCompletionSource<bool>();
            _block.SendAsync(async () =>
            {
                try
                {
                    if (!Equals(instance.CurrentState, currentState))
                        return;
                    await func(instance);
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }

        public Task ScheduleAsync<TCEvent>(TInstance instance, TCEvent @event, TState currentState,
            Func<TInstance, TCEvent, Task> func)
        {
            if (!Equals(currentState, instance.CurrentState))
            {
                return CompletedTask;
            }

            return _block.SendAsync(() => func(instance, @event));
        }

        public Task ProcessAsync<TCEvent>(TInstance instance, TCEvent @event, TState currentState,
            Func<TInstance, TCEvent, Task> func)
        {
            var tcs = new TaskCompletionSource<bool>();
            _block.SendAsync(async () =>
            {
                try
                {
                    if (!Equals(instance.CurrentState, currentState))
                        return;
                    await func(instance, @event);
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }


        ~StateMutator()
        {
            _cts.Cancel();
        }
    }
}