namespace DeepSignals.Settings.Helpers
{
    public class FileHelper
    {
        public static string ReadFile(string fileName) => File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));

        public static void WriteLine(string fileName, string line) => File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), Environment.NewLine + line);
    }
}