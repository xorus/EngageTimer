using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace EngageTimer;

/**
 * Dirt simple service container that only fixes my lazyness.
 */
public class Container
{
    private readonly Dictionary<Type, object> _container = new();
    private readonly List<IDisposable> _disposables = new();

    public TRegister Register<TRegister>(TRegister instance) where TRegister : class
    {
        _container.Add(typeof(TRegister), instance);
        return instance;
    }

    public TRegister Register<[MeansImplicitUse] TRegister>() where TRegister : class
    {
        var instance = (TRegister)Activator.CreateInstance(typeof(TRegister), this);
        return Register(instance);
    }

    public TRegister RegisterDisposable<TRegister>(TRegister instance) where TRegister : IDisposable
    {
        _container.Add(typeof(TRegister), instance);
        _disposables.Add(instance);
        return instance;
    }

    // next level lazy
    public TRegister RegisterDisposable<[MeansImplicitUse] TRegister>() where TRegister : IDisposable
    {
        var instance = (TRegister)Activator.CreateInstance(typeof(TRegister), this);
        return RegisterDisposable(instance);
    }

    public TRegister Resolve<TRegister>() where TRegister : class
    {
        return _container[typeof(TRegister)] as TRegister ?? throw new InvalidOperationException();
    }

    public void DoDispose()
    {
        _disposables.Reverse();
        foreach (var disposable in _disposables) disposable.Dispose();
    }
}