﻿using System;
using System.Windows;
using Accelerider.Windows.Infrastructure.Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Practices.Unity;
using System.Net;
using Accelerider.Windows.Common;
using Accelerider.Windows.Infrastructure;
using Accelerider.Windows.ViewModels;
using Prism.Mvvm;
using Prism.Unity;
using Prism.Regions;

using Accelerider.Windows.Views.NetDisk;
using Accelerider.Windows.Views.SharingGroup;
using Accelerider.Windows.Views.Transmission;

namespace Accelerider.Windows
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Overridered methods
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.Resolve<Core.Module>().Initialize();
            Container.RegisterInstance(new SnackbarMessageQueue(TimeSpan.FromSeconds(2)));
        }

        protected override void ConfigureViewModelLocator() => ViewModelLocationProvider.SetDefaultViewModelFactory(ResolveViewModel);

        protected override DependencyObject CreateShell() => new Views.Entering.EnteringWindow();

        protected override void InitializeShell()
        {
            ServicePointManager.DefaultConnectionLimit = 99999;
            ConfigureApplicationEventHandlers();
            ShellController.Show((Window)Shell);
        }

        protected override void InitializeModules()
        {
            base.InitializeModules();
            Container.Resolve<Components.Authenticator.Module>().Initialize();
            var region = Container.Resolve<IRegionViewRegistry>();
            region.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(NetDiskComposite));
            region.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(TransmissionComposite));
            region.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(SharingGroupComposite));
        }
        #endregion

        #region Private methods
        private void ConfigureApplicationEventHandlers()
        {
            var resolver = Container.Resolve<ExceptionResolver>();
            AppDomain.CurrentDomain.UnhandledException += resolver.UnhandledExceptionHandler;
            Application.Current.DispatcherUnhandledException += resolver.DispatcherUnhandledExceptionHandler;

            Application.Current.Exit += OnExit;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            Container.Resolve<IAcceleriderUser>().OnExit();
        }

        private object ResolveViewModel(object view, Type viewModelType)
        {
            var viewModel = Container.Resolve(viewModelType);
            if (view is FrameworkElement frameworkElement &&
                viewModel is ViewModelBase viewModelBase)
            {
                frameworkElement.Loaded += (sender, e) => viewModelBase.OnLoaded(sender);
                frameworkElement.Unloaded += (sender, e) => viewModelBase.OnUnloaded(sender);
            }
            return viewModel;
        }
        #endregion
    }
}
