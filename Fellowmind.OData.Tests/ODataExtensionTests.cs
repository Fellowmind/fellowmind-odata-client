using Fellowmind.OData.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Client;
using System.Linq;
using Xunit;

namespace Fellowmind.OData.Tests
{
    public class ODataExtensionTests : TestBase
    {
        [Fact]
        public void Tracking_Entity_Will_Start_Tracking()
        {
            TestODataClient helper = ServiceProvider.GetService<TestODataClient>();

            // Here entity tracking setup is done manually.
            DataServiceCollection<TestEntity> collection = helper.InitializeTracking<TestEntity>();

            Assert.Empty(helper.GetDataContext().EntityTracker.Entities);
            Assert.Empty(collection);

            TestEntity obj = new TestEntity();
            helper.TrackEntity(obj);

            Assert.Single(helper.GetDataContext().EntityTracker.Entities);
            Assert.Single(collection);
        }

        [Fact]
        public void Tracking_Entity_Will_Start_Tracking2()
        {
            TestODataClient helper = ServiceProvider.GetService<TestODataClient>();

            // Here entity tracking setup is automated.

            Assert.Empty(helper.GetDataContext().EntityTracker.Entities);
            Assert.False(helper.IsTrackingInitialized<TestEntity>());

            TestEntity obj = helper.CreateAndTrackEntity<TestEntity>();

            Assert.Single(helper.GetDataContext().EntityTracker.Entities);
            Assert.True(helper.IsTrackingInitialized<TestEntity>());
        }

        [Fact]
        public void Tracked_Entity_Will_Change_State_On_Update()
        {
            TestODataClient helper = ServiceProvider.GetService<TestODataClient>();

            // Here entity tracking setup is automated.
            TestEntity obj = helper.CreateAndTrackEntity<TestEntity>();

            EntityDescriptor entity = helper.GetDataContext().EntityTracker.Entities.First();

            // Hack to set state to unchanged.
            entity.GetType().GetProperty("State").SetValue(entity, EntityStates.Unchanged);
            Assert.True(entity.State == EntityStates.Unchanged);

            //Assert.Raises<PropertyChangedEventArgs>(
            //    h => obj.PropertyChanged += (o, e) => h(o, e),
            //    h => obj.PropertyChanged -= (o, e) => h(o, e),
            //    () => obj.Name = "Test");
            obj.Name = "Test";
            Assert.True(entity.State == EntityStates.Modified);
        }

        [Fact]
        public void Untracking_Entity_Will_Not_Cause_Delete()
        {
            TestODataClient helper = ServiceProvider.GetService<TestODataClient>();

            DataServiceCollection<TestEntity> collection = helper.InitializeTracking<TestEntity>();
            TestEntity obj = new TestEntity();

            helper.TrackEntity(obj);

            EntityDescriptor entity = helper.GetDataContext().EntityTracker.Entities.First();
            helper.UntrackEntity(obj);

            // Only state "Deleted" will cause delete.
            Assert.True(entity.State == EntityStates.Detached);
            Assert.Empty(collection);
            Assert.Empty(helper.GetDataContext().Entities);
        }

        // Re-enable after there are two entities.
        //[Fact]
        //public async Task Saving_Changes_Will_Reset_Collection_And_Context()
        //{
        //    TestODataClient helper = new TestODataClient();

        //    DataServiceCollection<TestEntity> collection = helper.InitializeTracking<TestEntity>();
        //    TestEntity obj = new TestEntity();

        //    helper.TrackEntity(obj);

        //    DataServiceCollection<TestEntity2> collection2 = helper.InitializeTracking<TestEntity2>();
        //    helper.TrackEntity(new TestEntity2());
        //    helper.TrackEntity(new TestEntity2());

        //    Assert.True(collection2.Count == 2);
        //    Assert.True(helper.GetDataContext().Entities.Count == 3);

        //    helper.TestMode = true; // Save doesn't try to go to actual endpoint.
        //    await helper.SaveChangesAsync();
        //    helper.TestMode = false;

        //    Assert.Empty(collection);
        //    Assert.Empty(collection2);
        //    Assert.Empty(helper.GetDataContext().Entities);
        //}

            // TODO: Re-enable after there are two entities.
        //[Fact]
        //public void Untrack_All_Resets_All_Collections()
        //{
        //    TestODataClient helper = new TestODataClient();

        //    DataServiceCollection<TestEntity> collection = helper.InitializeTracking<TestEntity>();
        //    TestEntity obj = new TestEntity();

        //    helper.TrackEntity(obj);

        //    DataServiceCollection<TestEntity2> collection2 = helper.InitializeTracking<TestEntity2>();
        //    helper.TrackEntity(new TestEntity2());
        //    helper.TrackEntity(new TestEntity2());

        //    Assert.True(collection.Count == 1);
        //    Assert.True(collection2.Count == 2);
        //    Assert.True(helper.GetDataContext().Entities.Count == 3);

        //    helper.UntrackAllTest();

        //    Assert.Empty(collection);
        //    Assert.Empty(collection2);
        //    Assert.Empty(helper.GetDataContext().Entities);
        //}

            // Re-enable after there are two entities.
        //[Fact]
        //public void Untrack_All_For_Type_Resets_All_For_Type()
        //{
        //    TestODataClient helper = new TestODataClient();

        //    DataServiceCollection<TestEntity> collection = helper.InitializeTracking<TestEntity>();
        //    TestEntity obj = new TestEntity();

        //    helper.TrackEntity(obj);

        //    DataServiceCollection<TestEntity2> collection2 = helper.InitializeTracking<TestEntity2>();
        //    helper.TrackEntity(new TestEntity2());
        //    helper.TrackEntity(new TestEntity2());

        //    Assert.True(collection.Count == 1);
        //    Assert.True(collection2.Count == 2);
        //    Assert.True(helper.GetDataContext().Entities.Count == 3);

        //    helper.UntrackAllTest<TestEntity2>();

        //    Assert.True(collection.Count == 1);
        //    Assert.Empty(collection2);
        //    Assert.True(helper.GetDataContext().Entities.Count == 1);
        //}

        [Fact]
        public void After_Using_Client_Is_Properly_Disposed()
        {
            TestODataClient helper = ServiceProvider.GetService<TestODataClient>();
            using (helper)
            {
                helper.TrackEntity(new TestEntity());
            }

            Assert.Null(helper.GetDataContext());
            Assert.False(helper.IsTrackingInitialized<TestEntity>());
        }

        [Fact]
        public void Non_Tracked_Entity_Can_Be_Attached_And_Detached()
        {
            TestODataClient helper = ServiceProvider.GetService<TestODataClient>();

            TestEntity obj = new TestEntity();
            helper.AttachEntity(obj);

            Assert.NotEmpty(helper.GetDataContext().Entities);

            helper.DetachEntity(obj);

            Assert.Empty(helper.GetDataContext().Entities);
        }
    }
}
