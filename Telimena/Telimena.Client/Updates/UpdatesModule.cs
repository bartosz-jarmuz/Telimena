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
        public async Task<UpdateInstallationResult> HandleUpdatesAsync(bool acceptBeta)
        {
            UpdateCheckResult checkResult = null;
            try
            {
                checkResult = await this.CheckForUpdatesAsync(acceptBeta).ConfigureAwait(false);
                if (checkResult.Exception == null)
                {
                    UpdateHandler handler = new UpdateHandler(this.telimena.Messenger, this.telimena.Properties.LiveProgramInfo, new DefaultWpfInputReceiver()
                        , new UpdateInstaller(), this.telimena.Locator);
                    await handler.HandleUpdates(this.telimena.Properties.UpdatePromptingMode, checkResult.ProgramUpdatesToInstall, checkResult.UpdaterUpdate).ConfigureAwait(false);
                }
                else
                {
                    throw checkResult.Exception;
                }

                return new UpdateInstallationResult()
                {
                    CheckResult = checkResult
                };
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while handling updates", this.telimena.Properties, ex);
                if (!this.telimena.Properties.SuppressAllErrors)
                {
                    throw exception;
                }

                return new UpdateInstallationResult()
                {
                    CheckResult = checkResult
                    ,Exception = exception
                };
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
        public UpdateInstallationResult InstallUpdates(UpdateCheckResult checkResult, bool acceptBeta)
        {
            return Task.Run(() => this.InstallUpdatesAsync(checkResult, acceptBeta)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<UpdateInstallationResult> InstallUpdatesAsync(UpdateCheckResult checkResult, bool takeBeta)
        {
            try
            {
                if (!takeBeta)
                {
                    checkResult.ProgramUpdatesToInstall = checkResult.ProgramUpdatesToInstall.Where(x => x.IsBeta == false).ToList();
                }
                UpdateHandler handler = new UpdateHandler(this.telimena.Messenger, this.telimena.Properties.LiveProgramInfo, new DefaultWpfInputReceiver()
                    , new UpdateInstaller(), this.telimena.Locator);
                await handler.HandleUpdates(UpdatePromptingModes.DontPrompt, checkResult.ProgramUpdatesToInstall, checkResult.UpdaterUpdate).ConfigureAwait(false);

                return new UpdateInstallationResult()
                {
                    CheckResult = checkResult
                };
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while installing updates", this.telimena.Properties, ex);
                if (!this.telimena.Properties.SuppressAllErrors)
                {
                    throw exception;
                }
                return new UpdateInstallationResult()
                {
                    CheckResult = checkResult
                    ,Exception = exception
                };
            }
        
        }

        /// <inheritdoc />
        public UpdateInstallationResult HandleUpdates(bool acceptBeta)
        {
            return Task.Run(() => this.HandleUpdatesAsync(acceptBeta)).GetAwaiter().GetResult();
        }
    }
}