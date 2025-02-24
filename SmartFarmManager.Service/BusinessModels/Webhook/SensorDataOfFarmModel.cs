using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Webhook
{
    public class SensorDataOfFarmModel
    {
        public string FarmCode { get; set; }
        public string Macaddress { get; set; }
        public List<CageModel> Cages { get; set; }
    }

    public class CageModel
    {
        public string PenCode { get; set; }
        public List<NodeModel> Nodes { get; set; }
    }

    public class NodeModel
    {
        public int NodeId { get; set; }
        public List<SensorModel> Sensors { get; set; }
    }

    public class SensorModel
    {
        public int PinCode { get; set; }
        public double Value { get; set; }
        public bool IsWarning { get; set; }
    }
}

