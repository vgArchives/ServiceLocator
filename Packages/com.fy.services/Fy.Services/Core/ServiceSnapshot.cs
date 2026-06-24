using System;

namespace Fy.Services
{
    internal readonly struct ServiceSnapshot
    {
        public readonly Type InterfaceType;
        public readonly IService Value;
        public readonly IServiceFactory Factory;
        public readonly bool IsDynamic;
        public readonly bool IsRequired;

        public ServiceSnapshot(Type interfaceType, IService value, IServiceFactory factory,
            bool isDynamic, bool isRequired)
        {
            InterfaceType = interfaceType;
            Value = value;
            Factory = factory;
            IsDynamic = isDynamic;
            IsRequired = isRequired;
        }
    }
}
