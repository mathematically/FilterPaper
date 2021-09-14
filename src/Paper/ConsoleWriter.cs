using System;

namespace Paper
{
    class ConsoleWriter: IConsoleWriter
    {
        public void Write(string output)
        {
            Console.WriteLine(output);
        }
    }
}
