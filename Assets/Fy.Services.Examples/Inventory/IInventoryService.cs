using System.Collections.Generic;
using Fy.Services;

namespace Fy.Services.Examples
{
    // The simplest kind of service: an ordinary C# class behind an interface.
    //
    // You don't have to register this anywhere. Because the type implements
    // IService and has a parameterless constructor, the ServiceLocator's
    // auto-loader hands out a default factory for it. The first time someone
    // calls ServiceLocator.TryGet<IInventoryService>() the instance is created
    // and cached. This is the pattern you'll reach for most of the time.
    public interface IInventoryService : IService
    {
        IReadOnlyDictionary<string, int> Items { get; }

        void Add(string item, int amount = 1);
        bool Remove(string item, int amount = 1);
        int Count(string item);
    }
}
