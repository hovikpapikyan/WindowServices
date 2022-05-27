using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ExtendedServiceController : ServiceController
{
    public event EventHandler<ServiceStatusEventArgs> StatusChanged;
    private Dictionary<ServiceControllerStatus, Task> _tasks = new Dictionary<ServiceControllerStatus, Task>();

    new public ServiceControllerStatus Status
    {
        get
        {
            base.Refresh();
            return base.Status;
        }
    }

    public ExtendedServiceController(string ServiceName) : base(ServiceName)
    {
        foreach (ServiceControllerStatus status in Enum.GetValues(typeof(ServiceControllerStatus)))
        {
            _tasks.Add(status, null);
        }
        StartListening();
    }

    private void StartListening()
    {
        foreach (ServiceControllerStatus status in Enum.GetValues(typeof(ServiceControllerStatus)))
        {
            if (Status != status && (_tasks[status] == null || _tasks[status].IsCompleted))
            {
                _tasks[status] = Task.Run(() =>
                {
                    try
                    {
                        base.WaitForStatus(status);
                        OnStatusChanged(new ServiceStatusEventArgs(status));
                        StartListening();
                    }
                    catch(Exception ex)
                    {
                       
                    }
                });
            }
        }
    }

    protected void OnStatusChanged(ServiceStatusEventArgs e)
    {
        StatusChanged?.Invoke(this, e);
    }
}