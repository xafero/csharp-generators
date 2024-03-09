using System.IO;
using SourceGenerated.Sql;

namespace SourceGenerated
{
    public static partial class Program
    {
        public static void Main()
        {
            // MainCg();
            // MainBg();

            using var fw = File.CreateText("test.sql");
            fw.WriteLine();
            fw.WriteLine();
            fw.WriteLine(Blog.CreateTable());
            fw.WriteLine();
            fw.WriteLine();
            fw.WriteLine(Funny.CreateTable());
            fw.WriteLine();
            fw.WriteLine();
            fw.WriteLine(House.CreateTable());
            fw.WriteLine();
            fw.WriteLine();
            fw.WriteLine(Person.CreateTable());
            fw.WriteLine();
            fw.WriteLine();
            fw.WriteLine(Profile.CreateTable());
            fw.WriteLine();
            fw.WriteLine();
            fw.WriteLine(Post.CreateTable());
            fw.WriteLine();
            fw.WriteLine();
            fw.WriteLine(HousePerson.CreateTable());
            fw.WriteLine();
            fw.WriteLine();
            fw.WriteLine(User.CreateTable());
            fw.WriteLine();
            fw.WriteLine();
        }
    }
}