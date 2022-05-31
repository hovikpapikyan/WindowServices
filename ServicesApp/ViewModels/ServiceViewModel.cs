using System;
using Prism.Mvvm;
using System.Linq;
using Prism.Commands;
using System.Windows;
using System.Management;
using ServicesApp.Models;
using System.Windows.Input;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TimeoutException = System.ServiceProcess.TimeoutException;

namespace ServicesApp.ViewModels;

public class ServiceViewModel : BindableBase
{
    public ServiceViewModel()
    {
        LoadServices();

        StartOrStopCommand = new DelegateCommand(() => StartOrStop());
    }

    #region Commands
    public ICommand StartOrStopCommand { get; set; }
    #endregion

    #region Properties
    private ObservableCollection<ServiceModel> _services;
    public ObservableCollection<ServiceModel> Services
    {
        get { return _services; }
        set { SetProperty(ref _services, value); }
    }

    private ServiceModel _selectedRow;
    public ServiceModel SelectedRow
    {
        get => _selectedRow;
        set => SetProperty(ref _selectedRow, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get { return _isBusy; }
        set { SetProperty(ref _isBusy, value); }
    }


    #endregion
    
    #region Methods

    public async Task LoadServices()
    {
        IsBusy = true;
        Services = await Task.Run(() => new ObservableCollection<ServiceModel>(GetServices()));
        IsBusy = false;
    }

    private IEnumerable<ServiceModel> GetServices()
    {
        foreach (var controller in ServiceExtension.GetServices())
        {
            var changeTracker = new ServiceChangeTracker(controller.ServiceName);
            changeTracker.ServiceChanged += ServiceController_StatusChanged;
            changeTracker.StartListening();

            yield return new ServiceModel
            {
                Status = controller.Status,
                ServiceName = controller.ServiceName,
                DisplayName = controller.DisplayName,
                Account = new ManagementObject("Win32_Service.Name='" + controller.ServiceName + "'")["StartName"]?.ToString(),
            };
        }
    }

    private void ServiceController_StatusChanged(object sender, ServiceControllerEventArgs e)
    {
        if (sender is not ServiceController service)
            return;
     
        var controller = Services.FirstOrDefault(s => s.ServiceName == service.ServiceName);
        if (controller is not null)
            Application.Current.Dispatcher.Invoke(() => controller.Status = e.ServiceController.Status);
    }

    #endregion

    #region Command Methods
    public async Task StartOrStop()
    {
        SelectedRow.Status = await Task.Run(() => GetStatus());
    }

    public ServiceControllerStatus GetStatus()
    {
        var controller = ServiceExtension.GetServices().FirstOrDefault(n => n.ServiceName == SelectedRow.ServiceName);
        try
        {
            if (controller is not null)
            {
                if (controller.Status == ServiceControllerStatus.Stopped)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);

                    return controller.Status;
                }

                if (controller.Status == ServiceControllerStatus.Running)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped);

                    return controller.Status;
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show($"The service failed when trying to {controller.Status} \n\n{ex.Message}");
        }
        catch (TimeoutException ex)
        {
            MessageBox.Show(ex.Message);
        }

        return controller.Status; 
    }
    #endregion

}