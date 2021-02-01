using System.IO;

namespace FileFS.Api.Serializers.Abstractions
{
    public interface ISerializer<TModel>
    {
        TModel ReadFrom(Stream stream);

        void WriteTo(Stream stream, TModel model);
    }
}