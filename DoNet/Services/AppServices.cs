using System;
using System.Collections.Generic;

namespace DoNet.Services;

/// <summary>
/// Tiny service locator. Deliberately dependency-free — swap it for
/// Microsoft.Extensions.DependencyInjection when the app grows past a handful of
/// services; call sites only use <see cref="GetRequired{T}"/>.
/// </summary>
public static class AppServices
{
    private static readonly Dictionary<Type, object> Registrations = new();

    /// <summary>Registers the implementations used by the running app.</summary>
    public static void RegisterDefaults()
    {
        Register<IAuthenticationService>(new DemoAuthenticationService());
        Register<ISystemStatusService>(new DemoSystemStatusService());
    }

    public static void Register<TService>(TService instance) where TService : class
        => Registrations[typeof(TService)] = instance;

    public static TService GetRequired<TService>() where TService : class
        => Registrations.TryGetValue(typeof(TService), out var instance)
            ? (TService)instance
            : throw new InvalidOperationException(
                $"No implementation registered for {typeof(TService).Name}. " +
                $"Call {nameof(AppServices)}.{nameof(RegisterDefaults)}() during startup.");
}
