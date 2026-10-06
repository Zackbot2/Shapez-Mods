using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioTools
{
    public interface ISerializedGameScenarioRewirer : IRewirer
    {
        SerializedGameScenario ModifySerializedGameScenario(SerializedGameScenario scenario);
    }
}
