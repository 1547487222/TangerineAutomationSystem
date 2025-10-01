using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public partial class PubMqttSubject: ObservableObject
    {
        public PubMqttSubject(string pubSubjectName,string pubSubjectUrl)
        {
            this.PubSubjectName = pubSubjectName;
            this.PubSubjectUrl = pubSubjectUrl;
        }
        public string PubSubjectName { get; set; }


        public string PubSubjectUrl { get; set; }

        [ObservableProperty]
        private object paramter;

        public object Self => this;
    }
}
