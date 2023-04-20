namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// The interface for basic ODataClient settings.
    /// </summary>
    public interface IODataClientBaseSettings
    {
        /// <summary>
        /// Gets the resource uri without any API paths.
        /// </summary>
        string ResourceUri { get; }

        /// <summary>
        /// Gets the resource uri that contains any API paths.
        /// </summary>
        string ResourceApiUri { get; }
    }
}
