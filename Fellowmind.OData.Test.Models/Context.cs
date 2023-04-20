using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fellowmind.OData.Test.Models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {
            GenerateTestData();
        }

        private void GenerateTestData()
        {
            for (double id = 100000000000; id < 100000000010; id++)
            {
                TestEntities.Add(new TestEntity()
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-{id}"),
                    Name = $"Entity: {id}",
                });
            }

            SaveChanges();
        }

        public DbSet<TestEntity> TestEntities { get; set; }
    }
}
