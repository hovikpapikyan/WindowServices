using System;
using Prism.Mvvm;
using System.Linq;
using Prism.Commands;
using System.Windows;
using System.Management;
using ServicesApp.Models;
using System.Windows.Input;
using System.ServiceProcess;
using System.Collections.ObjectModel;

namespace ServicesApp.ViewModels
{
    public class ServiceViewModel : BindableBase
    {
        public ServiceViewModel()
        {
            LoadServices();

            StartOrStopCommand = new DelegateCommand(StartOrStop);
        }

        public void LoadServices()
        {
            var output = new ObservableCollection<ServiceModel>();

            var serviceContrlollers = ServiceExtension.GetServices();
            foreach (var controller in serviceContrlollers)
            {
                var service = new ServiceModel
                {
                    Status = controller.Status,
                    ServiceName = controller.ServiceName,
                    DisplayName = controller.DisplayName,
                    Account = new ManagementObject("Win32_Service.Name='" + controller.ServiceName + "'")["StartName"]?.ToString(),
                };

                ExtendedServiceController serviceController = new(service.ServiceName);
                serviceController.StatusChanged += ServiceController_StatusChanged;

                output.Add(service);
            }

            Services = output;
        }

        private void ServiceController_StatusChanged(object sender, ServiceStatusEventArgs e)
        {
            if (sender is ServiceController service)
            {
                var controller = Services.FirstOrDefault(s => s.ServiceName == service.ServiceName);
                if (controller == null)
                    return;
                
                Application.Current.Dispatcher.BeginInvoke(new Action(() => controller.Status = e.Status));
            }
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

        #region Command Methods

        public void StartOrStop()
        {
            var controller = ServiceExtension.GetServices().FirstOrDefault(n => n.ServiceName == SelectedRow.ServiceName);
            try
            {
                if (controller == null)
                    return;

                if (controller.Status == ServiceControllerStatus.Stopped)
                {
                    controller.Start();

                    SelectedRow.Status = ServiceControllerStatus.Running;
                    return;
                }

                if (controller.Status == ServiceControllerStatus.Running)
                {
                    controller.Stop();

                    SelectedRow.Status = ServiceControllerStatus.Stopped;
                    return;
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"The service failed + {controller.Status} \n\n{ex}");
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                MessageBox.Show($"The service failed + {controller.Status} \n\n{ex}");
            }
        }
        #endregion
    }
}