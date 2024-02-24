﻿using System;
using System.IO;
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

            string dv1S;
            var dv1 = new DisplayValues
            {
                AspectRatio = 1.2f, AutoSaveKill = 45, AutoSaveTime = 6, DiskSize = 78233,
                Letter = 'd', Letters = ['a', 'b', 'c'], Money = 3439.44m, Police = 39.2112,
                VeryLong = 939, VeryMid = 202, VeryShort = 495, OneFlag = 29,
                TimeStamp = DateTime.Now, TempDirectory = "super secret",
                Vulcan = 21, ShowStatusBar = true, Image = [1, 2, 3, 4, 5, 6, 7]
            };
            using (var mem1 = new MemoryStream())
            {
                dv1.Write(mem1);
                dv1S = Convert.ToBase64String(mem1.ToArray());
            }
            Console.WriteLine(dv1S);

            var dv2 = new DisplayValues();
            var dv2B = Convert.FromBase64String(dv1S);
            using (var mem2 = new MemoryStream(dv2B))
            {
                dv2.Read(mem2);
            }
            Console.WriteLine(dv2);
        }
    }
}