using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Text;
using TrainsLib.Rewirers;

namespace TrainsLib.GameData
{
    /// <summary>
    /// Handles the attaining and managing of game data related to trains.
    /// </summary>
    internal class TrainsDataHandler : IDisposable
    {
        public static TrainsDataHandler? Instance;

        // hooks & rewirers
        private readonly List<RewirerHandle> _rewirerHandles = new();

        private TrainsDataHandler()
        {
            Instance ??= this;
        }

        public static void Initialize()
        {
            // create an instance, if one doesn't already exist
            if (Instance != null)
            {
                throw new InvalidOperationException($"Cannot initialize an already initialized {nameof(TrainsDataHandler)}");
            }
            Instance = new();

            Instance.CreateHooks();// create hooks
            
        }

        private void RegisterRewirers()
        {
            TrainsLibLogger.LogInfo("Registering rewiwers...");

            _rewirerHandles.Add(GameRewirers.AddRewirer(new TrainsSimulationSystemsRewirer()));
        }

        private void CreateHooks()
        {
            TrainsLibLogger.LogInfo("Creating hooks...");
        }

        public void Dispose()
        {
            // dispose all rewirers
            _rewirerHandles.ForEach(handle => GameRewirers.RemoveRewirer(handle));
            _rewirerHandles.Clear();

            // dispose all hooks

            // dispose this instance
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
