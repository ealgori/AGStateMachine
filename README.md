# AGStateMachine
Simple state machine with external state.

```csharp
public enum States
{    
    FirstState,
    SecondState
}
        
public enum Events
{
    OnTick
}
    
public class StateInstance : IInstance<States>
{
    public States CurrentState { get; set; }
    public int Counter { get; set; }
}
    
public class MyStateMachine : AGStateMachine<States, Events, StateInstance>{}

public static class Program
{
    public static async Task Main()
    {
        var stateInstance = new StateInstance();
        var stateMachine = new MyStateMachine();
         
        stateMachine.AddTransition(States.FirstState, Events.OnTick,
            async (inst) =>
            {
                inst.Counter++;
                if (inst.Counter == 10) // you don`t have to change state on every event;
                    inst.CurrentState = States.SecondState;
            });
                
         // await waits while event will be processed and buble up possible exceptions.
         await stateMachine.RaiseEvent(Events.OnTick, stateInstance);  
         
    }
}

```
Events process in one thread per state instance sequence, which guarantees state consistency.
Under the hood used ActionBlock per state instance and weak references, so state machine will not hold state instance for GC.


Also supported custom typed events:

```csharp
public class BigTick
{
    public int Counter { get; set; }
}
```

```csharp
stateMachine.AddTransition<BigTick>(States.FirstState,
                async (inst, ev) =>
                {
                    inst.Counter += ev.Counter;
                    if (inst.Counter > 20)
                        inst.CurrentState = States.SecondState;
                });
```


```csharp
var bigTick = new BigTick(){Counter = 14};
await stateMachine.RaiseEvent(bigTick, stateInstance);
```

Happy to hear suggestions and pull requests.
