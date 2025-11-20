namespace ProjetoTccBackend.Swagger.Interfaces
{
    /// <summary>
    /// Interface for providing example instances of a type for Swagger documentation.
    /// </summary>
    /// <typeparam name="T">The type for which to provide an example.</typeparam>
    public interface ISwaggerExampleProvider<T>
    {
        /// <summary>
        /// Gets an example instance of type T.
        /// </summary>
        /// <returns>An example instance of type T.</returns>
        T GetExample();
    }
}
