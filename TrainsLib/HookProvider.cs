using Core.Disposing;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Text;
using TrainsLib.Rewirers;

namespace TrainsLib
{
    /// <summary>
    /// Handles hooks and rewirers for <see cref="TrainsLib"/>.
    /// </summary>
    internal class TrainsLibHookProvider : IDisposable
    {
        public static TrainsLibHookProvider? Instance;

        // hooks & rewirers
        private readonly List<Hook> _hooks = new();
        private readonly List<RewirerHandle> _rewirerHandles = new();

        private TrainsLibHookProvider()
        {
            Instance ??= this;
        }

        public void Dispose()
        {
            // dispose all hooks
            _hooks.Clear();

            // dispose all rewirers
            _rewirerHandles.ForEach(handle => GameRewirers.RemoveRewirer(handle));
            _rewirerHandles.Clear();

            // dispose this instance
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void Initialize()
        {
            // create an instance, if one doesn't already exist
            if (Instance != null)
            {
                throw new InvalidOperationException($"Cannot initialize an already initialized {nameof(TrainsLibHookProvider)}.");
            }
            Instance = new();

            Instance.CreateHooks();
            Instance.RegisterRewirers();
        }

        private void CreateHooks()
        {
            TrainsLibLogger.LogInfo("Creating hooks...");
        }

        private void RegisterRewirers()
        {
            TrainsLibLogger.LogInfo("Registering rewiwers...");

            _rewirerHandles.Add(GameRewirers.AddRewirer(new TrainsSimulationSystemsRewirer()));
            _rewirerHandles.Add(GameRewirers.AddRewirer(new GameIslandsProvider()));
        }
    }
}
