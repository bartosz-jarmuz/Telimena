using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TestStack.White;
using TestStack.White.UIItems.WindowItems;

namespace Telimena.WebApp.UITests.IntegrationTests.BackwardCompatibilityIntegrationTests
{
    public static class TestHelpers
    {
        public static async Task<Window> WaitForWindowAsync(Predicate<Window> match, TimeSpan timeout, string errorMessage = "")
        {
            Window win = null;
            var timeoutWatch = Stopwatch.StartNew();
            while (win == null)
            {
                await Task.Delay(50);
                win = Desktop.Instance.Windows().Find(match);

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
            var timeoutWatch = Stopwatch.StartNew();
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