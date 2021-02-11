using System.IO;
using FileFS.DataAccess.Abstractions;
using Moq;

namespace FileFS.Tests.Shared.Factories
{
    /// <summary>
    /// Helper factory that creates mocked <see cref="IStorageStreamProvider"/> implementation that uses memory stream as a provided stream.
    /// </summary>
    public static class StorageStreamProviderMockFactory
    {
        /// <summary>
        /// Creates mocked <see cref="IStorageStreamProvider"/> implementation that uses memory stream as a provided stream.
        /// </summary>
        /// <param name="storageBuffer">Fixed buffer that will be used with stream.</param>
        /// <returns>Mocked <see cref="IStorageStreamProvider"/> implementation that uses memory stream as a provided stream.</returns>
        public static IStorageStreamProvider Create(byte[] storageBuffer)
        {
            var storageStreamProviderMock = new Mock<IStorageStreamProvider>();
            storageStreamProviderMock
                .Setup(provider => provider.OpenStream(true))
                .Returns(() => new MemoryStream(storageBuffer));

            storageStreamProviderMock
                .Setup(provider => provider.OpenStream(false))
                .Returns(() => new MemoryStream(storageBuffer));

            return storageStreamProviderMock.Object;
        }

        /// <summary>
        /// Creates mocked <see cref="IStorageStreamProvider"/> implementation that uses memory stream as a provided stream.
        /// </summary>
        /// <param name="expandableStream">Expandable stream that will be returned by OpenStream method..</param>
        /// <returns>Mocked <see cref="IStorageStreamProvider"/> implementation that uses memory stream as a provided stream.</returns>
        public static IStorageStreamProvider Create(MemoryStream expandableStream)
        {
            var storageStreamProviderMock = new Mock<IStorageStreamProvider>();
            storageStreamProviderMock
                .Setup(provider => provider.OpenStream(true))
                .Returns(expandableStream);

            storageStreamProviderMock
                .Setup(provider => provider.OpenStream(false))
                .Returns(expandableStream);

            return storageStreamProviderMock.Object;
        }
    }
}