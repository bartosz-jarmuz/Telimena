﻿using Microsoft.ApplicationInsights.Extensibility.Implementation;
using RandomFriendlyNameGenerator;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TelimenaClient.Model;
using TelimenaClient.Serializer;

namespace TelimenaClient
{


    internal class UserTrackingController
    {
        private const string UserRandomFileName = "user.random";
        private const string UserGuidFileName = "user.guid";
        private const string TrackingSettingsFileName = "Tracking.json";
        private const string NotYetComputedUser = "NOT YET COMPUTED";
        private readonly TelimenaProperties properties;
        private readonly Locator locator;
        private readonly ITelimenaSerializer serializer;
        private readonly UserInfo initialUserInfo;
        private readonly IRemoteSettingsProvider settingsProvider;

        public UserTrackingSettings Settings { get; private set; }

        public UserTrackingController(TelimenaProperties properties, Locator locator, ITelimenaSerializer serializer, UserInfo initialUserInfo = null)
        {
            this.properties = properties;
            this.locator = locator;
            this.serializer = serializer;
            this.initialUserInfo = initialUserInfo;
            this.settingsProvider = new RemoteSettingsProvider(this.properties.TelemetryApiBaseUrl);
        }

        public async Task LoadUserInfo()
        {
            if (this.properties.UserInfo != null && this.properties.UserInfo.UserIdentifier != NotYetComputedUser)
            {
                return;
            }

            string settingsString = await this.settingsProvider.GetUserTrackingSettings(this.properties.TelemetryKey);

            this.Settings = this.DeserializeAndStoreSettings(settingsString);

            UserInfo userInfo = this.GetUserInfo();
            this.properties.UserInfo = userInfo;
        }

        public UserInfo GetStoredUserInfo()
        {
            UserTrackingSettings storedSettings = this.GetStoredSettings();
            if (storedSettings != null)
            {
                var storedInfo = this.GetStoredUserInfo(storedSettings);
                if (storedInfo != null)
                { 
                    return storedInfo; 
                }    
            }
            return new UserInfo()
            {
                UserIdentifier = NotYetComputedUser
            }; 

        }

        private UserInfo GetUserIdentifier(string fileName, bool shared)
        {
            DirectoryInfo directory = this.locator.GetWorkingDirectory();
            if (shared)
            {
                directory = directory.Parent;
            }
            try
            {
                string filePath = Path.Combine(directory.FullName, fileName);
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath);
                    return new UserInfo() { UserIdentifier = lines.FirstOrDefault(), MachineName = lines.LastOrDefault() };
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }



        private void SetUserIdentifier(UserInfo value, string fileName, bool shared)
        {
            var directory = this.locator.GetWorkingDirectory();
            if (shared)
            {
                directory = directory.Parent;
            }
            try
            {
                string filePath = Path.Combine(directory.FullName, fileName);
                var lines = new string[] { value.UserIdentifier, value.MachineName };

                File.WriteAllLines(filePath, lines);
            }
            catch (Exception e)
            {
                //thats bad, but maybe it will succeed next time. Impact is smaller than throwing an error.
                TelemetryDebugWriter.WriteError($"Error while storing user identifier. Error: {e}");
            }
        }


        private UserTrackingSettings GetStoredSettings()
        {
            try
            {
                string path = Path.Combine(this.locator.GetWorkingDirectory().FullName, TrackingSettingsFileName);
                if (File.Exists(path))
                {
                    var stringified = File.ReadAllText(path);
                    return this.serializer.Deserialize<UserTrackingSettings>(stringified);
                }
            }
            catch (Exception ex)
            {
                TelemetryDebugWriter.WriteError("Error while restoring user tracking settings: "+ ex);
            }
            return null;

        }

        private void StoreSettings(UserTrackingSettings settings)
        {
            try
            {
                string stringified = this.serializer.Serialize(settings);

                string path = Path.Combine(this.locator.GetWorkingDirectory().FullName, TrackingSettingsFileName);
                File.WriteAllText(path, stringified);
            }
            catch (Exception ex)
            {
                TelemetryDebugWriter.WriteError("Error while storing user tracking settings: " + ex);
            }
        }


        private UserInfo GetUserInfo()
        {
            if (this.Settings.UserIdentifierMode == UserIdentifierMode.Custom && this.initialUserInfo != null)
            {
                return this.initialUserInfo;
            }

            UserInfo storedInfo = this.GetStoredUserInfo(this.Settings);
            if (storedInfo != null)
            {
                return storedInfo;
            }
            else
            {
                switch (this.Settings.UserIdentifierMode)
                {
                    case UserIdentifierMode.RandomFriendlyName:
                        UserInfo randomized = new UserInfo()
                        {
                            UserIdentifier = NameGenerator.Identifiers.Get(IdentifierComponents.Adjective | IdentifierComponents.Animal, separator: "_"),
                            MachineName = NameGenerator.Identifiers.Get(IdentifierComponents.Adjective | IdentifierComponents.Noun, separator: "-")
                        };
                        this.SetUserIdentifier(randomized, UserRandomFileName, this.Settings.ShareIdentifierWithOtherTelimenaApps);
                        return randomized;
                    case UserIdentifierMode.AnonymousGUID:
                        UserInfo guidized = new UserInfo()
                        {
                            UserIdentifier = Guid.NewGuid().ToString(),
                            MachineName = Guid.NewGuid().ToString()
                        };
                        this.SetUserIdentifier(guidized, UserGuidFileName, this.Settings.ShareIdentifierWithOtherTelimenaApps);
                        return guidized;
                    case UserIdentifierMode.NoTelemetry:
                        return new UserInfo();
                    case UserIdentifierMode.TrackPersonalData:
                        return new UserInfo { UserIdentifier = Environment.UserName, MachineName = Environment.MachineName };
                    default:
                        return new UserInfo();
                }
            }

        }

        private UserTrackingSettings DeserializeAndStoreSettings(string serializedSettings)
        {
            UserTrackingSettings settings = this.Deserialize(serializedSettings);
            if (settings == null)
            {
                settings = this.GetStoredSettings();
                if (settings == null)
                {
                    if (this.initialUserInfo != null)
                    {
                        settings = new UserTrackingSettings()
                        {
                            ShareIdentifierWithOtherTelimenaApps = false,
                            UserIdentifierMode = UserIdentifierMode.Custom
                        };
                    }
                    else
                    {
                        settings = new UserTrackingSettings()
                        {
                            ShareIdentifierWithOtherTelimenaApps = false,
                            UserIdentifierMode = UserIdentifierMode.AnonymousGUID
                        };
                    }

                }
            }
            else
            {
                this.StoreSettings(settings);
            }

            return settings;
        }

        private UserTrackingSettings Deserialize(string serializedSettings)
        {
            if (string.IsNullOrEmpty(serializedSettings)) {
                return null; 
            }
            try
            {
                
                return this.serializer.Deserialize<UserTrackingSettings>(serializedSettings);
            }
            catch (Exception ex)
            {
                TelemetryDebugWriter.WriteError("Error while deserializing user tracking settings: " + ex);
                return null;
            }
        }

        private UserInfo GetStoredUserInfo(UserTrackingSettings settings)
        {
            switch (settings.UserIdentifierMode)
            {
                case UserIdentifierMode.RandomFriendlyName:
                    UserInfo randomized = this.GetUserIdentifier(UserRandomFileName, settings.ShareIdentifierWithOtherTelimenaApps);
                    if (randomized == null || string.IsNullOrEmpty(randomized.UserIdentifier) || string.IsNullOrEmpty(randomized.MachineName))
                    {
                        return null;
                    }
                    return randomized;
                case UserIdentifierMode.AnonymousGUID:
                    UserInfo guidized = this.GetUserIdentifier(UserGuidFileName, settings.ShareIdentifierWithOtherTelimenaApps);
                    if (guidized == null || string.IsNullOrEmpty(guidized.UserIdentifier) || string.IsNullOrEmpty(guidized.MachineName))
                    {
                        return null;
                    }
                    return guidized;
                case UserIdentifierMode.NoTelemetry:
                    return new UserInfo();
                case UserIdentifierMode.TrackPersonalData:
                    return new UserInfo { UserIdentifier = Environment.UserName, MachineName = Environment.MachineName };
                default:
                    return new UserInfo();
            }
        }

    }
}