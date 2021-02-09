namespace FileFS.DataAccess.Serializers.Abstractions
{
    /// <summary>
    /// Abstraction that represents generic serializer.
    /// </summary>
    /// <typeparam name="TModel">Type of the model for which serializer is used.</typeparam>
    public interface ISerializer<TModel>
    {
        /// <summary>
        /// Returns model representation constructed from given byte array.
        /// </summary>
        /// <param name="buffer">Bytes buffer.</param>
        /// <returns>Model representation constructed from given byte array.</returns>
        TModel FromBytes(byte[] buffer);

        /// <summary>
        /// Returns bytes representation of given model instance.
        /// </summary>
        /// <param name="model">Model instance.</param>
        /// <returns>Bytes representation of given model instance.</returns>
        byte[] ToBytes(TModel model);
    }
}