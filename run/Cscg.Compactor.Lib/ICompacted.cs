namespace Cscg.Compactor.Lib
{
    public interface ICompacted
    {
        void ReadBy(ICompactor c);

        void WriteBy(ICompactor c);
    }
}