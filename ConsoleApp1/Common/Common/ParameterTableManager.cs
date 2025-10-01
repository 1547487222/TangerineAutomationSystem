using QStandaedPlatform.Engine.Laboratory.Documents;

namespace QStandaedPlatform.Engine.Common.Common
{
    /// <summary>
    /// 参数表管理器
    /// </summary>
    public static class ParameterTableManager
    {
        private static readonly Dictionary<Guid, IParameterTable> _parameterTable = [];

        public static ModuleInfoTable ModuleInfoTable { get; } = new ModuleInfoTable();

        public static LabTrayTable LabTrayTable { get; } = new LabTrayTable();

        public static ModuleFuncCodeTable ModuleFuncCodeTable { get; } = new ModuleFuncCodeTable();

        public static ModuleChannelGroupTable ModuleChannelGroupTable { get; } = new ModuleChannelGroupTable();

        private static readonly object _lock = new();

        public static void InitParameterTable()
        {
            AddTable(ModuleInfoTable);
            AddTable(ModuleFuncCodeTable);
            AddTable(LabTrayTable);
            AddTable(ModuleChannelGroupTable);
            LoadTable();
        }

        public static T Table<T>() where T : IParameterTable
        {
            var table = _parameterTable.Values.FirstOrDefault(x => x is T) ?? throw new Exception("Table not found");
            return (T)table;
        }

        public static IParameterTable Table(Guid id)
        {
            lock (_lock)
            {
                if (_parameterTable.TryGetValue(id, out var table))
                {
                    return table;
                }
                throw new Exception("Table not found");
            }
        }

        public static void AddTable(IParameterTable table)
        {
            lock (_lock)
            {
                _parameterTable.Add(table.Id, table);
            }
        }

        public static void AddParameter(Guid tableId, IParameter parameter)
        {
            lock (_lock)
            {
                _parameterTable[tableId].AddParameter(parameter);
            }
        }

        public static IParameter? GetParameter(Guid tableId, Guid id)
        {
            lock (_lock)
            {
                return _parameterTable[tableId].GetParameter(id);
            }
        }

        public static void SaveTable()
        {
            lock (_lock)
            {
                foreach (var table in _parameterTable.Values)
                {
                    table.SaveTable();
                }
            }
            foreach (var table in _parameterTable.Values)
            {
                foreach (var parameter in table.Parameters)
                {
                    parameter.InitlizeParameter();
                }
            }
        }

        public static void LoadTable()
        {
            lock (_lock)
            {
                foreach (var table in _parameterTable.Values)
                {
                    table.LoadTable();
                }
            }
            foreach (var table in _parameterTable.Values)
            {
                foreach (var parameter in table.Parameters)
                {
                    parameter.InitlizeParameter();
                }
            }
        }
    }


    public interface IParameterTable
    {
         Guid Id { get; }

         string Name { get; }

        void AddParameter(IParameter parameter);

        IParameter? GetParameter(Guid id);
        void SaveTable();
        void LoadTable();
        IParameter[] Parameters { get; }
    }



}
