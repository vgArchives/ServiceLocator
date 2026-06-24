using Fy.Services;

namespace Fy.Services.Examples
{
    // Most services are created lazily, the first time something asks for them.
    // [PreloadService] (see the implementation) flips that: the service is built
    // up front, before the first scene loads, instead of on first use.
    //
    // Reach for it when the cost of building the service is something you'd
    // rather pay during loading than mid-gameplay, like reading localization
    // tables off disk.
    public interface ILocalizationService : IService
    {
        string Get(string key);
    }
}
