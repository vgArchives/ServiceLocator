namespace Fy.Services
{
    /// <summary>
    /// The default factory for a plain service class that has a public parameterless constructor. Builds the service
    /// with <c>new T()</c>.
    /// </summary>
    /// <typeparam name="T">The concrete service class.</typeparam>
    public sealed class DefaultServiceFactory<T> : IServiceFactory where T : class, IService, new()
    {
        /// <summary>
        /// The shared instance. The factory holds no state, so one instance serves every request.
        /// </summary>
        public static readonly DefaultServiceFactory<T> Instance = new();

        private DefaultServiceFactory() { }

        /// <inheritdoc/>
        public IService GetService()
        {
            return new T();
        }
    }
}