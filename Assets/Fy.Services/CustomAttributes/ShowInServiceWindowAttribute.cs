using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitUse]
    [Preserve]
    public sealed class ShowInServiceWindowAttribute : Attribute { }
}
