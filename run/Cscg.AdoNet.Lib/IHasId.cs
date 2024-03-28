namespace Cscg.AdoNet.Lib
{
    public interface IHasId<out T>
    {
        T Id { get; }
    }
}