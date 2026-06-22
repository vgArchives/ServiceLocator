namespace Fy.Services
{
    public interface IServiceFactory
    {
        bool ShouldSetService => true;

        IService GetService();
    }
}