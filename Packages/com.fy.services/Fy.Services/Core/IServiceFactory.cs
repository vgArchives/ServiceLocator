namespace Fy.Services
{
    /// <summary>
    /// Knows how to build a service instance on demand for the <see cref="ServiceLocator"/>.
    /// </summary>
    /// <remarks>
    /// Register one with <see cref="ServiceLocator.SetFactory{T}"/>. The locator calls it the first time a service
    /// is requested and, unless told otherwise, caches the result.
    /// </remarks>
    public interface IServiceFactory
    {
        /// <summary>
        /// Whether the locator should cache and reuse the created instance (a singleton) or treat it as a
        /// throwaway value built fresh on every request. Defaults to caching.
        /// </summary>
        bool ShouldCacheService => true;

        /// <summary>
        /// Builds and returns the service instance.
        /// </summary>
        /// <returns>The service instance to use.</returns>
        IService GetService();
    }
}