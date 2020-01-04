using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace Telimena.WebApp.AppIntegrationTests.Utilities
{
    public class WindowHelpers
    {

        public static void ClickButtonByText(Window window, string text)
        {
            var btn = window.FindFirstChild(x => x.ByControlType(ControlType.Button).And(x.ByText(text))).AsButton();

            btn.Invoke();
        }

        public static async Task<Window> WaitForWindowAsync(Expression<Predicate<string>> match, TimeSpan timeout, string processName=null)
        {
            Window win = null;
            var compiled = match.Compile();

            Stopwatch timeoutWatch = Stopwatch.StartNew();

            while (true)
            {
                await Task.Delay(50).ConfigureAwait(false);

                
                Process[] allProcesses;
                if (processName != null)
                {
                    allProcesses = Process.GetProcessesByName(processName).Where(x => !string.IsNullOrEmpty(x.MainWindowTitle)).ToArray();
                }
                else
                {
                    allProcesses = Process.GetProcesses().Where(x => !string.IsNullOrEmpty(x.MainWindowTitle)).ToArray();
                }

                foreach (Process allProcess in allProcesses)
                {
                    if (!compiled.Invoke(allProcess.MainWindowTitle))
                    {
                        continue;
                    }

                    Application app = Application.Attach(allProcess);
                    using (var automation = new UIA3Automation())
                    {
                        var window = app.GetMainWindow(automation);
                        if (window != null)
                        {
                            return window;
                        }
                    }
                  
                }
                if (timeoutWatch.Elapsed > timeout)
                {
                    string expBody = ((LambdaExpression)match).Body.ToString();
                    throw new InvalidOperationException($"Failed to find window by expression on Title: {expBody}. " );

                }
            }
        }

    

      


        public static async Task<Window> WaitForMessageBoxAsync(Window parent, string title, TimeSpan timeout, string errorMessage = "")
        {
            Window win = null;
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            while (win == null)
            {
                await Task.Delay(50).ConfigureAwait(false);
                try
                {
                    win = parent.ModalWindows.FirstOrDefault(x => x.IsModal);
                }
                catch (Exception)
                {
                    //
                }

                if (timeoutWatch.Elapsed > timeout)
                {
                    throw new InvalidOperationException($"Failed to find MessageBox {errorMessage}. " +
                                                        $"Parent window title: {parent.Title}, IsAvailable: {parent.IsAvailable}. IsEnabled: {parent.IsEnabled}. IsOffscreen: {parent.IsOffscreen}");
                }
            }

            return win;
        }
    }
}
