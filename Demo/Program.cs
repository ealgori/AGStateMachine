using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AGStateMachine;
using Demo.Infrustructure;

namespace Demo
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Restart();
            int instanceCount = 1000;
            int messagesPerThread = 300;
            var countSM = new CountStateMachine();
            countSM.AddTransition(States.A, Events.M, OnM);
            countSM.AddTransition<TypedEvent>(States.A, OnTE);


            var instances = new List<StateInstance>(instanceCount);
            for (int i = 0; i < instanceCount; i++) instances.Add(new StateInstance());

            int counter = 0;
            ConsoleUtility.WriteProgressBar(counter);
            var producersTasks = Enumerable.Range(0, Environment.ProcessorCount).Select(_ => Task.Run(async () =>
            {
                for (int i = 0; i < messagesPerThread / 2; i++)
                {
                    var eventTasks = instances.Select((inst) => countSM.RaiseEvent(Events.M, inst));
                    var typedEventsTask = instances.Select((inst) => countSM.RaiseEvent(new TypedEvent(1), inst))
                        .ToArray();
                    await Task.WhenAll(eventTasks)
                            .ConfigureAwait(false)
                        ;
                    await Task.WhenAll(typedEventsTask)
                            .ConfigureAwait(false)
                        ;
                    Interlocked.Increment(ref counter);

                    ConsoleUtility.WriteProgressBar((int)
                                                    Volatile.Read(ref counter)*100/  
                                                    (messagesPerThread / 2 * Environment.ProcessorCount)
                        , true);
                }
            }));
            await Task.WhenAll(producersTasks)
                    .ConfigureAwait(false)
                ;

            var bugs = instances.Where(i => i.Counter != Environment.ProcessorCount * messagesPerThread).ToArray();
            if (bugs.Any())
            {
                foreach (var t in bugs)
                {
                    Console.WriteLine(t.Counter);
                }

                throw new InvalidOperationException("We lost some events!");
            }

            sw.Stop();
            Console.WriteLine($@"Complete successful : 
{instanceCount} instances 
{messagesPerThread} messages per instance
{sw.Elapsed.TotalSeconds} seconds");
        }

        public static Task OnM(StateInstance instance)
        {
            instance.Counter++;
            return Task.CompletedTask;
        }

        public static async Task OnTE(StateInstance instance, TypedEvent @event)
        {
            await Task.Delay(2);
            instance.Counter += @event.Count;
        }
    }
}