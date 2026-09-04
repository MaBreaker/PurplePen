using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using PurplePen;
using PurplePen.Livelox;
using PurplePen.Livelox.ApiContracts;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static PurplePen.ViewModels.SelectLocationsForMoveDialogViewModel;

namespace PurplePen.ViewModels.Livelox
{
    /// <summary>
    /// Simple Message Dialog
    /// </summary>
    /*
    public enum SimpleDialogType { None, Info, Error, Question }
    public enum SimpleDialogResult { None, OK, Yes, No, Cancel }

    public class DialogRequestedEventArgs : EventArgs
    {
        public SimpleDialogType DialogType { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
    */

    /// <summary>
    /// ViewModel for publishing events to Livelox.
    /// Manages user authentication, event creation/update, and file uploads.
    ///
    /// Migrated from WinForms PurplePen/Livelox/PublishToLiveloxDialog.cs.
    /// </summary>
    public partial class PublishToLiveloxDialogViewModel(
        Controller controller,
        SymbolDB symbolDB,
        LiveloxPublishSettings publishSettings) : ViewModelBase //:ObservableObject // 
    {
        public enum LiveloxDialogType { Publish, Consent, OpenNew, OpenUpdate, Error }
        private string? LiveloxUrl;

        private readonly Controller controller = controller;
        private readonly SymbolDB symbolDB = symbolDB;
        private readonly SettingsProvider settingsProvider = new SettingsProvider();
        private ImportableEvent? existingImportableEvent;
        //private bool isExecuting;
        private LiveloxApiClient? currentApiClient;
        private readonly List<IAbortable> ongoingCalls = new();
        /// <summary>
        /// Event raised when ViewModel wants to close the dialog.
        /// </summary>
        public event EventHandler? RequestClose;
        public LiveloxPublishSettings PublishSettings { get; } = publishSettings;

        [ObservableProperty]
        private LiveloxDialogType? dialogState = LiveloxDialogType.Consent;

        [ObservableProperty]
        private string? resolution;

        [ObservableProperty]
        private ObservableCollection<User> availableUsers = new ObservableCollection<User>();

        [ObservableProperty]
        private User? selectedUser;

        [ObservableProperty]
        private bool rememberConsent;

        [ObservableProperty]
        private string? eventName;

        [ObservableProperty]
        private string? eventOrganizers;

        [ObservableProperty]
        private string? eventTimeInterval;

        [ObservableProperty]
        private bool showConsentPanel;

        [ObservableProperty]
        private bool showSettingsPanel;

        [ObservableProperty]
        private bool showSettings = false;

        [ObservableProperty]
        private bool showUserPanel;

        [ObservableProperty]
        private bool showExistingEventPanel;

        [ObservableProperty]
        private bool showPublishButton;

        [ObservableProperty]
        private bool showPublishOtherButton;

        [ObservableProperty]
        private bool showUpdateButton;

        [ObservableProperty]
        private bool showContinueButton;

        [ObservableProperty]
        private bool showOkButton;

        [ObservableProperty]
        private bool showCancelButton;

        [ObservableProperty]
        private bool progressLoading;

        [ObservableProperty]
        private bool progressError = false;

        [ObservableProperty]
        private string progressColor = "Blue";

        [ObservableProperty]    
        private int progressValue = 0;

        [ObservableProperty]
        private string? progressMessage;

        [ObservableProperty]
        private bool canPublish = true;

        /*
        [ObservableProperty]
        private SimpleDialogType pendingDialogType = SimpleDialogType.None;

        [ObservableProperty]
        private string? pendingDialogMessage;

        [ObservableProperty]
        private string? pendingDialogTitle;

        [ObservableProperty]
        private SimpleDialogResult pendingDialogResult = SimpleDialogResult.None;

        /// Event raised when dialog is requested
        public event EventHandler<DialogRequestedEventArgs>? DialogRequested;

        /// Event raised when dialog is completed  
        public event EventHandler? DialogCompleted;
        */

        /// <summary>
        /// Initializes the dialog with existing event data if available.
        /// </summary>
        public async Task InitializeImportableEventAsync()
        {
            var importableEventId = controller.GetEventDB().GetEvent().liveloxImportableEventId;
            
            ProgressLoading = false;
            ProgressMessage = "";

            if (string.IsNullOrEmpty(importableEventId))
            {
                return;
            }

            var settings = settingsProvider.LoadSettings();
            var user = settings.Users.FirstOrDefault();
            if (user == null)
            {
                return;
            }

            ProgressLoading = true;
            ProgressMessage = LiveloxResources.LoadingLiveloxEvent;
            UpdateDialogState(LiveloxDialogType.Publish);

            var liveloxApiClient = CreateLiveloxApiClient(user.TokenInformation);
            try
            {
                Action<LiveloxApiCall<ImportableEvent>> callback = call =>
                {
                    if (call?.Result != null)
                    {
                        existingImportableEvent = call.Result;
                        UpdateExistingEventDisplay();
                    }
                    else if (call?.Exception is StatusCodeException statusEx &&
                        (statusEx.StatusCode == HttpStatusCode.NotFound ||
                         statusEx.StatusCode == HttpStatusCode.Forbidden))
                    {
                        // Event has been removed or user lost access
                        // Pretend the event hasn't been published
                        existingImportableEvent = null;
                    }
                    else if (call?.Exception is OAuth2Exception ||
                         (call?.Exception is StatusCodeException statusEx2 &&
                          statusEx2.StatusCode == HttpStatusCode.Unauthorized))
                    {
                        // Authorization problem, remove user
                        settings.Users = settings.Users.Skip(1).ToArray();
                        settingsProvider.SaveSettings(settings);
                        LoadAvailableUsers();
                    }
                    else if (call?.Exception != null)
                    {
                        // Unknown error
                        throw call.Exception ?? new Exception("Failed to get importable event");
                    }

                    ProgressLoading = false;
                    ProgressMessage = "";
                    UpdateDialogState();
                };
                liveloxApiClient.GetImportableEvent(importableEventId, callback);
            }
            catch (Exception)
            {
                ProgressLoading = false;
                ProgressMessage = "";
                UpdateDialogState();
                throw;
            }
        }

        /// <summary>
        /// Loads available users from settings and updates UI.
        /// </summary>
        public void LoadAvailableUsers()
        {
            var settings = settingsProvider.LoadSettings();
            AvailableUsers.Clear();

            foreach (var user in settings.Users)
            {
                AvailableUsers.Add(user);
            }

            if (AvailableUsers.Count > 0)
            {
                AvailableUsers.Add(new User
                {
                    FirstName = $"[{LiveloxResources.AnotherUser}]",
                    PersonId = -1
                });
                SelectedUser = AvailableUsers[0];
            }

            UpdateDialogState();
        }

        private void UpdateExistingEventDisplay()
        {
            var ev = existingImportableEvent?.ImportedEvent;
            if (ev != null)
            {
                EventName = ev.Name;
                EventOrganizers = string.Join(", ", ev.Organisers.Select(o => o.Name));

                if (ev.TimeInterval.Start == null || ev.TimeInterval.End == null)
                {
                    EventTimeInterval = "";
                }
                else
                {
                    var startTime = ev.TimeInterval.Start.Value.ToLocalTime();
                    var endTime = ev.TimeInterval.End.Value.ToLocalTime();

                    EventTimeInterval = startTime.ToShortDateString() + " " + startTime.ToShortTimeString() +
                        " - " +
                        (endTime.Date == startTime.Date ? "" : endTime.ToShortDateString() + " ") +
                        endTime.ToShortTimeString();
                }

                ShowExistingEventPanel = true;
            }
        }

        private void UpdateDialogStateX()
        {
            Resolution = PublishSettings.GetResolution(controller.MapScale)
                .ToString(CultureInfo.CurrentCulture);

            ShowUserPanel = AvailableUsers.Count > 0;
            ShowExistingEventPanel = existingImportableEvent?.ImportedEvent != null;
            ShowPublishButton = !ShowExistingEventPanel;
            ShowUpdateButton = ShowExistingEventPanel;  
            CanPublish = !ProgressLoading;
        }

        private void UpdateDialogState(LiveloxDialogType? type = null)
        {
            if (type != null)
                DialogState = type;

            if (DialogState == null)
                return;

            ProgressError = false;
            ProgressColor = "Blue";
            ProgressValue = 0;

            if (DialogState == LiveloxDialogType.Consent)
            {
                ShowSettingsPanel = false;
                ShowUserPanel = false;
                ShowExistingEventPanel = false;
                ShowConsentPanel = true;
                ShowContinueButton = true;
                ShowPublishButton = false;
                ShowPublishOtherButton = false;
                ShowUpdateButton = false;
                ShowOkButton = false;
                ShowCancelButton = true;

                ProgressLoading = false;
                CanPublish = false;
            }
            else if (DialogState == LiveloxDialogType.Publish)
            {
                Resolution = PublishSettings.GetResolution(controller.MapScale)
                    .ToString(CultureInfo.CurrentCulture);
                CanPublish = !ProgressLoading;
                ShowSettingsPanel = true;
                ShowUserPanel = AvailableUsers.Count > 0;
                ShowExistingEventPanel = existingImportableEvent?.ImportedEvent != null;
                ShowConsentPanel = false;
                ShowContinueButton = false;
                ShowPublishButton = !ShowExistingEventPanel;
                ShowPublishOtherButton = !ShowPublishButton;
                ShowUpdateButton = ShowExistingEventPanel;
                ShowCancelButton = true;
                ShowOkButton = false;
            }
            else if (DialogState == LiveloxDialogType.OpenNew)
            {
                ShowSettingsPanel = false;
                ShowUserPanel = false;
                ShowExistingEventPanel = false;
                ShowConsentPanel = false;
                ShowContinueButton = false;
                ShowPublishButton = false;
                ShowPublishOtherButton = false;
                ShowUpdateButton = false;
                ShowOkButton = true;
                ShowCancelButton = false;

                ProgressColor = "Green";
                ProgressValue = 1;
                ProgressLoading = false;
                CanPublish = false;
            }
            else if (DialogState == LiveloxDialogType.OpenUpdate)
            {
                ShowSettingsPanel = false;
                ShowUserPanel = false;
                ShowExistingEventPanel = false;
                ShowConsentPanel = false;
                ShowContinueButton = false;
                ShowPublishButton = false;
                ShowPublishOtherButton = false;
                ShowUpdateButton = false;
                ShowOkButton = true;
                ShowCancelButton = true;

                ProgressColor = "Green";
                ProgressValue = 1;
                ProgressLoading = false;
                CanPublish = false;
            }
            else // Error
            {
                ShowSettingsPanel = false;
                ShowUserPanel = false;
                ShowExistingEventPanel = false;
                ShowConsentPanel = false;
                ShowContinueButton = false;
                ShowPublishButton = false;
                ShowPublishOtherButton = false;
                ShowUpdateButton = false;
                ShowOkButton = true;
                ShowCancelButton = false;

                ProgressColor = "Red";
                ProgressValue = 1;
                ProgressError = true;
                ProgressLoading = false;
                CanPublish = false;
            }
        }


        [RelayCommand]
        private void Ok()
        {
            // Show published folder
            if (DialogState == LiveloxDialogType.Publish ||
                DialogState == LiveloxDialogType.OpenNew ||
                DialogState == LiveloxDialogType.OpenUpdate)
            {
                if (LiveloxUrl != null)
                {
                    ShowUrlInBrowser(LiveloxUrl);
                }
                else
                {
                    // This should newer happen
                    HandleError(new Exception("No Livelox URL available to open."));
                }
            }
            Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            Abort();
            Close();
        }

        [RelayCommand]
        private void Close()
        {
            // TODO: How to reach Dialog window and Close if from here ?
            //       axaml object has now two events Click and Command, from which other should be removed to avoid confusion.
            //       Command is the preferred way to handle button clicks in MVVM pattern.
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        public async Task ContinueAsync()
        {
            await RequestConsentAsync(RememberConsent);
        }

        [RelayCommand]
        public async Task PublishAsync()
        {
            if(SelectedUser == null)
            {
                // No user selected, request consent first
                HandleError(new Exception("No user selected."));
                return;
            }
            await PublishToLiveloxAsync(SelectedUser);
        }

        [RelayCommand]
        public async Task UpdateEventAsync()
        {
            if (SelectedUser == null)
            {
                // No user selected, request consent first
                HandleError(new Exception("No user selected."));
                return;
            }
            await UpdateLiveloxEventAsync(SelectedUser);
        }

        private async Task RequestConsentAsync(bool rememberConsent)
        {
            // For now, we'll assume the dialog shows and returns a result

            var refreshTokenLifeLength = rememberConsent
                ? (TimeSpan?)null
                : TimeSpan.FromHours(1);

            ProgressLoading = true;
            ProgressMessage = LiveloxResources.RedirectingToLivelox;

            var liveloxApiClient = CreateLiveloxApiClient(null);

            try
            {
                User? user = null; // This will be obtained from OAuth flow

                Action activateAppCallback = () =>
                {
                    // Do nothing for now; in a real application, this might bring the app to the foreground
                };

                // Show OAuth dialog and get user
                Action<LiveloxApiCall<User>> callback = call =>
                {
                    if (call != null)
                    {
                        ProgressLoading = false;
                        
                        user = call.Result;
                        if (rememberConsent)
                        {
                            var settings = settingsProvider.LoadSettings();
                            settings.Users = new[] { user }
                                .Concat(settings.Users.Where(o => o.PersonId != user.PersonId))
                                .ToArray();
                            settingsProvider.SaveSettings(settings);
                        }

                        ProgressLoading = false;
                        ProgressMessage = "";
                        UpdateDialogState(LiveloxDialogType.Publish);
                    }
                };

                Action<String> progressinfo = call =>
                {
                    if (call != null)
                    {
                        ProgressMessage = call;
                    }
                };

                // Show OAuth dialog / Browser
                liveloxApiClient.AskForUserConsent(activateAppCallback, refreshTokenLifeLength, callback, progressinfo);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                throw;
            }
        }

        private async Task PublishToLiveloxAsync(User user)
        {
            var manager = new PublishManager();
            string? temporaryDirectory = null;

            try
            {
                ProgressLoading = true;
                ProgressMessage = LiveloxResources.AssemblingCourseSettingInformation;
                UpdateDialogState();

                UpdateSettingsFromUI();
                temporaryDirectory = manager.CreateTemporaryDirectory();
                var importableEvent = manager.CreateImportableEvent(
                    controller, symbolDB,
                    PublishSettings.GetResolution(controller.MapScale),
                    temporaryDirectory);

                ProgressMessage = LiveloxResources.UploadingCourseSettingInformation;
                var liveloxApiClient = CreateLiveloxApiClient(user.TokenInformation);

                Action<LiveloxApiCall<ImportableEventLink>> callback = createCall =>
                {
                    if (!createCall.Success)
                    {
                        //throw createCall.Exception ?? new Exception("Failed to create importable event");
                        HandleError(createCall.Exception);
                        return;
                    }

                    var importableEventLink = createCall.Result;

                    PersistUserList(user);

                    // zip all files and upload them
                    var zipBytes = CreateZipFileBytes(temporaryDirectory, importableEvent);

                    Action<LiveloxApiCall<LiveloxApiNullResponse>> uploadCallback = uploadFilesCall =>
                    {
                        if (!uploadFilesCall.Success)
                        {
                            //throw uploadFilesCall.Exception ?? new Exception("Failed to upload files");
                            HandleError(uploadFilesCall.Exception);
                            return;
                        }

                        ProgressLoading = false;
                        PersistLiveloxEventIdToDB(importableEventLink.Id);
                        CanPublish = true;

                        // Show imported event in Livelox
                        ProgressMessage = LiveloxResources.ImportableEventCreatedInformation;

                        // Show event in Browser
                        LiveloxUrl = importableEventLink.LiveloxImportEventUrl;

                        //controller.ui.InfoMessage(LiveloxResources.ImportableEventCreatedInformation);

                        // Set dialog layout
                        UpdateDialogState(LiveloxDialogType.OpenNew);

                        // TODO:
                        // - Hide settings panel
                        // - Hide user panel
                        // - Add new "Close" button into dialog
                        // - Hide all buttons except "Close"-button

                        // TODO: Separate message dialog would need some deeper modifcations to API functions
                        /*
                        // Show imported event in Livelox
                        await InfoMessage(LiveloxResources.ImportableEventCreatedInformation);
                      
                        // Show event in Browser
                        ShowUrlInBrowser(importableEventLink.LiveloxImportEventUrl);

                        // Signal to close the dialog
                        //OnRequestClose(); // With this ResponseCallback fails and would need marshaling Dispatcher.UIThread.InvokeAsync(() => call.Callback(call));
                        */

                        /*
                        ProgressMessage = "";

                        DialogRequested?.Invoke(this, new DialogRequestedEventArgs
                        {
                            DialogType = SimpleDialogType.Question,
                            Message = "Import this event?",
                            Title = "Confirm"
                        });

                        // Continue after this completes
                        DialogCompleted += (s, e) =>
                        {
                            // Show event in Browser
                            ShowUrlInBrowser(importableEventLink.LiveloxImportEventUrl);
                            // Signal to close the dialog
                            OnRequestClose();
                        };
                        */
                    };
                    liveloxApiClient.UploadFile(importableEventLink.Id, "files.zip", zipBytes, uploadCallback);
                };
                liveloxApiClient.CreateImportableEvent(importableEvent, callback);
            }
            catch (Exception)
            {
                UpdateDialogState(LiveloxDialogType.Error);
                throw;
            }
            finally
            {
                if (temporaryDirectory != null)
                {
                    manager.DeleteTemporatyDirectory(temporaryDirectory);
                }
            }
        }

        private async Task UpdateLiveloxEventAsync(User user)
        {
            var manager = new PublishManager();
            string? temporaryDirectory = null;

            try
            {
                ProgressLoading = true;
                ProgressMessage = LiveloxResources.AssemblingCourseSettingInformation;
                UpdateDialogState(LiveloxDialogType.Publish);

                UpdateSettingsFromUI();
                temporaryDirectory = manager.CreateTemporaryDirectory();
                var importableEvent = manager.CreateImportableEvent(
                    controller, symbolDB,
                    PublishSettings.GetResolution(controller.MapScale),
                    temporaryDirectory);

                ProgressMessage = LiveloxResources.UploadingCourseSettingInformation;
                var liveloxApiClient = CreateLiveloxApiClient(user.TokenInformation);
                ImportableEventLink? existingImportableEventLink = existingImportableEvent?.Link;
                string? existingImportableEventLinkId = existingImportableEventLink?.Id;

                Action <LiveloxApiCall<ImportableEventLink>> callback = updateCall =>
                {
                    if (!updateCall.Success)
                    {
                        //throw updateCall.Exception ?? new Exception("Failed to update importable event");
                        HandleError(updateCall.Exception);
                        return;
                    }

                    var importableEventLink = updateCall.Result;

                    PersistUserList(user);

                    // zip all files and upload them
                    var zipBytes = CreateZipFileBytes(temporaryDirectory, importableEvent);

                    Action<LiveloxApiCall<LiveloxApiNullResponse>> uploadCallback = uploadFilesCall =>
                    {
                        // Remove temporary files
                        if (temporaryDirectory != null)
                        {
                            var manager2 = new PublishManager();
                            manager2.DeleteTemporatyDirectory(temporaryDirectory);
                        }

                        if (!uploadFilesCall.Success)
                        {
                            //throw uploadFilesCall.Exception ?? new Exception("Failed to upload files");
                            HandleError(uploadFilesCall.Exception);
                            return;
                        }

                        // Show success dialog
                        if (importableEventLink.LiveloxImportEventUrl != null)
                        {
                            PersistLiveloxEventIdToDB(importableEventLink.Id);

                            // Show imported event in Livelox
                            ProgressMessage = LiveloxResources.ImportableEventCreatedInformation;

                            // Show event in Browser
                            LiveloxUrl = importableEventLink.LiveloxImportEventUrl;

                            // Set dialog layout
                            UpdateDialogState(LiveloxDialogType.OpenNew);

                            // TODO: 
                            // - Hide settings panel
                            // - Hide user panel
                            // - Add new "Close" button into dialog
                            // - Hide all buttons except "Close"-button

                            // TODO: These would need some deeper modifications to API functions
                            /*
                            // Show imported event in Livelox
                            await InfoMessage(LiveloxResources.ImportableEventCreatedInformation);

                            // Show event in Browser
                            ShowUrlInBrowser(importableEventLink.LiveloxImportEventUrl);

                            // Signal to close the dialog
                            OnRequestClose(); // With this ResponseCallback fails and would need marshaling Dispatcher.UIThread.InvokeAsync(() => call.Callback(call));
                            */

                            /*
                            ProgressMessage = "";

                            DialogRequested?.Invoke(this, new DialogRequestedEventArgs
                            {
                                DialogType = SimpleDialogType.Info,
                                Message = "Import this event?",
                                Title = "Confirm"
                            });

                            // Continue after this completes
                            DialogCompleted += (s, e) =>
                            {
                                // Show event in Browser
                                ShowUrlInBrowser(importableEventLink.LiveloxImportEventUrl);
                                // Signal to close the dialog
                                OnRequestClose();
                            };
                            */
                        }
                        else
                        {
                            ProgressMessage = LiveloxResources.UpdatingLiveloxEvent;

                            Action<LiveloxApiCall<ImportableEventLink>> importCallback = importImportableEventCall =>
                            {
                                if (!importImportableEventCall.Success)
                                {
                                    //throw importImportableEventCall.Exception ?? new Exception("Failed to import event");
                                    HandleError(importImportableEventCall.Exception);
                                    return;
                                }

                                importableEventLink = importImportableEventCall.Result;
                                PersistLiveloxEventIdToDB(importableEventLink.Id);

                                // Show updated event in Livelox
                                ProgressMessage = LiveloxResources.ImportableEventUpdatedInformation;

                                // TODO: 
                                // - Hide settings panel
                                // - Hide user panel
                                // - Add new "Open" button into dialog
                                // - Hide all buttons except "Open" "Cancel"-button
                                // - Open url only if "Open" button is pressed

                                // Show event in Browser
                                LiveloxUrl = importableEventLink.LiveloxEditEventUrl;

                                // Set dialog layout
                                UpdateDialogState(LiveloxDialogType.OpenUpdate);

                                // TODO: These would need some deeper modifications to API functions
                                /*
                                // Show updated event in Livelox
                                if (await YesNoQuestion(LiveloxResources.ImportableEventUpdatedInformation, true) == true)
                                    // Show event in Browser
                                    ShowUrlInBrowser(importableEventLink.LiveloxEditEventUrl);

                                // Signal to close the dialog
                                OnRequestClose(); // With this ResponseCallback fails and would need marshaling Dispatcher.UIThread.InvokeAsync(() => call.Callback(call));
                                */
                                /*
                                ProgressMessage = "";

                                DialogRequested?.Invoke(this, new DialogRequestedEventArgs
                                {
                                    DialogType = SimpleDialogType.Question,
                                    Message = LiveloxResources.ImportableEventUpdatedInformation,
                                    Title = MiscText.AppTitle
                                });

                                // Continue after this completes
                                DialogCompleted += (s, e) =>
                                {
                                    if (PendingDialogResult == SimpleDialogResult.Yes) {
                                        // Show event in Browser
                                        ShowUrlInBrowser(importableEventLink.LiveloxEditEventUrl);
                                    }
                                    // Signal to close the dialog
                                    OnRequestClose();
                                };
                                */
                            };
                            liveloxApiClient.ImportImportableEvent(importableEventLink.Id, importCallback);
                        }
                    };
                    liveloxApiClient.UploadFile(importableEventLink.Id, "files.zip", zipBytes, uploadCallback);
                };
                _ = liveloxApiClient.UpdateImportableEvent(existingImportableEventLinkId, importableEvent, callback);
            }
            catch (Exception)
            {
                UpdateDialogState(LiveloxDialogType.Error);
                throw;
            }
            finally
            {
                if (temporaryDirectory != null)
                {
                    manager.DeleteTemporatyDirectory(temporaryDirectory);
                }
            }
        }

        private void UpdateSettingsFromUI()
        {
            if (double.TryParse(Resolution, NumberStyles.Any, CultureInfo.CurrentCulture, out var resolution))
            {
                if (LiveloxPublishSettings.IsLargeScaleMap(controller.MapScale))
                {
                    PublishSettings.largeScaleMapResolution = resolution;
                }
                else
                {
                    PublishSettings.smallScaleMapResolution = resolution;
                }
            }
        }

        private byte[] CreateZipFileBytes(string directory, ImportableEvent importableEvent)
        {
            var fileNames = importableEvent.Maps.Select(map => map.FileName)
                .Concat(importableEvent.CourseDataFileNames)
                .Concat(importableEvent.CourseImageFileNames)
                .ToArray();

            var buffer = new byte[4096];
            using (var zipStream = new MemoryStream())
            {
                using (var zipOutputStream = new ZipOutputStream(zipStream))
                {
                    foreach (var fileName in fileNames)
                    {
                        using (var fileStream = new FileStream(
                            Path.Combine(directory, fileName), FileMode.Open, FileAccess.Read))
                        {
                            var zipEntry = new ZipEntry(ZipEntry.CleanName(fileName))
                            {
                                IsUnicodeText = true
                            };
                            zipOutputStream.PutNextEntry(zipEntry);
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fileStream.Read(buffer, 0, buffer.Length);
                                zipOutputStream.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }

                    zipOutputStream.Finish();
                    return zipStream.ToArray();
                }
            }
        }

        private void PersistLiveloxEventIdToDB(string liveloxImportableEventId)
        {
            var eventDB = controller.GetEventDB();
            var undoMgr = controller.GetUndoMgr();
            const int commandNumber = 27635; // what number to use here?
            undoMgr.BeginCommand(commandNumber, CommandNameText.SetLiveloxImportableEventId);
            ChangeEvent.SetLiveloxImportableEventId(eventDB, liveloxImportableEventId);
            undoMgr.EndCommand(commandNumber);
        }

        private void PersistUserList(User user)
        {
            var settings = settingsProvider.LoadSettings();
            
            // the user is to be remembered,
            // place it first in the list
            settings.Users = new[] { user }
                .Concat(settings.Users.Where(o => o.PersonId != user.PersonId))
                .ToArray();
            settingsProvider.SaveSettings(settings);
        }

        private User? GetSelectedUser()
        {
            if (SelectedUser == null || SelectedUser.PersonId == -1)
            {
                return null;
            }
            return SelectedUser;
        }

        private LiveloxApiClient CreateLiveloxApiClient(OAuth2TokenInformation? tokenInformation)
        {
            currentApiClient?.Dispose();
            currentApiClient = new LiveloxApiClient(tokenInformation, OnApiClientRequestCreated, OnApiClientRequestCompleted);
            return currentApiClient;
        }

        private void HandleError(Exception ex)
        {
            string message;
            if ((ex as StatusCodeException)?.StatusCode == HttpStatusCode.Unauthorized ||
                (ex as OAuth2Exception)?.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Authorization problem, remove user
                var settings = settingsProvider.LoadSettings();
                settings.Users = settings.Users.Skip(1).ToArray();
                settingsProvider.SaveSettings(settings);
                message = LiveloxResources.UnauthorizedMessage;
            }
            else
            {
                message = ex?.Message ?? LiveloxResources.UnknownError;
            }
            ProgressMessage = message;
            UpdateDialogState(LiveloxDialogType.Error);
        }

        public async Task InfoMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
            {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Information
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task WarningMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
            {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Warning
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task ErrorMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
            {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Error
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task<bool> OKCancelMessage(string message, bool okDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
            {
                Message = message,
                Buttons = MessageBoxButtons.OkCancel,
                DefaultButton = okDefault ? MessageBoxButton.Ok : MessageBoxButton.Cancel,
                Icon = MessageBoxIcon.Question
            };
            await Services.DialogService.ShowDialogAsync(vm);
            return vm.ChosenButton == MessageBoxButton.Ok;
        }

        public async Task<YesNoCancel> YesNoCancelQuestion(string message, bool yesDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
            {
                Message = message,
                Buttons = MessageBoxButtons.YesNoCancel,
                DefaultButton = yesDefault ? MessageBoxButton.Yes : MessageBoxButton.No,
                Icon = MessageBoxIcon.Question
            };
            await Services.DialogService.ShowDialogAsync(vm);
            if (vm.ChosenButton == MessageBoxButton.Yes)
                return YesNoCancel.Yes;
            else if (vm.ChosenButton == MessageBoxButton.No)
                return YesNoCancel.No;
            else
                return YesNoCancel.Cancel;
        }

        public async Task<bool> YesNoQuestion(string message, bool yesDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
            {
                Message = message,
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = yesDefault ? MessageBoxButton.Yes : MessageBoxButton.No,
                Icon = MessageBoxIcon.Question
            };
            await Services.DialogService.ShowDialogAsync(vm);
            return vm.ChosenButton == MessageBoxButton.Yes;
        }

        private void ShowEvent()
        {
            if (existingImportableEvent?.Link != null)
            {
                ShowUrlInBrowser(existingImportableEvent.Link.LiveloxShowEventUrl);
            }
        }

        private void EditEvent()
        {
            if (existingImportableEvent?.Link != null)
            {
                ShowUrlInBrowser(existingImportableEvent.Link.LiveloxEditEventUrl);
            }
        }

        private static void ShowUrlInBrowser(string url)
        {
            // Opens request in the browser.
            //Services.WebsiteLauncher.ShowWebsite(url);
            var processInfo = new System.Diagnostics.ProcessStartInfo(url)
            {
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(processInfo);
        }

        private void OnApiClientRequestCreated(IAbortable call)
        {
            ongoingCalls.Add(call);
        }

        private void OnApiClientRequestCompleted(IAbortable call)
        {
            ongoingCalls.Remove(call);
        }

        /// <summary>
        /// Raises the RequestClose event to signal the parent dialog should close.
        /// </summary>
        /*
        private void Close()
        {
            // How to close PublishToLiveloxDialog dialog from here?
            //RequestClose?.Invoke(this, EventArgs.Empty);
        }
        */

        public void Abort()
        {
            var callsToAbort = ongoingCalls.ToArray();
            foreach (var call in callsToAbort)
            {
                call.Abort();
                ongoingCalls.Remove(call);
            }

            if (currentApiClient != null)
            {
                currentApiClient.Abort();
            }
        }
    }
}