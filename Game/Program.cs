using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class Program
    {
        static void Main()
        {
            using (Window window = new Window())
            {
                window.Run(60.0);
            }
        }
    }
}
