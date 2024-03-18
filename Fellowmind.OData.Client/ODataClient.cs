using Microsoft.OData.Client;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fellowmind.OData.Client.Core;
using Microsoft.OData.Extensions.Client;

[assembly: InternalsVisibleTo("Fellowmind.OData.Tests")]
namespace Fellowmind.OData.Client
{
    /// <summary>
    /// Helper class to use Microsoft OData client with Dynamics CE and F&amp;O entities.
    /// This class was developed to help avoiding numerous issues when not using the client in a specific way.
    /// This class also implements IDisposable and so allows working inside a 'using'-block.
    /// This class allows you to work with multiple data contexts and move entities between those.
    /// !!!Doing queries with 'Expand' will cause major problems when moving entities between contexts and is blocked.!!!
    /// </summary>
    /// <typeparam name="TClient">DataServiceContext type used to communicate with Dynamisc CE or FO.</typeparam>
    public abstract class ODataClient<TClient> : IODataClient<TClient> where TClient : DataServiceContext
    {
        private const int MaxUrlLength = 2048;

        private readonly string _clientName;
        private readonly IODataClientBaseSettings _clientSettings;
        private readonly IODataClientFactory _clientFactory;
        private readonly IList<object> _collections = new List<object>();

        private static readonly HashSet<string> _allowedMethods = new HashSet<string> { "GET", "DELETE", "PATCH", "PUT" };
        private static readonly Regex _entityIdRegex = new Regex(@"\((.{36})\)", RegexOptions.Compiled);

        /// <summary>
        /// Defines target company to use for Dynamics Finance Operations GET and DELETE requests.
        /// </summary>
        private string _targetCompany = null;

        /// <inheritdoc />
        public event EventHandler<ContextCreatedEventArgs<TClient>> ContextCreated;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClient{TClient}"/> class.
        /// </summary>
        /// <param name="clientFactory">The odataclientfactory.</param>
        /// <param name="clientSettings">The client settings.</param>
        /// <param name="clientName">The name of the ODataClient and HttpClient. Used when you have multiple clients that use different HttpClients.</param>
        protected ODataClient(IODataClientFactory clientFactory, IODataClientBaseSettings clientSettings, string clientName = "ODataClient")
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _clientSettings = clientSettings ?? throw new ArgumentNullException(nameof(clientSettings));

            if (string.IsNullOrWhiteSpace(clientName))
            {
                throw new ArgumentNullException(nameof(clientName), "The client name cannot be null or empty!");
            }

            _clientName = clientName;

            RefreshDataContext();
        }

        /// <summary>
        /// OData context used to communicate with target system. Only make queries through this context! For other operations, use the methods of this class.
        /// </summary>
        protected TClient DataContext { get; private set; }

        /// <summary>
        /// Sets target company for Dynamics Finance &amp; Operations GET requests.
        /// </summary>
        /// <remarks>By default, Dynamics F&amp;O OData requests return data only for the company set to the integration user.</remarks>
        /// <param name="company">Null will disable the feature. Empty string will return data for all companies. Giving a company (DataAreaId) will target queries to data of that company.</param>
        [Obsolete("Setting this value doesn't work in all cases (e.g. $select). Use WithTargetCompany extension method instead.")]
        public void SetDynamicsOperationsTargetCompany(string company)
        {
            _targetCompany = company;
        }

        /// <summary>
        /// Enables tracking for the given entity type. Tracking is also automatically initialized when calling TrackEntity for non-tracked entity type.
        /// </summary>
        /// <typeparam name="T">Entity type for which tracking is set up.</typeparam>
        /// <returns>Data service collection for tracking the given entity type.</returns>
        public DataServiceCollection<T> InitializeTracking<T>() where T : BaseEntityType
        {
            DataServiceCollection<T> dsc = new DataServiceCollection<T>(DataContext);
            _collections.Add(dsc);

            return dsc;
        }

        /// <summary>
        /// Returns true, if tracking has been enabled for the given entity type.
        /// </summary>
        /// <typeparam name="T">Entity type to check.</typeparam>
        /// <returns>True if tracking is enabled.</returns>
        public bool IsTrackingInitialized<T>() where T : BaseEntityType
        {
            return _collections.OfType<DataServiceCollection<T>>().Any();
        }

        /// <summary>
        /// Creates new instance of the specified entity and sets it for change tracking.
        /// </summary>
        /// <typeparam name="T">Type of entity to create.</typeparam>
        /// <returns>New instance of tracked entity.</returns>
        public T CreateAndTrackEntity<T>() where T : BaseEntityType, new()
        {
            T entity = Activator.CreateInstance<T>();
            TrackEntity(entity);
            return entity;
        }

        /// <inheritdoc />
        public async Task<T> GetEntityAsync<T>(Dictionary<string, object> ids) where T : BaseEntityType, new()
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            string entitySetName = typeof(T).GetAttributeValue((EntitySetAttribute esa) => esa.EntitySet);
            string keyString = Serializer.GetKeyString(DataContext, ids);

            return await new DataServiceQuerySingle<T>(DataContext, $"{entitySetName}({keyString})").GetValueAsync();
        }

        /// <inheritdoc />
        public async Task<T> GetEntityAsync<T>(object id) where T : BaseEntityType, new()
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            string entitySetName = typeof(T).GetAttributeValue((EntitySetAttribute esa) => esa.EntitySet);
            string keyString = Serializer.GetKeyString(DataContext, new Dictionary<string, object>() { { "Id", id } });

            return await new DataServiceQuerySingle<T>(DataContext, $"{entitySetName}({keyString})").GetValueAsync();
        }

        /// <inheritdoc />
        public async Task<T> GetEntityAndTrackAsync<T>(object id) where T : BaseEntityType, new()
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            T entity = await GetEntityAsync<T>(id);
            if (entity != null)
            {
                TrackEntity(entity);
            }

            return entity;
        }

        /// <inheritdoc />
        public async Task<T> GetEntityAndTrackAsync<T>(Dictionary<string, object> ids) where T : BaseEntityType, new()
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            T entity = await GetEntityAsync<T>(ids);
            if (entity != null)
            {
                TrackEntity(entity);
            }

            return entity;
        }

        /// <summary>
        /// Creates new instance of the specified entity.
        /// </summary>
        /// <typeparam name="T">Type of entity to create.</typeparam>
        /// <returns>New entity instance.</returns>
        public T CreateEntity<T>() where T : BaseEntityType, new()
        {
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Adds entity to be tracked for changes.
        /// Starting tracking for 'Unchanged' entity creates 'Update' message.
        /// Starting tracking for 'Detached' entity create 'Insert' message.
        /// </summary>
        /// <typeparam name="T">Type of entity to track.</typeparam>
        /// <param name="entity">Entity to track.</param>
        public void TrackEntity<T>(T entity) where T : BaseEntityType
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Make sure tracking collection exist.
            DataServiceCollection<T> collection = _collections.OfType<DataServiceCollection<T>>().FirstOrDefault();
            if (collection == null)
            {
                InitializeTracking<T>();
                collection = _collections.OfType<DataServiceCollection<T>>().FirstOrDefault();
            }

            if (collection.Contains(entity))
            {
                return; // Already tracked.
            }

            // Make sure the entity is in unchanged state.
            EntityDescriptor entityDescriptor = DataContext.EntityTracker.TryGetEntityDescriptor(entity);
            if (entityDescriptor != null && entityDescriptor.State != EntityStates.Unchanged)
            {
                throw new InvalidOperationException("Only unchanged items may be added for tracking. Otherwise the operation sent to the server may be wrong one (insert instead of update).");
            }

            IBaseEntityType baseEntityType = entity;
            object trackingContext =  baseEntityType.Context;

            if (trackingContext != null && trackingContext != DataContext)
            {
                throw new InvalidOperationException("You are trying to add an entity which is already tracked by another context. Please query the item again through this context or use AttachEntity to add an entity without tracking enabled.");
            }

            collection.Add(entity);
        }

        /// <summary>
        /// Removes entity from being tracked for changes. Does not generate delete operation.
        /// </summary>
        /// <typeparam name="T">Type of entity to untrack.</typeparam>
        /// <param name="entity">Entity to untrack.</param>
        public void UntrackEntity<T>(T entity) where T : BaseEntityType
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            DataServiceCollection<T> collection = _collections.OfType<DataServiceCollection<T>>().FirstOrDefault();
            if (collection == null)
            {
                throw new InvalidOperationException("Cannot untrack entity type which does not have a tracking collection.");
            }

            collection.Remove(entity);
        }

        /// <summary>
        /// This creates delete operation for the given entity. To stop tracking of entity use UntrackEntity. To detach an entity use DetachEntity.
        /// </summary>
        /// <typeparam name="T">Type of entity to delete.</typeparam>
        /// <param name="entity">Entity to delete.</param>
        public void DeleteEntity<T>(T entity) where T : BaseEntityType
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // Delete doesn't necessarily need tracking.
            if (IsTrackingInitialized<T>())
            {
                UntrackEntity(entity); // Disable further tracking.
            }

            DataContext.DeleteObject(entity); // To delete, this is needed.
        }

        /// <summary>
        /// Attaches entity to the data context. If an entity is already tracked by another context, that tracking will be removed.
        /// </summary>
        /// <typeparam name="T">Entity type to attach.</typeparam>
        /// <param name="entity">Entity to attach.</param>
        public void AttachEntityAndTrack<T>(T entity) where T : BaseEntityType
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            AttachEntity(entity);
            TrackEntity(entity);
        }

        /// <summary>
        /// Attaches entity to the data context. If an entity is already tracked by another context, that tracking will be removed.
        /// </summary>
        /// <typeparam name="T">Entity type to attach.</typeparam>
        /// <param name="entity">Entity to attach.</param>
        public void AttachEntity<T>(T entity) where T : BaseEntityType
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DataServiceCollection<T> collection = _collections.OfType<DataServiceCollection<T>>().FirstOrDefault();
            if (collection != null)
            {
                throw new InvalidOperationException("You are trying to attach entity type which has tracking enabled. Use TrackEntity instead. If you want to get an existing item tracked, you must query it from the data source.");
            }

            string entitySetName = entity.GetType().GetAttributeValue((EntitySetAttribute esa) => esa.EntitySet);
            DataContext.AttachTo(entitySetName, entity);

            // Make sure the entity is detached from possible previous context.
            IBaseEntityType baseEntityType = entity;
            DataServiceContext trackingContext = baseEntityType.Context;

            if (trackingContext != null && trackingContext != DataContext)
            {
                baseEntityType.Context = DataContext;
                
                // Detect if expand was used in other context, because that will screw up this context and cannot be repaired.
                bool expandWasUsed = false;
                foreach (var property in entity.GetType().GetProperties())
                {
                    if (property.GetValue(entity) != null && IsEnumerableBaseEntityType(property.GetValue(entity).GetType()))
                    {
                        if (((IEnumerable<BaseEntityType>)property.GetValue(entity)).Any())
                        {
                            expandWasUsed = true;
                            break;
                        }
                    }
                }

                if (expandWasUsed)
                {
                    throw new InvalidOperationException($"Expand was used for entity of type '{entity.GetType().Name}' which is not supported. Attaching such an entity will screw up the tracking context. Get the referenced entities separately.");
                }
            }
        }

        /// <summary>
        /// Detaches given entity from data context.
        /// </summary>
        /// <typeparam name="T">Entity type to detach.</typeparam>
        /// <param name="entity">Entity to detach.</param>
        public void DetachEntity<T>(T entity) where T : BaseEntityType
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            DataContext.Detach(entity);
        }

        /// <summary>
        /// Save changes to the target system with specified options.
        /// </summary>
        /// <param name="saveOptions">Options for how to save. See <see cref="SaveOptions"/></param>
        /// <returns>A <see cref="IODataSaveResult"/> wrapping all responses from the save operation.</returns>
        public async Task<IODataSaveResult> SaveChangesAsync(SaveOptions saveOptions)
        {
            DataServiceResponse response = null;

            if (DataContext.Entities.Any(x => x.State != EntityStates.Unchanged))
            {
                if (saveOptions.HasFlag(SaveOptions.PostOnlySetProperties) && saveOptions.HasFlag(SaveOptions.BatchWithSingleChangeset))
                {
                    response = await DataContext.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset | SaveChangesOptions.PostOnlySetProperties).ConfigureAwait(false);
                }
                else if (saveOptions.HasFlag(SaveOptions.BatchWithSingleChangeset))
                {
                    response = await DataContext.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset).ConfigureAwait(false);
                }
                else if (saveOptions.HasFlag(SaveOptions.PostOnlySetProperties))
                {
                    response = await DataContext.SaveChangesAsync(SaveChangesOptions.PostOnlySetProperties).ConfigureAwait(false);
                }
                else
                {
                    response = await DataContext.SaveChangesAsync().ConfigureAwait(false); // SaveOptions.None falls here.
                }
            }

            if (!saveOptions.HasFlag(SaveOptions.SkipContextReCreationAfterSave))
            {
                UntrackAll();
                RefreshDataContext();
            }

            return new ODataSaveResult(true, response);
        }

        /// <summary>
        /// Save changes to the target system and reset the client to the original state.
        /// </summary>
        /// <remarks>Uses <see cref="SaveOptions"/> 'BatchWithSingleChangeset' and 'PostOnlySetProperties'.</remarks>
        /// <returns>A <see cref="IODataSaveResult"/> wrapping all responses from the save operation.</returns>
        public async Task<IODataSaveResult> SaveChangesAsync()
        {
            return await SaveChangesAsync(SaveOptions.BatchWithSingleChangeset | SaveOptions.PostOnlySetProperties).ConfigureAwait(false);
        }

        /// <summary>
        /// Save changes to the target system with specified options.
        /// </summary>
        /// <param name="saveOptions">Options for how to save. See <see cref="SaveOptions"/></param>
        /// <returns>A <see cref="IODataSaveResult"/> wrapping all responses from the save operation.</returns>
        public async Task<IODataSaveResult> SaveChangesWithEntityResultsAsync(SaveOptions saveOptions)
        {
            IODataSaveResult responses = await SaveChangesAsync(saveOptions).ConfigureAwait(false);

            return new ODataSaveResult(responses.Success, responses.Response, responses.Response?.Select(x => (x as ChangeOperationResponse).Descriptor as EntityDescriptor)
                .Where(descriptor => descriptor != null)
                .ToDictionary(descriptor => descriptor.Entity as BaseEntityType, descriptor => descriptor.Identity));
        }

        /// <summary>
        /// Gets the entity from tracking context by Id.
        /// If not found, then returns null.
        /// </summary>
        /// <typeparam name="T">The type of the entity to find.</typeparam>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns></returns>
        public T GetEntityReference<T>(Guid entityId) where T : BaseEntityType
        {
            Type entityType = typeof(T);

            // Get key property of this entity (eg. for entity of type ecr_route this would be ecr_routeid)
            var keyNames = entityType.GetAttributeValue((KeyAttribute keyattr) => keyattr.KeyNames);
            if (keyNames.Count != 1)
            {
                throw new Exception("Cannot make entity reference since the key of this entity is not formed from one parameter.");
            }

            string keyPropName = entityType.GetAttributeValue((KeyAttribute keyAttr) => keyAttr.KeyNames.First());
            PropertyInfo keyProp = entityType.GetProperty(keyPropName);

            T entity = DataContext.Entities.Where(e => e.Entity is T).FirstOrDefault(e => (Guid)keyProp.GetValue(e.Entity, null) == entityId)?.Entity as T;

            return entity;
        }

        /// <summary>
        /// Gets the entity from tracking context by Id.
        /// If not found, then returns a new instance of the entity that has ID of given entityId and is attached to the DataContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public T GetOrCreateEntityReference<T>(Guid entityId) where T : BaseEntityType
        {
            T entity = GetEntityReference<T>(entityId);
            Type entityType = typeof(T);

            // Entity is not already tracked so we will create new instance of the entity and set its entity id and attach it to the entity tracker
            if (entity == null)
            {
                entity = Activator.CreateInstance(entityType) as T;

                string keyPropName = entityType.GetAttributeValue((KeyAttribute keyAttr) => keyAttr.KeyNames.First());
                PropertyInfo keyProp = entityType.GetProperty(keyPropName);

                keyProp.SetValue(entity, entityId, null);

                AttachEntity(entity);
            }

            return entity;
        }

        /// <summary>
        /// Returns the entity id from DataContext.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public Guid? GetEntityIdFromTrackingContext(BaseEntityType entity)
        {
            var trackedEntity = DataContext.Entities.FirstOrDefault(e => e.Entity == entity);

            if (trackedEntity == null)
            {
                return null;
            }

            string entityIdentityLocalPath = trackedEntity.Identity.LocalPath;
            Match matchResult = _entityIdRegex.Match(entityIdentityLocalPath);

            if (matchResult.Success)
            {
                Guid id = Guid.Parse(matchResult.Groups[1].Value);
                return id;
            }

            return null;
        }

        /// <summary>
        /// Gets all the pages for the given querty.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns></returns>
        protected async IAsyncEnumerable<IEnumerable<T>> GetPagesAsync<T>(IQueryable<T> query) where T : BaseEntityType
        {
            // DataServiceQueryContinuation<T> contains the next link
            DataServiceQueryContinuation<T> nextLink = null;

            // Get the first page
            QueryOperationResponse<T> response = await query.ExecuteAsync<T>() as QueryOperationResponse<T>;

            do
            {
                if (nextLink != null)
                {
                    response = await DataContext.ExecuteAsync<T>(nextLink) as QueryOperationResponse<T>;
                }

                if (response == null)
                {
                    break;
                }

                yield return response.ToList();
            }

            // Loop if there is a next link
            while ((nextLink = response.GetContinuation()) != null);
        }

        /// <summary>
        /// Clears tracking collection for the given type.
        /// </summary>
        /// <typeparam name="T">Entity type to untrack.</typeparam>
        protected void UntrackAll<T>() where T : BaseEntityType
        {
            DataServiceCollection<T> collection = _collections.OfType<DataServiceCollection<T>>().FirstOrDefault();

            if (collection == null)
            {
                throw new InvalidOperationException("Unknown collection provided. Please call CreateTrackableCollection to create the collection.");
            }

            collection.Clear(true);
        }

        /// <summary>
        /// Clears all tracking collections and removes items from entity and link collections.
        /// </summary>
        protected void UntrackAll()
        {
            foreach (object collection in _collections)
            {
                collection.GetType().GetMethod("Clear", new Type[] { typeof(bool) }).Invoke(collection, new object[] { true });
            }

            foreach (var link in DataContext.Links)
            {
                DataContext.DetachLink(link.Source, link.SourceProperty, link.Target);
            }

            foreach (var entity in DataContext.Entities)
            {
                DataContext.Detach(entity.Entity);
            }
        }
        
        /// <summary>
        /// Checks if the given type can be casted to IEnumerable{BaseEntityType}.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if the given type can be casted to IEnumerable{BaseEntityType}.</returns>
        private bool IsEnumerableBaseEntityType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            int count = type.GetInterfaces().Count(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>) && typeof(BaseEntityType).IsAssignableFrom(x.GetGenericArguments()[0]));
            return count > 0;
        }

        /// <summary>
        /// Sets target company information to all GET requests.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataContext_BuildingRequest(object sender, BuildingRequestEventArgs e)
        {
            if (e.Headers.TryGetValue("If-Match", out string val))
            {
                e.Headers.Remove("If-Match");
            }

            // If target company is null, nothing is done (default operation).
            if (_targetCompany != null)
            {
                // Only modify some type of requests (for example not POST).
                if (_allowedMethods.Contains(e.Method.ToUpperInvariant()))
                {
                    // Set query parameters to the request.
                    Uri uri = e.RequestUri;
                    if (_targetCompany != string.Empty)
                    {
                        uri = AddFilterParameterToUri(e.RequestUri, "dataAreaId eq '" + _targetCompany + "'");
                    }
                    
                    e.RequestUri = AddParameterToUri(uri, "&cross-company=true");
                }
            }

            if (e.RequestUri.AbsoluteUri.Length > MaxUrlLength)
            {
                throw new InvalidOperationException($"The generated URL length '{e.RequestUri.AbsoluteUri.Length}' is over the maximum of '{MaxUrlLength}'.");
            }
        }

        /// <summary>
        /// Adds given parameter to Uri while trying to maintain correctness.
        /// </summary>
        /// <param name="url">Uri to modify.</param>
        /// <param name="parameter">Parameter to add to the Uri.</param>
        /// <returns>Resulting Uri after modification.</returns>
        private Uri AddFilterParameterToUri(Uri url, string parameter)
        {
            string uri = url.ToString();

            // Make sure we are not adding a duplicate.
            if (uri.Contains(parameter))
            {
                return url;
            }

            // Make sure '?' exists in the Uri.
            if (!uri.Contains("?"))
            {
                uri += "?";
            }

            // If filter is already there, we need to expand it.
            if (uri.Contains("$filter="))
            {
                uri += (" and " + parameter);
            }
            else
            {
                uri += ("$filter=" + parameter);
            }

            return new Uri(uri);
        }

        /// <summary>
        /// Adds given parameter to Uri while trying to maintain correctness.
        /// </summary>
        /// <param name="url">Uri to modify.</param>
        /// <param name="parameter">Parameter to add to the Uri.</param>
        /// <returns>Resulting Uri after modification.</returns>
        private Uri AddParameterToUri(Uri url, string parameter)
        {
            string uri = url.ToString();

            // Make sure we are not adding a duplicate.
            if (uri.Contains(parameter))
            {
                return url;
            }

            // Make sure '?' exists in the Uri.
            if (!uri.Contains("?"))
            {
                uri += "?";
            }

            // If filter is already there, we need to expand it.
            uri += parameter;

            return new Uri(uri);
        }

        /// <summary>
        /// Refreshes the data context and tracking collections to their original states.
        /// </summary>
        private void RefreshDataContext()
        {
            if (DataContext != null)
            {
                DataContext.BuildingRequest -= DataContext_BuildingRequest;
            }

            DataContext = _clientFactory.CreateClient<TClient>(new Uri(_clientSettings.ResourceApiUri), _clientName);
            DataContext.IgnoreResourceNotFoundException = true;
            
            // Make sure there won't be multiple handlers.
            DataContext.BuildingRequest += DataContext_BuildingRequest;

            // Clear all change tracking. New collections are created automatically when needed when 
            // starting to track new entities.
            _collections.Clear();

            ContextCreated?.Invoke(this, new ContextCreatedEventArgs<TClient>(DataContext));
        }

        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    UntrackAll();

                    if (DataContext != null)
                    {
                        DataContext.BuildingRequest -= DataContext_BuildingRequest;
                    }

                    DataContext = null;

                    _collections.Clear();
                }

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}
