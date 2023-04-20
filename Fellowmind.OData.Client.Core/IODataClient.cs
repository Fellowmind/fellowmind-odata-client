using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.OData.Client;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// Interface defining OData client for communication with FO and CE.
    /// </summary>
    /// <typeparam name="TClient">Undelying CE or FO client type.</typeparam>
    public interface IODataClient<TClient> : IDisposable 
        where TClient : DataServiceContext
    {
        /// <summary>
        /// Fires after DataServiceContext-based OData context is created.
        /// </summary>
        event EventHandler<ContextCreatedEventArgs<TClient>> ContextCreated;

        /// <summary>
        /// Attaches entity to the data context. If an entity is already tracked by another context, that tracking will be removed.
        /// </summary>
        /// <typeparam name="T">Entity type to attach.</typeparam>
        /// <param name="entity">Entity to attach.</param>
        void AttachEntity<T>(T entity) where T : BaseEntityType;

        /// <summary>
        /// Attaches entity to the data context. If an entity is already tracked by another context, that tracking will be removed.
        /// </summary>
        /// <typeparam name="T">Entity type to attach.</typeparam>
        /// <param name="entity">Entity to attach.</param>
        void AttachEntityAndTrack<T>(T entity) where T : BaseEntityType;

        /// <summary>
        /// Detaches given entity from data context.
        /// </summary>
        /// <typeparam name="T">Entity type to detach.</typeparam>
        /// <param name="entity">Entity to detach.</param>
        void DetachEntity<T>(T entity) where T : BaseEntityType;

        /// <summary>
        /// Get entity of the given type and ID.
        /// </summary>
        /// <typeparam name="T">Type of entity to get.</typeparam>
        /// <param name="id">ID of entity to get.</param>
        /// <returns>Entity of the given type and ID or null if not found.</returns>
        Task<T> GetEntityAsync<T>(object id) where T : BaseEntityType, new();

        /// <summary>
        /// Get entity of the given type and ID.
        /// </summary>
        /// <typeparam name="T">Type of entity to get.</typeparam>
        /// <param name="ids">Multi-key ID of entity to get. Key of the dictionary is ID field name and value is the ID value.</param>
        /// <returns>Entity of the given type and ID or null if not found.</returns>
        Task<T> GetEntityAsync<T>(Dictionary<string, object> ids) where T : BaseEntityType, new();

        /// <summary>
        /// Get entity of the given type and ID and mark it for tracking.
        /// </summary>
        /// <typeparam name="T">Type of entity to get.</typeparam>
        /// <param name="id">ID of entity to get.</param>
        /// <returns>Entity of the given type and ID or null if not found.</returns>
        Task<T> GetEntityAndTrackAsync<T>(object id) where T : BaseEntityType, new();

        /// <summary>
        /// Get entity of the given type and ID and mark it for tracking.
        /// </summary>
        /// <typeparam name="T">Type of entity to get.</typeparam>
        /// <param name="ids">Multi-key ID of entity to get. Key of the dictionary is ID field name and value is the ID value.</param>
        /// <returns>Entity of the given type and ID or null if not found.</returns>
        Task<T> GetEntityAndTrackAsync<T>(Dictionary<string, object> ids) where T : BaseEntityType, new();

        /// <summary>
        /// Creates new instance of the specified entity and sets it for change tracking.
        /// </summary>
        /// <typeparam name="T">Type of entity to create.</typeparam>
        /// <returns>New instance of tracked entity.</returns>
        T CreateAndTrackEntity<T>() where T : BaseEntityType, new();

        /// <summary>
        /// Creates new instance of the specified entity.
        /// </summary>
        /// <typeparam name="T">Type of entity to create.</typeparam>
        /// <returns>New entity instance.</returns>
        T CreateEntity<T>() where T : BaseEntityType, new();

        /// <summary>
        /// This creates delete operation for the given entity. To stop tracking of entity use UntrackEntity. To detach an entity use DetachEntity.
        /// </summary>
        /// <typeparam name="T">Type of entity to delete.</typeparam>
        /// <param name="entity">Entity to delete.</param>
        void DeleteEntity<T>(T entity) where T : BaseEntityType;

        /// <summary>
        /// Adds entity to be tracked for changes.
        /// Starting tracking for 'Unchanged' entity creates 'Update' message.
        /// Starting tracking for 'Detached' entity create 'Insert' message.
        /// </summary>
        /// <typeparam name="T">Type of entity to track.</typeparam>
        /// <param name="entity">Entity to track.</param>
        void TrackEntity<T>(T entity) where T : BaseEntityType;

        /// <summary>
        /// Removes entity from being tracked for changes. Does not generate delete operation.
        /// </summary>
        /// <typeparam name="T">Type of entity to untrack.</typeparam>
        /// <param name="entity">Entity to untrack.</param>
        void UntrackEntity<T>(T entity) where T : BaseEntityType;

        /// <summary>
        /// Enables tracking for the given entity type. Tracking is also automatically initialized when calling TrackEntity for non-tracked entity type.
        /// </summary>
        /// <typeparam name="T">Entity type for which tracking is set up.</typeparam>
        /// <returns>Data service collection for tracking the given entity type.</returns>
        DataServiceCollection<T> InitializeTracking<T>() where T : BaseEntityType;

        /// <summary>
        /// Returns true, if tracking has been enabled for the given entity type.
        /// </summary>
        /// <typeparam name="T">Entity type to check.</typeparam>
        /// <returns>True if tracking is enabled.</returns>
        bool IsTrackingInitialized<T>() where T : BaseEntityType;

        /// <summary>
        /// Save changes to the target system and reset the client to the original state.
        /// </summary>
        /// <returns>DataServiceResponse wrapping all responses from the save operation.</returns>
        Task<IODataSaveResult> SaveChangesAsync();

        /// <summary>
        /// Save changes to the target system and reset the client to the original state.
        /// </summary>
        /// <returns>DataServiceResponse wrapping all responses from the save operation.</returns>
        Task<IODataSaveResult> SaveChangesAsync(SaveOptions saveOptions);

        /// <summary>
        /// Saves changes to the target system.
        /// </summary>
        /// <param name="saveOptions">Options for how to save. See <see cref="SaveOptions"/></param>
        /// <returns>Dictionary where saved entity is the key and the value is the Identity URI of the entity. ID can be parsed from the URI.</returns>
        Task<IODataSaveResult> SaveChangesWithEntityResultsAsync(SaveOptions saveOptions);
    }
}
