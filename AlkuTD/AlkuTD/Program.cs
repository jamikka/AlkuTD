using System;

namespace AlkuTD
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CurrentGame game = new CurrentGame())
            {
                game.Run();
            }
        }
    }
#endif
}

