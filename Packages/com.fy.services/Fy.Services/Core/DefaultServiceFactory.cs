namespace Fy.Services
{
    public sealed class DefaultServiceFactory<T> : IServiceFactory where T : class, IService, new()
    {
        public static readonly DefaultServiceFactory<T> Instance = new();
        
        private DefaultServiceFactory() { }
        
        public IService GetService()
        {
            return new T();
        }
    }
}