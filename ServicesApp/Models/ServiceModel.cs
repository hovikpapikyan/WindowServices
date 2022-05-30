using Prism.Mvvm;
using System.ServiceProcess;

namespace ServicesApp.Models
{
    public class ServiceModel : BindableBase
    {
        public string ServiceName { get; set; }
        
        public string DisplayName { get; set; }

        private ServiceControllerStatus _status;
        public ServiceControllerStatus Status 
        {
            get { return _status; }
            set { SetProperty(ref _status, value); } 
        }
        
        public string Account { get; set; }
    }
}