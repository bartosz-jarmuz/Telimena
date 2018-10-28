using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems.WindowItems;

namespace Telimena.WebApp.UITests.Base
{
    public static class TestHelpers
    {

        public static async Task<Window> WaitForWindowAsync(Predicate<string> match, TimeSpan timeout, string errorMessage = "")
        {
            Window win = null;
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            while (true)
            {
                await Task.Delay(50);

                Process[] allProcesses = Process.GetProcesses();

                foreach (Process allProcess in allProcesses)
                {
                    if (!match.Invoke(allProcess.MainWindowTitle))
                    {
                        continue;
                    }

                    Application app = TestStack.White.Application.Attach(allProcess);
                    win = app.Find(match, InitializeOption.NoCache);
                    if (win != null)
                    {
                        return win;
                    }
                }

                if (timeoutWatch.Elapsed > timeout)
                {
                    throw new InvalidOperationException($"Failed to find window {errorMessage}");
                }
            }

            return win;
        }

        public static async Task<Window> WaitForMessageBoxAsync(Window parent, string title, TimeSpan timeout, string errorMessage = "")
        {
            Window win = null;
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            while (win == null)
            {
                await Task.Delay(50);
                win = parent.MessageBox(title);

                if (timeoutWatch.Elapsed > timeout)
                {
                    throw new InvalidOperationException($"Failed to find MessageBox {errorMessage}");
                }
            }

            return win;
        }
    }
}