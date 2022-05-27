using System.ServiceProcess;

namespace ServicesApp
{
    public static class ServiceExtension
    {
        public static ServiceController[] GetServices()
        {
            return ServiceController.GetServices();
        }
    }
}