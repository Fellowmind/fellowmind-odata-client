using Microsoft.OData.Client;
using System;

namespace Fellowmind.OData.Test.Models
{
    [EntitySet("TestEntities")]
    public class TestEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
