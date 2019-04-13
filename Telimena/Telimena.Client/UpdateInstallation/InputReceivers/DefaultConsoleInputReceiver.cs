using System;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    internal class DefaultConsoleInputReceiver : IReceiveUserInput
    {
        public bool ShowInstallUpdatesNowQuestion(string maxVersion, long totalDownloadSize, LiveProgramInfo programInfo)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"An update to version {maxVersion} is downloaded and available for immediate installation (total size: {totalDownloadSize.GetSizeString()})." +
                              $"\r\nWould you like to install now?" +
                              $"\r\n[Y]es / [N]o");

            ConsoleKeyInfo choice = Console.ReadKey();
            while (choice.Key != ConsoleKey.Y && choice.Key != ConsoleKey.N)
            {
                Console.WriteLine("Invalid choice. Press Y for [Y]es or N for [N]o");
                choice = Console.ReadKey();
            }

            Console.ForegroundColor = currentColor;
            return choice.Key == ConsoleKey.Y;
        }

        public bool ShowDownloadAndInstallUpdatesQuestion(string maxVersion, long totalDownloadSize, LiveProgramInfo programInfo)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"An update to version {maxVersion} is available for immediate download (total size: {totalDownloadSize.GetSizeString()})." +
                              $"\r\nWould you like to install now?" +
                              $"\r\n[Y]es / [N]o");

            ConsoleKeyInfo choice = Console.ReadKey();
            while (choice.Key != ConsoleKey.Y && choice.Key != ConsoleKey.N)
            {
                Console.WriteLine("Invalid choice. Press Y for [Y]es or N for [N]o");
                choice = Console.ReadKey();
            }

            Console.ForegroundColor = currentColor;
            return choice.Key == ConsoleKey.Y;
        }
    }
}