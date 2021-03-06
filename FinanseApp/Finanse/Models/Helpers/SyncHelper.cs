﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Finanse.DataAccessLayer;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Services.OneDrive;

namespace Finanse.Models.Helpers {
    public class SyncHelper {
        public static async void CheckSyncBase() {
            try {
                if (!(ConnectedToInternet() && Settings.SyncData))
                    return;

                OneDriveService.Instance.Initialize(OneDriveScopes.AppFolder);

                if (!await OneDriveService.Instance.LoginAsync())
                    throw new Exception("Unable to sign in");

                var stackMoneyFolder = await OneDriveService.Instance.AppRootFolderAsync();

                while (true) {
                    if (ConnectedToInternet() && Settings.SyncData) {
                        var selectedFile = await StorageFile.GetFileFromPathAsync(DalBase.DbPath);
                        if (selectedFile != null) {

                            try {
                                var file = await stackMoneyFolder.GetFileAsync("db.sqlite");

                                StorageFile myLocalFile;

                                using (var remoteStream = await file.OpenAsync()) {
                                    byte[] buffer = new byte[remoteStream.Size];
                                    var localBuffer = await remoteStream.ReadAsync(buffer.AsBuffer(),
                                        (uint) remoteStream.Size, InputStreamOptions.ReadAhead);
                                    var localFolder = ApplicationData.Current.LocalFolder;
                                    myLocalFile = await localFolder.CreateFileAsync(DalBase.DbOneDriveName,
                                        CreationCollisionOption.ReplaceExisting);
                                    using (var localStream = await myLocalFile.OpenAsync(FileAccessMode.ReadWrite)) {
                                        await localStream.WriteAsync(localBuffer);
                                        await localStream.FlushAsync();
                                    }
                                }

                                SyncDal.UpdateBase(DalBase.DbOneDriveName);
                                await myLocalFile.DeleteAsync();
                            }
                            catch (ServiceException e) {
                                Debug.WriteLine("Can't update database. Message: " + e.Message);
                            }
                            finally {
                                try {
                                    using (var localStream = await selectedFile.OpenReadAsync()) {
                                        await stackMoneyFolder.CreateFileAsync(selectedFile.Name,
                                            CreationCollisionOption.ReplaceExisting, localStream);
                                    }
                                }
                                catch (Exception e) {
                                    Debug.WriteLine("Can't write db to OneDrive. Message: " + e.Message);
                                }
                            }
                        }
                    }
                    await Task.Delay(60000); // 1 minute
                }
            }
            catch (Exception e) {
                Debug.WriteLine("Blad przy synchronizacji bazy. Wiadomosc: " + e.Message);
            }
        }

        public static async Task CopyDbFileFromOneDrive() {
            try {
                if (!(ConnectedToInternet() && Settings.SyncData))
                    return;

                OneDriveService.Instance.Initialize(OneDriveScopes.AppFolder);

                if (!await OneDriveService.Instance.LoginAsync())
                    throw new Exception("Unable to sign in");

                var stackMoneyFolder = await OneDriveService.Instance.AppRootFolderAsync();


                if (!ConnectedToInternet() || !Settings.SyncData)
                    return;

                try {
                    var file = await stackMoneyFolder.GetFileAsync("db.sqlite");

                    using (var remoteStream = await file.OpenAsync()) {
                        byte[] buffer = new byte[remoteStream.Size];
                        var localBuffer = await remoteStream.ReadAsync(buffer.AsBuffer(),
                            (uint) remoteStream.Size, InputStreamOptions.ReadAhead);

                        var localFolder = ApplicationData.Current.LocalFolder;

                        var myLocalFile = await localFolder.CreateFileAsync("db.sqlite",
                            CreationCollisionOption.FailIfExists);

                        using (var localStream = await myLocalFile.OpenAsync(FileAccessMode.ReadWrite)) {
                            await localStream.WriteAsync(localBuffer);
                            await localStream.FlushAsync();
                        }
                    }
                }
                catch (ServiceException e) {
                    Debug.WriteLine("Can't update database. Message: " + e.Message);
                }
                Debug.WriteLine("Load db from cloud completed");
            }
            catch (Exception e) {
                Debug.WriteLine("Nie udalo sie skopiowac bazy z OneDrive. Wiadomosc: " + e.Message);
            }
        }

        private static bool ConnectedToInternet() {
            var internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            var level = internetConnectionProfile?.GetNetworkConnectivityLevel();
            return level == NetworkConnectivityLevel.InternetAccess;
        }
    }
}
