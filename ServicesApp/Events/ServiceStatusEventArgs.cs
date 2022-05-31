using System;
using System.ServiceProcess;

public class ServiceControllerEventArgs : EventArgs
{
    public ServiceController ServiceController { get; private set; }
    
    public ServiceControllerEventArgs(ServiceController service) => ServiceController = service;
}