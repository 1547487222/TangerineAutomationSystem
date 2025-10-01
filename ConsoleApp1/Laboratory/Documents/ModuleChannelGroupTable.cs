using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleChannelGroupTable : IParameterTable
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.Parse("6d1e2f3a-421c-4e7e-8f9a-bcde12345678");
        [JsonIgnore]
        public string Name => "模块通道表";

        public List<ModuleChannelGroup> ModuleChannels { get; set; } = [];
        [JsonIgnore]
        public IParameter[] Parameters => [.. ModuleChannels];

        public void AddParameter(IParameter parameter)
        {
            if (parameter is not ModuleChannelGroup moduleChannelGroup)
                throw new NotImplementedException();
            ModuleChannels.Add(moduleChannelGroup);
        }

        public IParameter? GetParameter(Guid id)
        {
            var parameter = ModuleChannels.FirstOrDefault(x => x.ParameterId == id);
            if (parameter == null)
                return null;
            parameter.ModuleInfoParameter = ParameterTableManager.ModuleInfoTable.ModuleInfoParameters.FirstOrDefault(p => p.ModuleInfoId == parameter.ModuleInfoId);
            return parameter;
        }

        public void LoadTable()
        {
            var path = Environment.CurrentDirectory + "\\Tables" + "\\ModuleChannelGroupTable.json";
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Tables"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Tables");
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            var json = File.ReadAllText(path);
            var moduleChannelGroupTable = JsonConvert.DeserializeObject<ModuleChannelGroupTable>(json);
            if (moduleChannelGroupTable != null)
            {
                ModuleChannels = moduleChannelGroupTable.ModuleChannels;
            }
        }

        public void SaveTable()
        {
            var path = Environment.CurrentDirectory + "\\Tables" + "\\ModuleChannelGroupTable.json";
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Tables"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Tables");
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, json);
        }
    }

}



