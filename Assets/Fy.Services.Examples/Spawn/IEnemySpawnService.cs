namespace Fy.Services.Examples
{
    // The concrete service in the family. This is the interface you actually
    // resolve. It inherits the shared ISpawnService contract but, because it
    // isn't itself [AbstractService], the auto-loader gives it a factory.
    public interface IEnemySpawnService : ISpawnService { }
}
