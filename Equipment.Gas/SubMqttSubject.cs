using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public partial class SubMqttSubject : ObservableObject
    {

        public SubMqttSubject()
        {

        }

        public SubMqttSubject(string subjectName, string subjectUrl, bool isenable = false)
        {
            this.SubjectName = subjectName;
            this.SubjectUrl = subjectUrl;
            this.IsEnable = isenable;
        }
        [ObservableProperty]
        private bool isEnable = false;

        [ObservableProperty]
        private string subjectName = string.Empty;

        [ObservableProperty]
        private string subjectUrl= string.Empty;
    }
}
