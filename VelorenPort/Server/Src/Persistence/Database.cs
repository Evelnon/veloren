using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace VelorenPort.Server.Persistence {
    /// <summary>
    /// Simple SQLite context that applies SQL scripts embedded in the
    /// assembly. It mimics the rust implementation enough for
    /// CharacterLoader.
    /// </summary>
    public sealed class Database : IDisposable {
        public SqliteConnection Connection { get; }

        public Database(string path) {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var builder = new SqliteConnectionStringBuilder { DataSource = path };
            Connection = new SqliteConnection(builder.ConnectionString);
            Connection.Open();
            RunMigrations();
        }

        private void RunMigrations() {
            using var tx = Connection.BeginTransaction();
            Connection.Execute("CREATE TABLE IF NOT EXISTS migrations (version INTEGER PRIMARY KEY)");
            var applied = new HashSet<int>();
            using (var cmd = Connection.CreateCommand()) {
                cmd.Transaction = tx;
                cmd.CommandText = "SELECT version FROM migrations";
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    applied.Add(reader.GetInt32(0));
                }
            }

            var assembly = Assembly.GetExecutingAssembly();
            var prefix = typeof(Database).Namespace + ".Migrations.";
            var resources = assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(prefix) && r.EndsWith(".sql"))
                .OrderBy(r => r);
            foreach (var res in resources) {
                var name = res.Substring(prefix.Length);
                if (name.StartsWith("V") && int.TryParse(name.Split("__")[0][1..], out var version)) {
                    if (applied.Contains(version)) continue;
                    using var stream = assembly.GetManifestResourceStream(res)!;
                    using var reader = new StreamReader(stream);
                    var sql = reader.ReadToEnd();
                    using var cmd = Connection.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    using var insert = Connection.CreateCommand();
                    insert.Transaction = tx;
                    insert.CommandText = "INSERT INTO migrations(version) VALUES($v)";
                    insert.Parameters.AddWithValue("$v", version);
                    insert.ExecuteNonQuery();
                }
            }
            tx.Commit();
        }

        public void Dispose() => Connection.Dispose();
    }

    static class SqliteExtensions {
        public static int Execute(this SqliteConnection conn, string sql) {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd.ExecuteNonQuery();
        }
    }
}
