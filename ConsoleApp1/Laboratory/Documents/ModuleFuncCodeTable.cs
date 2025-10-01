using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleFuncCodeTable : IParameterTable
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.Parse("f9e2a7c6-3b5d-4c8e-a1b2-c3d4e5f67890");
        [JsonIgnore]
        public string Name => "模块功能码表";

        public List<ModuleFuncCodeParameter> ModuleFuncCodeParameters { get; set; } = [];
        [JsonIgnore]
        public IParameter[] Parameters => [.. ModuleFuncCodeParameters];

        public void AddParameter(IParameter parameter)
        {
            if (parameter is not ModuleFuncCodeParameter moduleFuncCodeParameter)
                throw new NotImplementedException();
            ModuleFuncCodeParameters.Add(moduleFuncCodeParameter);
        }

        public IParameter? GetParameter(Guid id)
        {
            var parameter = ModuleFuncCodeParameters.FirstOrDefault(x => x.ParameterId == id);
            if (parameter == null)
                return null;
            parameter.ModuleInfoParameter = ParameterTableManager.ModuleInfoTable.ModuleInfoParameters.FirstOrDefault(p => p.ModuleInfoId == parameter.ModuleInfoId);
            return parameter;
        }

        public void LoadTable()
        {
            var path = Environment.CurrentDirectory + "\\Tables" + "\\ModuleFuncCodeTable.json";

            if (!Directory.Exists(Environment.CurrentDirectory + "\\Tables"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Tables");
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            var json = File.ReadAllText(path);
            var moduleFuncCodeTable = JsonConvert.DeserializeObject<ModuleFuncCodeTable>(json);
            if (moduleFuncCodeTable != null)
            {
                ModuleFuncCodeParameters = moduleFuncCodeTable.ModuleFuncCodeParameters;
            }
        }

        public void SaveTable()
        {
            var path = Environment.CurrentDirectory + "\\Tables" + "\\ModuleFuncCodeTable.json";
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



