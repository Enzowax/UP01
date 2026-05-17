using System;
using System.IO;
using System.Text.Json;

namespace ShutIKrol.Data
{
    public class DbSettings
    {
        public string Server   { get; set; } = ".\\SQLEXPRESS";
        public string Database { get; set; } = "UP_ShutIKrol";
        public bool   WinAuth  { get; set; } = true;
        public string Login    { get; set; } = "";
        public string Password { get; set; } = "";

        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_settings.json");

        public static DbSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<DbSettings>(File.ReadAllText(FilePath))
                           ?? new DbSettings();
            }
            catch { }
            return new DbSettings();
        }

        public void Save()
        {
            try { File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); }
            catch { }
        }

        public string ToConnectionString()
        {
            string auth = WinAuth
                ? "Integrated Security=True;"
                : $"User Id={Login};Password={Password};";
            return $"Data Source={Server};Initial Catalog={Database};{auth}TrustServerCertificate=True;";
        }
    }
}
