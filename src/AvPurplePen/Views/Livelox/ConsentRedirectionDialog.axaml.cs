using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels.Livelox;

namespace AvPurplePen.Views.Livelox
{
    /// <summary>
    /// Dialog requesting user consent for Livelox authorization.
    /// Avalonian cross-platform implementation.
    /// </summary>
    public partial class ConsentRedirectionDialog : Window
    {
        public ConsentRedirectionDialog()
        {
            InitializeComponent();
        }

        private void ContinueButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ConsentRedirectionDialogViewModel vm)
            {
                vm.UserConsented = true;
            }
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}