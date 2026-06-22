using Fy.Services;

namespace Fy.Services.Examples
{
    // [RequiredService] says "the game cannot run without this".
    //
    // It pairs with ServiceLocator.GetChecked<T>(): for a required service
    // GetChecked returns the instance, and if for some reason it can't be
    // resolved it throws instead of handing back null. Calling GetChecked on a
    // service that is NOT marked required logs a warning, nudging you toward
    // TryGet for optional things.
    //
    // The auto-loader also validates at startup that every required service has
    // a way to be created, so a missing one is caught early.
    [RequiredService]
    public interface ISaveService : IService
    {
        void Save(string slot);
        bool TryLoad(string slot, out string data);
    }
}
