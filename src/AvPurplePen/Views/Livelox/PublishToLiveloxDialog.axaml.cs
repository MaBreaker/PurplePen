using Avalonia.Controls;
using Avalonia.Interactivity;
using AvUtil;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using PurplePen.ViewModels;
using PurplePen.ViewModels.Livelox;
using SkiaSharp;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace AvPurplePen.Views.Livelox
{
    /// <summary>
    /// Dialog for publishing events to Livelox.
    /// Avalonian cross-platform implementation.
    ///
    /// Migrated from WinForms PurplePen/Livelox/PublishToLiveloxDialog.cs.
    /// </summary>
    public partial class PublishToLiveloxDialog : Window
    {
        public PublishToLiveloxDialog()
        {
            InitializeComponent();
            Opened += PublishToLiveloxDialog_Opened;
        }

        private async void PublishToLiveloxDialog_Opened(object? sender, EventArgs e)
        {
            if (DataContext is PublishToLiveloxDialogViewModel vm)
            {
                vm.LoadAvailableUsers();
                vm.RequestClose += PublishToLiveloxDialog_Close;
                await vm.InitializeImportableEventAsync();
            }
        }

        private async void PublishToLiveloxDialog_Close(object? sender, EventArgs e)
        {
            // TODO: axaml object has now two events Click and Command, from which other should be removed to avoid confusion.
            //       Command is the preferred way to handle button clicks in MVVM pattern.
            if (DataContext is PublishToLiveloxDialogViewModel vm)
            {
                vm.RequestClose -= PublishToLiveloxDialog_Close;
                vm.Abort();
            }
            Close();
        }

        private async Task<bool> ShowConsentDialogAsync(ConsentRedirectionDialogViewModel viewModel)
        {
            var consentDialog = new ConsentRedirectionDialog();
            consentDialog.DataContext = viewModel;

            bool? result = await consentDialog.ShowDialog<bool?>(this);
            return result == true;
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {

            //if (DataContext is PublishToLiveloxDialogViewModel vm) vm.Abort();
            //PublishToLiveloxDialog_Close(sender, e);
        }

        /// <summary>
        /// Repaint the logo panel with the Livelox logo.
        /// </summary>
        private void LiveloxPanel_Paint(object? sender, SkiaDrawingView.PaintEventArgs e)
        {
            // Drawing in design mode causes the designer to crash.
            e.Canvas.Clear(SKColors.Empty);
            IGraphicsBitmap liveloxImage = ImageResources.LiveloxImage;
            Skia_GraphicsTarget panel = new Skia_GraphicsTarget(e.Canvas);
            panel.DrawBitmap(liveloxImage, new RectangleF(0, 0, Convert.ToSingle(e.LogicalSize.Width), Convert.ToSingle(e.LogicalSize.Height)), BitmapScaling.HighQuality);
        }

        /// <summary>
        /// Handled vie model message box requests and shows platform-specific dialog boxes.
        /// </summary>
        /*
        private async void ViewModel_DialogRequested(object? sender, DialogRequestedEventArgs e)
        {
            // Show platform-specific dialog
            bool? result = null;

            if (e.DialogType == SimpleDialogType.Question)
            {
                MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
                {
                    Message = e.Message,
                    Buttons = MessageBoxButtons.YesNo,
                    DefaultButton = MessageBoxButton.Yes,
                    Icon = MessageBoxIcon.Question
                };
                await Services.DialogService.ShowDialogAsync(vm);
                result = vm.ChosenButton == MessageBoxButton.Yes;

            }
            else if (e.DialogType == SimpleDialogType.Error)
            {
                MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
                {
                    Message = e.Message,
                    Icon = MessageBoxIcon.Error,
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok
                };
                await Services.DialogService.ShowDialogAsync(vm);
                result = true;
            }
            else if (e.DialogType == SimpleDialogType.Info)
            {
                MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
                {
                    Message = e.Message,
                    Icon = MessageBoxIcon.Error,
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok
                };
                await Services.DialogService.ShowDialogAsync(vm);
                result = true;
            }

            if (result == true)  // User clicked Yes or OK
            {
                (sender as PublishToLiveloxDialogViewModel)!.PendingDialogResult = SimpleDialogResult.Yes;
            }
            else if (result == false)
            {
                (sender as PublishToLiveloxDialogViewModel)!.PendingDialogResult = SimpleDialogResult.No;
            }

            // Notify ViewModel that dialog is complete
            (sender as PublishToLiveloxDialogViewModel)!.DialogCompleted?.Invoke(sender, EventArgs.Empty);
        }
        */
    }

}