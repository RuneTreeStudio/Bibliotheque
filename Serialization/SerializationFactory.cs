using System;

namespace Serialization
{
    public enum SerializationType
    {
        Xml,
        Binary // La serialisation binaire pourrait etre possible aussi (plus complique peut-etre)
    }

    public class SerializationFactory
    {
        public static ISerializationStrategy CreateSerializer(SerializationType type)
        {
            return type switch
            {
                SerializationType.Xml => new XmlSerializationStrategy(),
                _ => throw new ArgumentException($"Type de sérialisation non supporté : {type}")
            };
        }
    }
}
