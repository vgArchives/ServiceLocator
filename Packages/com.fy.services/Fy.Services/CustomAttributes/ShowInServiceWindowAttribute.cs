using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    /// <summary>
    /// Put this on a service field or property to show its value in the Service Locator editor window.
    /// </summary>
    /// <remarks>
    /// The window reads the marked members and displays them read-only, so you can inspect a running service's state.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitUse]
    [Preserve]
    public sealed class ShowInServiceWindowAttribute : Attribute { }
}
