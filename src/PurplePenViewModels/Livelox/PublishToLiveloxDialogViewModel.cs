using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using PurplePen;
using PurplePen.Livelox;
using PurplePen.Livelox.ApiContracts;
using PurplePen.MapModel;

namespace PurplePen.ViewModels.Livelox
{
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
        private readonly Controller controller = controller;
        private readonly SymbolDB symbolDB = symbolDB;
        private readonly SettingsProvider settingsProvider = new SettingsProvider();
        private ImportableEvent? existingImportableEvent;
        //private bool isExecuting;
        private LiveloxApiClient? currentApiClient;
        private readonly List<IAbortable> ongoingCalls = new();

        public LiveloxPublishSettings PublishSettings { get; } = publishSettings;

        [ObservableProperty]
        private string? resolution;

        [ObservableProperty]
        private ObservableCollection<User> availableUsers = new ObservableCollection<User>();

        [ObservableProperty]
        private User? selectedUser;

        [ObservableProperty]
        private string? eventName;

        [ObservableProperty]
        private string? eventOrganizers;

        [ObservableProperty]
        private string? eventTimeInterval;

        [ObservableProperty]
        private bool showSettings = false;

        [ObservableProperty]
        private bool showUserPanel;

        [ObservableProperty]
        private bool showExistingEvent;

        [ObservableProperty]
        private bool showPublishButton;

        [ObservableProperty]
        private bool showPublishOtherButton;

        [ObservableProperty]
        private bool showUpdateButton;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? loadingMessage;

        [ObservableProperty]
        private bool canPublish = true;

        /// <summary>
        /// Initializes the dialog with existing event data if available.
        /// </summary>
        public async Task InitializeImportableEventAsync(
            Func<ConsentRedirectionDialogViewModel, Task<bool>> showConsentDialog)
        {
            var importableEventId = controller.GetEventDB().GetEvent().liveloxImportableEventId;

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

            IsLoading = true;
            LoadingMessage = LiveloxResources.LoadingLiveloxEvent;

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
                        // Authorization problem - remove user
                        // Remove the event ID from the database so that we don't try to update it again.
                        settings.Users = settings.Users.Skip(1).ToArray();
                        settingsProvider.SaveSettings(settings);
                        LoadAvailableUsers();
                    }
                    else if (call?.Exception != null)
                    {
                        // Unknown error
                        throw call.Exception ?? new Exception("Failed to get importable event");
                    }

                    IsLoading = false;
                    LoadingMessage = "";
                    UpdateDialogState();
                };
                liveloxApiClient.GetImportableEvent(importableEventId, callback);

                // AI generated code not work like this, as it continues immediately
                /*
                var call = await Task.Run(() => liveloxApiClient.GetImportableEvent(importableEventId, null));

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
                    existingImportableEvent = null;
                }
                else if (call?.Exception is OAuth2Exception)
                {
                    // Authorization problem - remove user
                    settings.Users = settings.Users.Skip(1).ToArray();
                    settingsProvider.SaveSettings(settings);
                }
                */
            }
            catch (Exception)
            {
                IsLoading = false;
                LoadingMessage = "";
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

                ShowExistingEvent = true;
            }
        }

        private void UpdateDialogState()
        {
            Resolution = PublishSettings.GetResolution(controller.MapScale)
                .ToString(CultureInfo.CurrentCulture);

            ShowUserPanel = AvailableUsers.Count > 0;
            ShowExistingEvent = existingImportableEvent?.ImportedEvent != null;
            ShowPublishButton = !ShowExistingEvent;
            ShowUpdateButton = ShowExistingEvent;
            CanPublish = !IsLoading;
        }

        [RelayCommand]
        private void Cancel()
        {
            Abort();
            // TODO: How to reach Dialog window and Close if from here ?
            //_dialog.Close(false);
        }

        [RelayCommand]
        public async Task PublishAsync()
        {
            var selectedUser = GetSelectedUser();
            if (selectedUser == null)
            {
                await RequestConsentAsync(user => PublishToLiveloxAsync(user));
            }
            else
            {
                await PublishToLiveloxAsync(selectedUser);
            }
        }

        [RelayCommand]
        public async Task UpdateEventAsync()
        {
            var selectedUser = GetSelectedUser();
            if (selectedUser == null)
            {
                await RequestConsentAsync(user => UpdateLiveloxEventAsync(user));
            }
            else
            {
                await UpdateLiveloxEventAsync(selectedUser);
            }
        }

        private async Task RequestConsentAsync(Func<User, Task> nextStep)
        {
            //TODO: ConsentRedirectionDialog should be shown before this dialog waiting for user consent and vm UserConsented value should be get from that dialog
            var consentRedirectVm = new ConsentRedirectionDialogViewModel();

            // For now, we'll assume the dialog shows and returns a result
            consentRedirectVm.UserConsented = true;

            if (!consentRedirectVm.UserConsented)
            {
                return;
            }

            var refreshTokenLifeLength = consentRedirectVm.RememberConsent
                ? (TimeSpan?)null
                : TimeSpan.FromHours(1);

            IsLoading = true;
            LoadingMessage = LiveloxResources.RedirectingToLivelox;

            var liveloxApiClient = CreateLiveloxApiClient(null);

            try
            {
                User? user = null; // This will be obtained from OAuth flow

                Action activateAppCallback = () =>
                {
                    // Do nothing for now; in a real application, this might bring the app to the foreground
                };

                // TODO: Show OAuth dialog and get user
                Action<LiveloxApiCall<User>> callback = call =>
                {
                    if (call != null)
                    {
                        user = call.Result;
                        if (consentRedirectVm.RememberConsent)
                        {
                            var settings = settingsProvider.LoadSettings();
                            settings.Users = new[] { user }
                                .Concat(settings.Users.Where(o => o.PersonId != user.PersonId))
                                .ToArray();
                            settingsProvider.SaveSettings(settings);
                        }

                        _ = nextStep(user);
                    }
                };

                Action<String> progressinfo = call =>
                {
                    if (call != null)
                    {
                        LoadingMessage = call;
                    }
                };

                // Show OAuth dialog / Browser
                liveloxApiClient.AskForUserConsent(activateAppCallback, refreshTokenLifeLength, callback, progressinfo);

                IsLoading = false;
                /*
                // AI generated code not work like this, as it continues immediately

                await Task.Run(() => liveloxApiClient.AskForUserConsent(activateAppCallback, refreshTokenLifeLength, callback, progressinfo));
                if (user != null)
                {
                    if (consentRedirectVm.RememberConsent)
                    {
                        var settings = settingsProvider.LoadSettings();
                        settings.Users = new[] { user }
                            .Concat(settings.Users.Where(o => o.PersonId != user.PersonId))
                            .ToArray();
                        settingsProvider.SaveSettings(settings);
                    }

                    await nextStep(user);
                }
                */

            }
            catch (Exception)
            {
                IsLoading = false;
                throw;
            }
        }

        private async Task PublishToLiveloxAsync(User user)
        {
            var manager = new PublishManager();
            string? temporaryDirectory = null;

            try
            {
                IsLoading = true;
                LoadingMessage = LiveloxResources.AssemblingCourseSettingInformation;
                CanPublish = false;

                UpdateSettingsFromUI();
                temporaryDirectory = manager.CreateTemporaryDirectory();
                var importableEvent = manager.CreateImportableEvent(
                    controller, symbolDB,
                    PublishSettings.GetResolution(controller.MapScale),
                    temporaryDirectory);

                LoadingMessage = LiveloxResources.UploadingCourseSettingInformation;
                var liveloxApiClient = CreateLiveloxApiClient(user.TokenInformation);

                Action<LiveloxApiCall<ImportableEventLink>> callback = createCall =>
                {
                    if (!createCall.Success)
                    {
                        throw createCall.Exception ?? new Exception("Failed to create importable event");
                    }

                    var importableEventLink = createCall.Result;

                    PersistUserList(user);

                    // zip all files and upload them
                    var zipBytes = CreateZipFileBytes(temporaryDirectory, importableEvent);

                    Action<LiveloxApiCall<LiveloxApiNullResponse>> uploadCallback = uploadFilesCall =>
                    {
                        if (!uploadFilesCall.Success)
                        {
                            throw uploadFilesCall.Exception ?? new Exception("Failed to upload files");
                        }

                        PersistLiveloxEventIdToDB(importableEventLink.Id);

                        if (temporaryDirectory != null)
                        {
                            var manager2 = new PublishManager();
                            manager2.DeleteTemporatyDirectory(temporaryDirectory);
                        }
                        IsLoading = false;
                        CanPublish = true;

                        // Show imported event in Livelox
                        LoadingMessage = LiveloxResources.ImportableEventCreatedInformation;
                        // TODO: Show OK dialog
                        //await InfoMessage(LiveloxResources.ImportableEventUpdatedInformation);
                        ShowUrlInBrowser(importableEventLink.LiveloxImportEventUrl);
                        //Services.WebsiteLauncher.ShowWebsite(importableEventLink.LiveloxImportEventUrl);

                        // TODO: How to reach PublishToLiveloxDialog window and Close if from here?
                    };
                    liveloxApiClient.UploadFile(importableEventLink.Id, "files.zip", zipBytes, uploadCallback);
                };
                liveloxApiClient.CreateImportableEvent(importableEvent, callback);

                /*
                // AI generated code not work like this, as it continues immediately
                var createCall = await Task.Run(() => liveloxApiClient.CreateImportableEvent(importableEvent, callback));
                
                if (!createCall.Success)
                {
                    throw createCall.Exception ?? new Exception("Failed to create importable event");
                }

                var importableEventLink = createCall.Result;
                var zipBytes = CreateZipFileBytes(temporaryDirectory, importableEvent);

                var uploadCall = await Task.Run(() =>
                    liveloxApiClient.UploadFile(importableEventLink.Id, "files.zip", zipBytes, null));

                if (!uploadCall.Success)
                {
                    throw uploadCall.Exception ?? new Exception("Failed to upload files");
                }

                PersistUserList(user);

                if (importableEventLink.LiveloxImportEventUrl != null)
                {
                    PersistLiveloxEventIdToDB(importableEventLink.Id);

                    // Show success dialog and open browser
                    LoadingMessage = LiveloxResources.ImportableEventCreatedInformation;

                    // show import user interface in Livelox in browser
                    ShowUrlInBrowser(importableEventLink.LiveloxImportEventUrl);
                    //await Services.WebsiteLauncher.ShowWebsite(importableEventLink.LiveloxImportEventUrl);
                }
                */
            }
            catch (Exception)
            {
                if (temporaryDirectory != null)
                {
                    var manager2 = new PublishManager();
                    manager2.DeleteTemporatyDirectory(temporaryDirectory);
                }
                IsLoading = false;
                CanPublish = false;
                throw;
            }
        }

        private async Task UpdateLiveloxEventAsync(User user)
        {
            var manager = new PublishManager();
            string? temporaryDirectory = null;

            try
            {
                IsLoading = true;
                LoadingMessage = LiveloxResources.AssemblingCourseSettingInformation;
                CanPublish = false;

                UpdateSettingsFromUI();
                temporaryDirectory = manager.CreateTemporaryDirectory();
                var importableEvent = manager.CreateImportableEvent(
                    controller, symbolDB,
                    PublishSettings.GetResolution(controller.MapScale),
                    temporaryDirectory);

                LoadingMessage = LiveloxResources.UploadingCourseSettingInformation;
                var liveloxApiClient = CreateLiveloxApiClient(user.TokenInformation);
                ImportableEventLink? existingImportableEventLink = existingImportableEvent?.Link;
                string? existingImportableEventLinkId = existingImportableEventLink?.Id;

                Action <LiveloxApiCall<ImportableEventLink>> callback = updateCall =>
                {
                    if (!updateCall.Success)
                    {
                        throw updateCall.Exception ?? new Exception("Failed to update importable event");
                    }

                    var importableEventLink = updateCall.Result;

                    PersistUserList(user);

                    // zip all files and upload them
                    var zipBytes = CreateZipFileBytes(temporaryDirectory, importableEvent);

                    Action<LiveloxApiCall<LiveloxApiNullResponse>> uploadCallback = uploadFilesCall =>
                    {
                        if (!uploadFilesCall.Success)
                        {
                            throw uploadFilesCall.Exception ?? new Exception("Failed to upload files");
                        }

                        // Show success dialog
                        if (importableEventLink.LiveloxImportEventUrl != null)
                        {
                            PersistLiveloxEventIdToDB(importableEventLink.Id);

                            if (temporaryDirectory != null)
                            {
                                var manager2 = new PublishManager();
                                manager2.DeleteTemporatyDirectory(temporaryDirectory);
                            }
                            IsLoading = false;
                            CanPublish = true;

                            // Show imported event in Livelox
                            LoadingMessage = LiveloxResources.ImportableEventCreatedInformation;
                            // TODO: Show OK dialog
                            //await InfoMessage(LiveloxResources.ImportableEventUpdatedInformation);
                            //ShowUrlInBrowser(importableEventLink.LiveloxImportEventUrl);

                            //await Services.WebsiteLauncher.ShowWebsite(importableEventLink.LiveloxImportEventUrl);

                            // TODO: How to reach PublishToLiveloxDialog window and Close if from here?
                        }
                        else
                        {
                            LoadingMessage = LiveloxResources.UpdatingLiveloxEvent;

                            Action<LiveloxApiCall<ImportableEventLink>> importCallback = importImportableEventCall =>
                            {
                                if (!importImportableEventCall.Success)
                                {
                                    throw importImportableEventCall.Exception ?? new Exception("Failed to import event");
                                }

                                importableEventLink = importImportableEventCall.Result;
                                PersistLiveloxEventIdToDB(importableEventLink.Id);

                                if (temporaryDirectory != null)
                                {
                                    var manager2 = new PublishManager();
                                    manager2.DeleteTemporatyDirectory(temporaryDirectory);
                                }
                                IsLoading = false;
                                CanPublish = true;

                                // Show updated event in Livelox
                                LoadingMessage = LiveloxResources.ImportableEventUpdatedInformation;
                                // TODO: Yes/No dialog to show import user interface in Livelox in browser
                                //if (YesNoQuestion(LiveloxResources.ImportableEventUpdatedInformation, true))
                                //{
                                    ShowUrlInBrowser(importableEventLink.LiveloxEditEventUrl);
                                    //await Services.WebsiteLauncher.ShowWebsite(importableEventLink.LiveloxEditEventUrl);
                                //}

                                // TODO: How to reach PublishToLiveloxDialog window and Close if from here?
                            };
                            liveloxApiClient.ImportImportableEvent(importableEventLink.Id, importCallback);
                        }
                    };
                    liveloxApiClient.UploadFile(importableEventLink.Id, "files.zip", zipBytes, uploadCallback);
                };
                _ = liveloxApiClient.UpdateImportableEvent(existingImportableEventLinkId, importableEvent, callback);

                /*
                // AI generated code not work like this, as it continues immediately
                var updateCall = await Task.Run(() =>
                    liveloxApiClient.UpdateImportableEvent(
                        existingImportableEvent.Link.Id, importableEvent, null));

                if (!updateCall.Success)
                {
                    throw updateCall.Exception ?? new Exception("Failed to update importable event");
                }

                var importableEventLink = updateCall.Result;
                var zipBytes = CreateZipFileBytes(temporaryDirectory, importableEvent);

                var uploadCall = await Task.Run(() =>
                    liveloxApiClient.UploadFile(importableEventLink.Id, "files.zip", zipBytes, null));

                if (!uploadCall.Success)
                {
                    throw uploadCall.Exception ?? new Exception("Failed to upload files");
                }

                PersistUserList(user);

                if (importableEventLink.LiveloxImportEventUrl != null)
                {
                    PersistLiveloxEventIdToDB(importableEventLink.Id);
                }
                else
                {
                    LoadingMessage = LiveloxResources.UpdatingLiveloxEvent;
                    var importCall = await Task.Run(() =>
                        liveloxApiClient.ImportImportableEvent(
                            existingImportableEvent.Link.Id, null));

                    if (!importCall.Success)
                    {
                        throw importCall.Exception ?? new Exception("Failed to import event");
                    }

                    importableEventLink = importCall.Result;
                    PersistLiveloxEventIdToDB(importableEventLink.Id);
                }
                */
            }
            catch (Exception)
            {
                if (temporaryDirectory != null)
                {
                    var manager2 = new PublishManager();
                    manager2.DeleteTemporatyDirectory(temporaryDirectory);
                }
                IsLoading = false;
                CanPublish = false;
                throw;
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

        public static void InfoMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
            {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Information
            };
            _ = Services.DialogService.ShowDialogAsync(vm);
            //var success = Dispatcher.UIThread.InvokeAsync(() => {
            //    return Services.DialogService.ShowDialogAsync(vm);
            //});
        }

        public static bool YesNoQuestion(string message, bool yesDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel
            {
                Message = message,
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = yesDefault ? MessageBoxButton.Yes : MessageBoxButton.No,
                Icon = MessageBoxIcon.Question
            };
            _ = Services.DialogService.ShowDialogAsync(vm);
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