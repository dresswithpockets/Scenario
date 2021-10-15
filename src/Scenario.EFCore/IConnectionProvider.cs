using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Scenario.EFCore
{
    public interface IConnectionProvider<TContext> : IDisposable
        where TContext : DbContext
    {
        DbConnection Connection { get; }
        
        DbContextOptions<TContext> Options { get; }
    }
}