namespace Assets.Scripts.Craiel.GDX.AI.Sharp.Contracts
{
    using Utils;

    public interface IYamlSerializable
    {
        void Serialize(YamlFluentSerializer serializer);
        void Deserialize(YamlFluentDeserializer deserializer);
    }
}
