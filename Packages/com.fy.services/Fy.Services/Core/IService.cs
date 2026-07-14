// ReSharper disable RequiredBaseTypesIsNotInherited

using System;

namespace Fy.Services
{
    /// <summary>
    /// Base type every service must implement to be used with the <see cref="ServiceLocator"/>.
    /// </summary>
    /// <remarks>
    /// A service is a pair: a definition interface that extends this one, and a concrete class that implements it.
    /// You ask for the interface and the locator hands you the implementation. Being disposable lets the locator
    /// tear a service down when it is replaced or when play mode ends.
    /// </remarks>
    [AbstractService]
    public interface IService : IDisposable { }
}
    
