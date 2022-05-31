using Prism.Mvvm;
using System.ServiceProcess;

namespace ServicesApp.Models
{
    public class ServiceModel : BindableBase
    {
        public string Account { get; set; }
        public string DisplayName { get; set; }
        public string ServiceName { get; set; }

        private ServiceControllerStatus _status;
        public ServiceControllerStatus Status 
        {
            get { return _status; }
            set { SetProperty(ref _status, value); } 
        }
    }
}