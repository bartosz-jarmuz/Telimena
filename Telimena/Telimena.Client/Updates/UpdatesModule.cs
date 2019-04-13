using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    /// <inheritdoc />
    public class UpdatesModule : IUpdatesModule
    {
        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="telimena"></param>
        public UpdatesModule(Telimena telimena)
        {
            this.telimena = telimena;
        }

        private readonly Telimena telimena;

        /// <inheritdoc />
        public async Task<UpdateCheckResult> CheckForUpdatesAsync(bool acceptBeta = true)
        {
            UpdateRequest updateRequest = null;

            try
            {
                TelemetryInitializeResponse result = await this.telimena.InitializeIfNeeded().ConfigureAwait(false);
                if (result.Exception != null)
                {
                    throw result.Exception;
                }

                string updaterVersion = this.GetUpdaterVersion();
                updateRequest = new UpdateRequest(this.telimena.Properties.TelemetryKey, this.telimena.Properties.ProgramVersion, this.telimena.Properties.LiveProgramInfo.UserId, acceptBeta
                    , this.telimena.Properties.TelimenaVersion, updaterVersion);

                UpdateResponse temp = await
                    this.GetUpdateResponse(ApiRoutes.ProgramUpdateCheck, updateRequest).ConfigureAwait(false);

                ConfiguredTaskAwaitable<UpdateResponse> programUpdateTask = this
                    .GetUpdateResponse(ApiRoutes.ProgramUpdateCheck, updateRequest).ConfigureAwait(false);
                ConfiguredTaskAwaitable<UpdateResponse> updaterUpdateTask = this
                    .GetUpdateResponse(ApiRoutes.UpdaterUpdateCheck, updateRequest).ConfigureAwait(false);

                UpdateResponse programUpdateResponse = await programUpdateTask;
                UpdateResponse updaterUpdateResponse = await updaterUpdateTask;

                return new UpdateCheckResult
                {
                    ProgramUpdatesToInstall = programUpdateResponse.UpdatePackages
                    ,
                    UpdaterUpdate = updaterUpdateResponse?.UpdatePackages?.FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while sending check for updates request", this.telimena.Properties, ex
                    , new KeyValuePair<Type, object>(typeof(UpdateRequest), updateRequest)
                    , new KeyValuePair<Type, object>(typeof(UpdateRequest), updateRequest));
                if (!this.telimena.Properties.SuppressAllErrors)
                {
                    throw exception;
                }

                return new UpdateCheckResult { Exception = exception };
            }
        }

        /// <inheritdoc />
        public async Task<UpdateCheckResult> HandleUpdatesAsync(bool acceptBeta)
        {
            try
            {
                UpdateCheckResult checkResult = await this.CheckForUpdatesAsync(acceptBeta).ConfigureAwait(false);
                if (checkResult.Exception == null)
                {
                    UpdateHandler handler = new UpdateHandler(this.telimena.Messenger, this.telimena.Properties.LiveProgramInfo, new DefaultWpfInputReceiver()
                        , new UpdateInstaller(), this.telimena.Locator);
                    await handler.HandleUpdates(true, checkResult.ProgramUpdatesToInstall, checkResult.UpdaterUpdate).ConfigureAwait(false);
                }
                else
                {
                    throw checkResult.Exception;
                }

                return checkResult;
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while handling updates", this.telimena.Properties, ex);
                if (!this.telimena.Properties.SuppressAllErrors)
                {
                    throw exception;
                }

                return new UpdateCheckResult { Exception = exception };
            }
        }

        /// <summary>
        ///     Gets the update response.
        /// </summary>
        /// <returns>Task&lt;UpdateResponse&gt;.</returns>
        private Task<UpdateResponse> GetUpdateResponse(string requestUri, UpdateRequest request)
        {
            return this.telimena.Messenger.SendPostRequest<UpdateResponse>(requestUri, request);
        }

        internal string GetUpdaterVersion()
        {
            FileInfo updaterFile = this.telimena.Locator.GetUpdater(this.telimena.Properties.LiveProgramInfo);
            if (updaterFile.Exists)
            {
                string version = TelimenaVersionReader.ReadToolkitVersion(updaterFile.FullName);
                return string.IsNullOrEmpty(version) ? "0.0.0.0" : version;
            }

            return "0.0.0.0";
        }
        
        /// <inheritdoc />
        public UpdateCheckResult CheckForUpdates()
        {
            return Task.Run(() => this.CheckForUpdatesAsync()).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public void InstallUpdates(UpdateCheckResult checkResult, bool acceptBeta)
        {
            Task.Run(() => this.InstallUpdatesAsync(checkResult, acceptBeta)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public Task InstallUpdatesAsync(UpdateCheckResult checkResult, bool takeBeta)
        {
            try
            {
                if (!takeBeta)
                {
                    checkResult.ProgramUpdatesToInstall = checkResult.ProgramUpdatesToInstall.Where(x => x.IsBeta == false).ToList();
                }
                UpdateHandler handler = new UpdateHandler(this.telimena.Messenger, this.telimena.Properties.LiveProgramInfo, new DefaultWpfInputReceiver()
                    , new UpdateInstaller(), this.telimena.Locator);
                return handler.HandleUpdates(false, checkResult.ProgramUpdatesToInstall, checkResult.UpdaterUpdate);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while installing updates", this.telimena.Properties, ex);
                if (!this.telimena.Properties.SuppressAllErrors)
                {
                    throw exception;
                }

                return Task.FromResult(false);
            }
        
        }

        /// <inheritdoc />
        public UpdateCheckResult HandleUpdates(bool acceptBeta)
        {
            return Task.Run(() => this.HandleUpdatesAsync(acceptBeta)).GetAwaiter().GetResult();
        }
    }


    /// <summary>
    /// How should the 'update opportunity' be communicated to the user
    /// </summary>
    public enum UserConfirmationModes
    {
        /// <summary>
        /// Present the 'there is update available, would you like to continue' notification before downloading the update packages.
        /// <para>This is good for quick updates right after app start. User 'starts the update' before they start using the app, and they need to wait for the download to complete</para>
        /// </summary>
        AskBeforeDownload,

        /// <summary>
        /// Download the update packages before presenting the 'there is update available, would you like to continue' notification.
        /// <para>This is good for large updates. The User don't need to wait for the download to complete, however the update interrupts their work.</para>
        /// </summary>
        AskAfterDownload,

        /// <summary>
        /// The user is not given a choice at all.
        /// </summary>
        DontAsk
    }
}