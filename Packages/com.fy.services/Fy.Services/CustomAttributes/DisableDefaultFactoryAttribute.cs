using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    /// <summary>
    /// Put this on a service class to stop the auto-loader from registering a default factory for it.
    /// </summary>
    /// <remarks>
    /// Use it when the service needs custom construction: register your own factory with
    /// <see cref="ServiceLocator.SetFactory{T}"/> or set the instance with <see cref="ServiceLocator.SetService{T}"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IService))]
    [Preserve]
    public sealed class DisableDefaultFactoryAttribute : Attribute { }
}