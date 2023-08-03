using System.Diagnostics;
//using System.Security.Cryptography;

namespace AppLauncher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var targetPath = Path.Join(AppContext.BaseDirectory, @"Robocraft2BlockPenSimApp\Robocraft2BlockPenSimApp.exe");
            Process.Start(targetPath);
            
            //try
            //{
            //    string hash;
            //    using(var fs = new FileStream(targetPath, FileMode.Open))
            //    {
            //        using (var sha = SHA256.Create())
            //        {
            //            hash = string.Join("", sha.ComputeHash(fs).Select(b => b.ToString("X2")));
            //            Console.WriteLine(hash);
            //            Console.ReadKey();
            //        }
            //    }
                
            //    var targetHash = "FA5879A71A363A65C062A1A988C160E277B4B4AA45C8DCA758EF6C6C3983D0BA";
            //    if (hash == targetHash)
            //    {
            //        Process.Start(targetPath);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    Console.ReadKey();
            //}
        }
    }
}