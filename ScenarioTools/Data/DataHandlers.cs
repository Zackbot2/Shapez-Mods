using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioTools.Data
{
    public static class DataHandlers
    {
        public static List<IScenarioDataHandler> Handlers = new();

        public static void AddDataHandler(IScenarioDataHandler dataHandler)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException(nameof(dataHandler));
            }
            ScenarioToolsMod.Logger.Info?.Log($"Adding scenario data handler: {dataHandler.GetType().Name}");
            Handlers.Add(dataHandler);
        }

        public static void RemoveDataHandler(IScenarioDataHandler dataHandler)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException(nameof(dataHandler));
            }
            ScenarioToolsMod.Logger.Info?.Log($"Removing scenario data handler: {dataHandler.GetType().Name}");
            Handlers.Remove(dataHandler);
        }
    }
}
