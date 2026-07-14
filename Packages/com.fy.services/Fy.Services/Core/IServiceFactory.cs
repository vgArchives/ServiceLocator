namespace Fy.Services
{
    public interface IServiceFactory
    {
        bool ShouldCacheService => true;

        IService GetService();
    }
}