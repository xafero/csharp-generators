using System.IO;

namespace Cscg.AdoNet.Lib
{
    public static class IoTool
    {
        public static string DeleteIfExists(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
            return file;
        }
    }
}