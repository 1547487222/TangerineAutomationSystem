using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class LabTrayTable : IParameterTable
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.Parse("6d1e2f3a-4b5c-6d7e-8f9a-bcde12345678");
        [JsonIgnore]
        public string Name => "托盘表";

        public List<LabTray> LabTrayParameters { get; set; } = [];
        [JsonIgnore]
        public IParameter[] Parameters => [.. LabTrayParameters];

        public void AddParameter(IParameter parameter)
        {
            if (parameter is not LabTray tubeRackParameter)
                throw new NotImplementedException();
            LabTrayParameters.Add(tubeRackParameter);
        }

        public IParameter? GetParameter(Guid id)
        {
            var parameter = LabTrayParameters.FirstOrDefault(x => x.ParameterId == id);
            return parameter;
        }

        public void LoadTable()
        {
            var path = Environment.CurrentDirectory + "\\Tables" + "\\LabTrayTable.json";
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Tables"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Tables");
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            var json = File.ReadAllText(path);
            var tubeRackTable = JsonConvert.DeserializeObject<LabTrayTable>(json);
            if (tubeRackTable != null)
            {
                LabTrayParameters = tubeRackTable.LabTrayParameters;
            }
        }

        public void SaveTable()
        {
            var path = Environment.CurrentDirectory + "\\Tables" + "\\LabTrayTable.json";
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



