using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    /// <summary>
    /// Put this on a service interface to declare that it must always be available.
    /// </summary>
    /// <remarks>
    /// Enables <see cref="ServiceLocator.GetChecked{T}"/> for the service, and lets the
    /// <see cref="ServiceAutoLoader"/> warn at startup if no implementation is registered.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface)]
    [BaseTypeRequired(typeof(IService))]
    [Preserve]
    public sealed class RequiredServiceAttribute : Attribute { }
}