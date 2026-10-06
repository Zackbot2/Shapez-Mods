using Core.Logging;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ScenarioTools
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class is super similar to <see cref="GameScenarioInterceptor"/>, for the sole reason that I wanted it to be as close to the original implementation as possible.
    /// Not trying to steal any code, it would actually be easier if I just did it myself!
    /// </remarks>
    internal class SerializedGameScenarioInterceptor : IDisposable
    {
        private readonly IRewirerProvider RewirerProvider;
        private readonly ILogger Logger;
        private readonly ILHook IlHook;

        public SerializedGameScenarioInterceptor(IRewirerProvider rewirerProvider, ILogger logger)
        {
            RewirerProvider = rewirerProvider;
            Logger = logger;

            MethodInfo target = typeof(GameMode).GetMethod("From", BindingFlags.Static | BindingFlags.Public);
            MethodInfo getRawScenario = typeof(IGameData).GetMethod("GetRawScenario", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            IlHook = new ILHook(
                source: target!,
                manip: context =>
                {
                    ILCursor cursor = new(context);
                    cursor.GotoNext(MoveType.After, instruction => instruction.MatchCallvirt<IGameData>("GetRawScenario"));
                    cursor.EmitDelegate<Func<SerializedGameScenario, SerializedGameScenario>>(Postfix);
                });
        }

        public void Dispose()
        {
            IlHook.Dispose();
        }

        private SerializedGameScenario Postfix(SerializedGameScenario scenario)
        {
            foreach (ISerializedGameScenarioRewirer rewirer in RewirerProvider.RewirersOfType<ISerializedGameScenarioRewirer>())
            {
                scenario = rewirer.ModifySerializedGameScenario(scenario);
            }

            return scenario;
        }
    }
}
