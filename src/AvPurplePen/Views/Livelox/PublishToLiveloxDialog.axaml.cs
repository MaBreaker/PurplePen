using Avalonia.Controls;
using Avalonia.Interactivity;
using AvUtil;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using PurplePen.ViewModels.Livelox;
using SkiaSharp;
using System;
using System.Drawing;
using System.Threading.Tasks;

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
                await vm.InitializeImportableEventAsync(ShowConsentDialogAsync);
            }
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
            // TODO: How to execute ViewModel Abort funtion from here ?
            //vm.Abort();
            Close(false);
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
    }
}