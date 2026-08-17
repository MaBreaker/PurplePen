using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels.Livelox
{
    /// <summary>
    /// ViewModel for the Livelox consent redirection dialog.
    /// Handles user consent for OAuth authorization.
    ///
    /// Migrated from WinForms PurplePen/Livelox/ConsentRedirectionDialog.cs.
    /// </summary>
    public partial class ConsentRedirectionDialogViewModel : ObservableObject // ViewModelBase
    {
        [ObservableProperty]
        private bool rememberConsent;

        [ObservableProperty]
        private bool userConsented;

        public ConsentRedirectionDialogViewModel()
        {
            rememberConsent = false;
            userConsented = false;
        }
    }
}