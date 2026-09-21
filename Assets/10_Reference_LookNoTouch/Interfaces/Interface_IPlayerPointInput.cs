public interface IPlayerPointInput
{
    bool Pressed { get; }
    bool IsPressing { get; }
    bool Released { get; }
}