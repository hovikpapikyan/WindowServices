using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using WindowServices.Extensions;
using System.Collections.Generic;
using System.Windows;

public class ServiceChangeTracker : ServiceController
{
    public event EventHandler<ServiceControllerEventArgs> ServiceChanged;
    private Dictionary<ServiceControllerStatus, Task> _tasks = new Dictionary<ServiceControllerStatus, Task>();

    new public ServiceControllerStatus Status
    {
        get
        {
            base.Refresh();
            return base.Status;
        }
    }

    public ServiceChangeTracker(string ServiceName) : base(ServiceName)
    {
        foreach (ServiceControllerStatus status in Enum.GetValues(typeof(ServiceControllerStatus)))
            _tasks.Add(status, null);
    }

    public void StartListening()
    {
        foreach (ServiceControllerStatus status in Enum.GetValues(typeof(ServiceControllerStatus)))
        {
            if (Status != status && (_tasks[status] == null || _tasks[status].IsCompleted))
            {
                _tasks[status] = Task.Run(async () =>
                {
                    try
                    {
                        await this.WaitForStatusAsync(status, TimeSpan.FromMinutes(5));

                        OnServiceChanged(new ServiceControllerEventArgs(this));

                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);// for Log...
                    }
                    finally
                    {
                        StartListening();
                    }
                });
            }
        }
    }

    protected void OnServiceChanged(ServiceControllerEventArgs e)
    {
        ServiceChanged?.Invoke(this, e);
    }
}