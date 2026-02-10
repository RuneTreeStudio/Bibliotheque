using System;

namespace Serialization
{
    public interface ISerializationStrategy
    {
        void Serialize<T>(T data, string filePath);
        T Deserialize<T>(string filePath);
    }
}
