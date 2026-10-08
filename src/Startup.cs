using System;
using Microsoft.Win32;

namespace EyeRest
{
    internal interface IStartupRegistration
    {
        string Read();
        void Write(string command);
    }
    internal sealed class StartupRegistration : IStartupRegistration
    {
        private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        public string Read()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath, false))
                return key == null ? null : key.GetValue("EyeRest", null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
        }
        public void Write(string command)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath))
            {
                if (command == null) key.DeleteValue("EyeRest", false);
                else key.SetValue("EyeRest", command, RegistryValueKind.String);
            }
        }
        public static string CommandFor(string executable)
        {
            if (String.IsNullOrWhiteSpace(executable) || executable.IndexOf('"') >= 0) throw new ArgumentException("Invalid executable path");
            return "\"" + executable + "\" --startup";
        }
    }
    internal static class SettingsCommit
    {
        public static void Save(Preferences value, SettingsStore store, IStartupRegistration startup, string executable)
        {
            string before = startup.Read();
            string after = value.StartWithWindows ? StartupRegistration.CommandFor(executable) : null;
            bool changed = !String.Equals(before, after, StringComparison.Ordinal);
            if (changed) startup.Write(after);
            try { store.Save(value); }
            catch (Exception saveError)
            {
                if (changed)
                {
                    try { startup.Write(before); }
                    catch (Exception rollbackError) { throw new AggregateException("设置保存失败，且开机自启状态未能恢复。请重新检查自启选项。", saveError, rollbackError); }
                }
                throw;
            }
        }
    }
}
