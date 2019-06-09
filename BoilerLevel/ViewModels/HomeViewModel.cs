using BoilerLevel.Models;
using BoilerLevel.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using OpenExtensions.Collections;

namespace BoilerLevel.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly INavigationService NavigationService;
        public OCollection<Boiler> Boilers { get; set; } = new OCollection<Boiler>();

        public HomeViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            Boilers.ReplaceRange(BoilerManager.Boilers);
        }

        public void GoToBoilersDetail(Boiler boiler)
        {
            NavigationService.NavigateTo("BoilerDetails", boiler);
        }

        public void DeleteBoiler(Boiler boiler)
        {
            BoilerManager.DeleteBoiler(boiler);
            Boilers.Remove(boiler);
        }

        private RelayCommand<Boiler> _AddBoiler;
        public RelayCommand<Boiler> AddBoiler
            => _AddBoiler ?? (_AddBoiler = new RelayCommand<Boiler>(CreateBoiler, CanCreateBoiler));

        private void CreateBoiler(Boiler boiler)
        {
            BoilerManager.AddBoiler(boiler);
            Boilers.Add(boiler);
        }

        private bool CanCreateBoiler(Boiler boiler)
        {
            if (boiler == null || boiler.Name == "" || boiler.Name == null)
                return false;
            else
                return true;
        }
    }
}