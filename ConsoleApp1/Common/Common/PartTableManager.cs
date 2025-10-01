using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PartTableManager
    {
        private readonly static Dictionary<Guid, PartBackup> _partDict = [];
        private readonly static ConcurrentDictionary<Guid, PartMapper> _partMapperDict = new();

        public static IReadOnlyList<(string partName, string desc)> GetPartDescriptions()
        {
            return PartDescriptions.GetPartDescriptions();
        }

        static PartTableManager()
        {
            LoadTable();
        }

        public static void LoadTable()
        {
            
        }

        public static void SaveTable() 
        {

        }


        public static void Shutdown()
        {
            if (!_partMapperDict.IsEmpty)
            {
                foreach (var partMapper in _partMapperDict.Values)
                {
                    partMapper.UnInitialize();
                }
            }
        }


        public static bool RegisterPart(string partName, out PartMapper? partMapper)
        {
            partMapper = null;
            var partBackup = PartDescriptions.GetPartBackup(partName);
            if (partBackup != null)
            {
                _partDict[partBackup.PartId] = partBackup;
                partMapper = new PartMapper(partBackup);
                _partMapperDict[partMapper.PartId] = partMapper;
                OnPartCollectionChanged?.Invoke();
                return true;
            }
            return false;
        }

        public static event Action? OnPartCollectionChanged;


        public static bool RemovePart(PartMapper partMapper)
        {
            if (_partDict.ContainsKey(partMapper.PartId))
            {
                return _partMapperDict.Remove(partMapper.PartId, out _) && _partDict.Remove(partMapper.PartId);
            }
            return false;
        }


        public static IEnumerable<PartMapper> GetPartMappers()
        {
            return _partMapperDict.Values.AsEnumerable();
        }
    }
}
