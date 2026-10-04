using ShapezShifter.Hijack;

namespace BetterProgression
{
    public interface ISerializedGameScenarioRewirer : IRewirer
    {
        SerializedGameScenario ModifySerializedGameScenario(SerializedGameScenario scenario);
    }
}
