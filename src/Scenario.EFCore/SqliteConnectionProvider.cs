using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Scenario.EFCore
{
    public class SqliteConnectionProvider<TContext> : IConnectionProvider<TContext>
        where TContext : DbContext
    {
        public DbConnection Connection { get; }
        
        public DbContextOptions<TContext> Options { get; }

        public SqliteConnectionProvider(Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
        {
            Options = new DbContextOptionsBuilder<TContext>().UseSqlite(CreateTemporaryDatabase(), sqliteOptionsAction)
                .Options;
            Connection = RelationalOptionsExtension.Extract(Options).Connection;
        }

        private static DbConnection CreateTemporaryDatabase()
        {
            var tempDb = Path.GetTempFileName();
            var connection = new SqliteConnection($"Filename={tempDb}");
            connection.Open();
            return connection;
        }

        public void Dispose() => Connection.Dispose();
    }
}