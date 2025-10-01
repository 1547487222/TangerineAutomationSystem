using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleInfoTable : IParameterTable
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.Parse("8c5b7f3e-0a2d-4f6e-9b8c-1a7e2d3f4c5b");

        [JsonIgnore]
        public string Name => "模块信息表";

        public List<ModuleInfoParameter> ModuleInfoParameters { get; set; } = [];
        [JsonIgnore]
        public IParameter[] Parameters => [.. ModuleInfoParameters];

        public void AddParameter(IParameter parameter)
        {
            if (parameter is not ModuleInfoParameter moduleInfoParameter)
                throw new NotImplementedException();
            ModuleInfoParameters.Add(moduleInfoParameter);
        }

        public IParameter? GetParameter(Guid id)
        {
            var parameter = ModuleInfoParameters.FirstOrDefault(x => x.ParameterId == id);
            return parameter;
        }

        public void LoadTable()
        {
            var path = Environment.CurrentDirectory + "\\Tables" + "\\ModuleInfoTable.json";
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Tables"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Tables");
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            var json = File.ReadAllText(path);
            var moduleInfoTable = JsonConvert.DeserializeObject<ModuleInfoTable>(json);
            if (moduleInfoTable != null)
            {
                ModuleInfoParameters = moduleInfoTable.ModuleInfoParameters;
            }
        }

        public void SaveTable()
        {
            var path = Environment.CurrentDirectory + "\\Tables" + "\\ModuleInfoTable.json";
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



