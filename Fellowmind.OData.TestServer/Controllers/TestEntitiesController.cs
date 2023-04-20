using Fellowmind.OData.Test.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fellowmind.OData.TestServer.Controllers
{
    public class TestEntitiesController : ODataController
    {
        private readonly Context _context;

        public TestEntitiesController(Context context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<TestEntity> Get()
        {
            return _context.TestEntities;
        }

        [EnableQuery]
        public SingleResult<TestEntity> Get([FromODataUri] Guid key)
        {
            IQueryable<TestEntity> result = _context.TestEntities.Where(x => x.Id == key);
            return SingleResult.Create(result);
        }

        public async Task<CreatedODataResult<TestEntity>> Post(TestEntity entity)
        {
            _context.TestEntities.Add(entity);
            await _context.SaveChangesAsync();

            return Created(entity);
        }

        /// <summary>
        /// Partial update of entity.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<UpdatedODataResult<TestEntity>> Patch([FromODataUri] Guid key, Delta<TestEntity> entity)
        {
            TestEntity testEntity = _context.TestEntities.FirstOrDefault(x => x.Id == key);
            foreach(string propertyName in entity.GetChangedPropertyNames())
            {
                object value = entity.GetInstance().GetType().GetProperty(propertyName).GetValue(entity.GetInstance());
                testEntity.GetType().GetProperty(propertyName).SetValue(testEntity, value);
            }

            await _context.SaveChangesAsync();
            return Updated(testEntity);
        }

        /// <summary>
        /// Full update of entity.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<UpdatedODataResult<TestEntity>> Put([FromODataUri] Guid key, TestEntity entity)
        {
            if (!ModelState.IsValid)
            {
                throw new InvalidOperationException($"Invalid model state.");
            }

            return Updated(new TestEntity() { Id = Guid.Empty, Name = "Jee" });
        }

        public async Task<StatusCodeResult> Delete([FromODataUri] Guid key)
        {
            TestEntity testEntity = _context.TestEntities.FirstOrDefault(x => x.Id == key);
            _context.TestEntities.Remove(testEntity);
            await _context.SaveChangesAsync();

            return StatusCode((int)System.Net.HttpStatusCode.NoContent);
        }
    }
}
