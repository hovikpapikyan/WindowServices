using System;
using System.ServiceProcess;

public class ServiceStatusEventArgs : EventArgs
{
    public ServiceControllerStatus Status { get; private set; }
    public ServiceStatusEventArgs(ServiceControllerStatus Status)
    {
        this.Status = Status;
    }
}