using System;

namespace KirosDungeons
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new KirosDungeons())
                game.Run();
        }
    }
}
