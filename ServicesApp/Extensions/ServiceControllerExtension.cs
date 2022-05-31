using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace WindowServices.Extensions
{
    public static class ServiceControllerExtension
    {
        public static async Task WaitForStatusAsync(this ServiceController controller, ServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            var utcNow = DateTime.UtcNow;

            while (controller.Status != desiredStatus)
            {
                if (DateTime.UtcNow - utcNow > timeout)
                {
                    throw new System.TimeoutException($"Failed to wait for '{controller.ServiceName}' to change status to '{desiredStatus}'.");
                }
                
                await Task.Delay(250).ConfigureAwait(false);
                
                controller.Refresh();
            }
        }
    }
}