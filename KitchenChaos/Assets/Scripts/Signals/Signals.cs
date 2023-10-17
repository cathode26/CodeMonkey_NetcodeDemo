using System;
using System.Collections.Generic;

public interface ISignal { }

static public class Signals
{
    static SignalHub signalHub = new SignalHub();
    static public T Get<T>() where T : ISignal, new()
    {
        return signalHub.Get<T>();
    }
}
public class SignalHub
{
    private Dictionary<Type, ISignal> typeToSignals = new Dictionary<Type, ISignal>();
    public T Get<T>() where T : ISignal, new()
    {
        Type type = typeof(T);
        if (typeToSignals.TryGetValue(type, out ISignal signal))
            return (T)signal;

        T instance = (T)Activator.CreateInstance(type);
        typeToSignals[type] = instance;
        return instance;
    }
}
public class Signal : ISignal
{
    public event Action signal;
    public void AddListener(Action action)
    {
        signal += action;
    }
    public void RemoveListener(Action action)
    {
        signal -= action;
    }
    public void Dispatch()
    {
        signal?.Invoke();
    }
}
public class Signal<T> : ISignal
{
    public event Action<T> signal;
    public void AddListener(Action<T> action)
    {
        signal += action;
    }
    public void RemoveListener(Action<T> action)
    {
        signal -= action;
    }
    public void Dispatch(T t)
    {
        signal?.Invoke(t);
    }
}
public class Signal<T, E> : ISignal
{
    public event Action<T, E> signal;
    public void AddListener(Action<T, E> action)
    {
        signal += action;
    }
    public void RemoveListener(Action<T, E> action)
    {
        signal -= action;
    }
    public void Dispatch(T t, E e)
    {
        signal?.Invoke(t, e);
    }
}
public class Signal<T, E, F> : ISignal
{
    public event Action<T, E, F> signal;
    public void AddListener(Action<T, E, F> action)
    {
        signal += action;
    }
    public void RemoveListener(Action<T, E, F> action)
    {
        signal -= action;
    }
    public void Dispatch(T t, E e, F f)
    {
        signal?.Invoke(t, e, f);
    }
}