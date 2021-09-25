using System;

namespace Paper
{
    public class ConsoleWriter: IConsoleWriter
    {
        public void WriteLine(string output)
        {
            Console.WriteLine(output);
        }
    }
}