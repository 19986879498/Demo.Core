using System;
using Microsoft.EntityFrameworkCore;

namespace Demo.Core
{
    public class DBContextOracle: Microsoft.EntityFrameworkCore.DbContext
    {
        public DBContextOracle(DbContextOptions<DBContextOracle> db):base(db)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
