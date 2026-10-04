namespace Mannan.Core.Bootstrap
{
    public interface IInitializable
    {
        void Initialize();
        void Shutdown();
    }
}
