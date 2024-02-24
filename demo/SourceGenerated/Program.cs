using System;
using autogen;
using test;

namespace SourceGenerated
{
    [Funny]
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine(ConstStrings.Greeting1);
        }
    }
}

/*
       public static void DisplayValues()
       {
           float aspectRatio;
           string tempDirectory;
           int autoSaveTime;
           bool showStatusBar;

           if (File.Exists(fileName))
           {
               using (var stream = File.Open(fileName, FileMode.Open))
               {
                   using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                   {
                       aspectRatio = reader.ReadSingle();
                       tempDirectory = reader.ReadString();
                       autoSaveTime = reader.ReadInt32();
                       showStatusBar = reader.ReadBoolean();
                   }
               }

               Console.WriteLine("Aspect ratio set to: " + aspectRatio);
               Console.WriteLine("Temp directory is: " + tempDirectory);
               Console.WriteLine("Auto save time set to: " + autoSaveTime);
               Console.WriteLine("Show status bar: " + showStatusBar);
           }
       }
 *
 */