using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telimena.TestUtilities.Base
{
    public static class SharedTestHelpers
    {
        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
        public static void DeleteDirectory(string path)
        {

            string[] subDirs = new string[0];

            try
            {

             subDirs = Directory.GetDirectories(path);
            }
            catch { //ok, maybe the dir does not exist
                    }

            foreach (string directory in subDirs)
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (DirectoryNotFoundException)
            {
                //very well
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(50);
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(50);
                Directory.Delete(path, true);
            }
        }
    
        [DebuggerStepThrough]
        public static string GetMethodName([CallerMemberName] string caller = "")
        {
            StackTrace stackTrace = new System.Diagnostics.StackTrace();
            var frames = stackTrace.GetFrames();
            for (int index = 1; index < frames.Length; index++)
            {
                StackFrame stackFrame = frames[index];
                var methodBase = stackFrame.GetMethod();
                if (methodBase.Name == "MoveNext")
                  
                {
                    continue;
                }
                if (methodBase.DeclaringType != null && !
                        (methodBase.DeclaringType.Assembly.FullName.StartsWith("mscorlib")
                         || methodBase.DeclaringType.Assembly.FullName.StartsWith("System.Net.Http")
                         || methodBase.DeclaringType.Assembly.FullName.StartsWith("System, ")
                         ))
                {
                    return methodBase.Name;
                }
            }

            return caller;
        }
    }
}