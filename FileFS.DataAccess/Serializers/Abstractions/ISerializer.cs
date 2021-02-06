namespace FileFS.DataAccess.Serializers.Abstractions
{
    public interface ISerializer<TModel>
    {
        TModel FromBuffer(byte[] buffer);

        byte[] ToBuffer(TModel model);
    }
}