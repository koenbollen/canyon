using System;

namespace Canyon
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CanyonGame game = new CanyonGame())
            {
                game.Run();
            }
        }
    }
#endif
}

