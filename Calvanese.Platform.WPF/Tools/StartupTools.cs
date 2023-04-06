using Microsoft.Win32;
using System;
using System.Windows;

namespace Calvanese.Platform.WPF.Tools;

public class StartupTools {
    #region Methods (Check)

    public static bool IsInStartup(string registryKeyTitle, string executablePath, bool isForAllUsers = false) {
        try {
            // Prepare the registry key.
            RegistryKey? registryKey;

            // Open the startup registry key.
            if(isForAllUsers) registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            else registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            // Resolve the registry key value.
            string? registryKeyValue = registryKey?.GetValue(registryKeyTitle)?.ToString();

            // Signify failure when null.
            if(registryKeyValue == null) return false;

            // Signify success when we found a match.
            if(registryKeyValue.ToLower().Equals(executablePath.ToLower())) return true;

            // Signify failure.
            return false;
        }
        catch(Exception) {}

        // Signify failure.
        return false;
    }

    #endregion

    #region Methods (Removal)

    public static bool RemoveFromStartup(string registryKeyTitle, string executablePath, bool isForAllUsers = false) {
        try {
            // Prepare the registry key.
            RegistryKey? registryKey;

            // Open the startup registry key.
            if(isForAllUsers) registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            else registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            
            // Delete the registry key value as long as the executable path matches.
            if(registryKey?.GetValue(registryKeyTitle)?.ToString()?.ToLower() == executablePath.ToLower()) {
                // Delete the registry key value.
                registryKey?.DeleteValue(registryKeyTitle);

                // Signify success.
                return true;
            }

            // Signify failure.
            return false;
        }
        catch(Exception) {}

        // Signify failure.
        return false;
    }

    #endregion

    #region Methods (Startup)

    public static bool AddToStartup(string registryKeyTitle, string executablePath, bool isForAllUsers = false) {
        // Prepare the registry key.
        RegistryKey? registryKey;

        try {
            // Open the startup registry key.
            if(isForAllUsers) registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            else registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            // Create the registry key value.
            registryKey?.SetValue(registryKeyTitle, executablePath);
        }
        catch(Exception) {
            // Signify failure.
            return false;
        }

        // Signify success.
        return true;
    }

    #endregion
}
