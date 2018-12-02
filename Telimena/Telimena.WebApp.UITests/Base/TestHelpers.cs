using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems.WindowItems;

namespace Telimena.WebApp.UITests.Base
{
    public static class TestHelpers
    {

        public static async Task<Window> WaitForWindowAsync(Expression<Predicate<string>> match, TimeSpan timeout, string errorMessage = "")
        {
            Window win = null;
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            while (true)
            {
                await Task.Delay(50);

                Process[] allProcesses = Process.GetProcesses().Where(x => !string.IsNullOrEmpty(x.MainWindowTitle)).ToArray();
                var compiled = match.Compile();

                foreach (Process allProcess in allProcesses)
                {
                    if (!compiled.Invoke(allProcess.MainWindowTitle))
                    {
                        continue;
                    }

                    Application app = TestStack.White.Application.Attach(allProcess);
                    win = app.Find(compiled, InitializeOption.NoCache);
                    if (win != null)
                    {
                        return win;
                    }
                }
                if (timeoutWatch.Elapsed > timeout)
                {
                    string expBody = ((LambdaExpression)match).Body.ToString();

                    throw new InvalidOperationException($"Failed to find window by expression on Title: {expBody}.{errorMessage}");
                }
            }
        }


        public static async Task<Window> WaitForMessageBoxAsync(Expression<Predicate<string>> match, string title, TimeSpan timeout, string errorMessage = "")
        {

            Window win = null;
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            while (true)
            {
                await Task.Delay(50);

                Process[] allProcesses = Process.GetProcesses().Where(x => !string.IsNullOrEmpty(x.MainWindowTitle)).ToArray();
                var compiled = match.Compile();

                var matchinApps = allProcesses.Where(x => compiled.Invoke(x.MainWindowTitle)).ToList();

                foreach (Process appProcess in matchinApps)
                {
                    Application app = TestStack.White.Application.Attach(appProcess);
                    win = app.Find(compiled, InitializeOption.NoCache);
                    if (win != null)
                    {
                        try
                        {
                            Window msgBox = win.MessageBox(title);
                            if (msgBox != null)
                            {
                                return msgBox;
                            }
                        }
                        catch (Exception) { }
                        
                    }
                }
                if (timeoutWatch.Elapsed > timeout)
                {
                    string expBody = ((LambdaExpression)match).Body.ToString();

                    throw new InvalidOperationException($"Failed to find window by expression on Title: {expBody}.{errorMessage}");
                }
            }

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