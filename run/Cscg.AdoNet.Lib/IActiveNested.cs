namespace Cscg.AdoNet.Lib
{
    public interface IActiveNested<out T>
    {
        T Inner { get; }
    }
}