using Game.Core.Rails;
using ShapezShifter.Hijack;
using UnityEngine;

namespace MoreTrainColours
{
    public class MoreTrainColoursScenarioRewirer : IGameScenarioRewirer
    {
        public GameScenario ModifyGameScenario(GameScenario gameScenario)
        {
            MoreTrainColoursMod.Logger.Info?.Log($"Registering black rail color for scenario {gameScenario.UniqueId}...");
            SerializableRailColor blackSerializableRailColor = new()
            {
                Id = new SerializableRailColorId("Black"),
                Tint = Color.black
            };

            MoreTrainColoursMod.Logger.Info?.Log("Attempting to register...");
            if (!gameScenario.RailColorRegistry.TryRegisterColor(blackSerializableRailColor.Id, blackSerializableRailColor.Tint, out _))
            {
                MoreTrainColoursMod.Logger.Error?.Log($"Failed to register black rail color with id {blackSerializableRailColor.Id}");
            }
            else
            {
                MoreTrainColoursMod.Logger.Info?.Log($"Successfully registered black rail color for scenario {gameScenario.UniqueId}.");
            }
            return gameScenario;
        }
    }
}
