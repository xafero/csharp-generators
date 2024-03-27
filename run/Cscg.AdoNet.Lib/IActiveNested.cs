namespace Cscg.AdoNet.Lib
{
    public interface IActiveNested<out T> : IActiveNested
    {
        new T Inner { get; }
    }

    public interface IActiveNested
    {
        object Inner { get; }
    }
}