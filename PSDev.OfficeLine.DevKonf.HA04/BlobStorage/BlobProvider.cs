#region Copyright (c) 2002-2018, Sage GmbH

//Copyright (c) 2018, Sage GmbH (http://www.sage.de).
//Alle Rechte vorbehalten.
//Weitergabe und Vervielfältigung dieser Moduls oder von Teilen daraus sind, zu welchem Zweck
//und in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die
//Sage GmbH nicht gestattet. In diesem Modul enthaltene Informationen können
//ohne vorherige Ankündigung geändert werden. Die Module werden im Rahmen des Developer Programms
//den Teilnehmern als As-Is Basis zur Verfügung gestellt und stellen keinen Anspruch auf Vollständigkeit dar.

#endregion Copyright (c) 2002-2018, Sage GmbH

using Sagede.Core.ServerEnvironment;
using Sagede.Settings.OfficeLine;
using Sagede.Shared.BlobStorage;
using System;
using System.IO;
using System.Security;

namespace WEKO.BirdHome.Absatzplanungimport
{
    /// <summary>
    /// Klasse kapselt den Zugriff auf den BlobStorageServer
    /// </summary>
    public class BlobProvider
    {
        private readonly string _user;
        private readonly SecureString _password;
        private IContainer _container;

        public BlobProvider(string user, SecureString password)
        {
            _user = user;
            _password = password;
        }

        private IContainer GetContainer()
        {
            try
            {
                if (_container != null) return _container;
                var configurationAccess = ConfigurationService.ConfigurationAccess as ConfigurationAccess;
                if (configurationAccess != null)
                {
                    var serviceRegistryStorage = new ServiceRegistryStorageRemoteRegistry(
                        configurationAccess.RemoteRegistryServerName, configurationAccess.RemoteRegistryDefaultKey);
                    var storage = StorageProvider.GetBlobStorage(serviceRegistryStorage, _user, _password);
                    if (storage != null)
                    {
                        _container = storage.GetContainerReference("Data");
                        if (_container != null)
                        {
                            _container.CreateIfNotExists();
                        }
                        else
                        {
                            TraceLog.Logger.Warning("GetContainer() _container==null");
                        }
                    }
                    else
                    {
                        TraceLog.Logger.Warning("GetContainer() storage==null");
                    }
                }
                else
                {
                    TraceLog.Logger.Warning("GetContainer() configurationAccess==null");
                }
            }
            catch (Exception ex)
            {
                TraceLog.LogException(ex);
                return null;
            }
            return _container;
        }

        /// <summary>
        /// Speicherung des Content als Blob
        /// </summary>
        /// <param name="blobName">Name des Blob-Objektes</param>
        /// <param name="path">Pfad im BlobStorage</param>
        /// <param name="content">Inhalt</param>
        public void SaveBlob(string blobName, string path, byte[] content)
        {
            try
            {
                if (!String.IsNullOrEmpty(path)) blobName = String.Join("/", path, blobName);
                var container = GetContainer();
                if (container == null)
                    throw new Exception("ErrorBlobStorageUnavailable");

                container.GetBlobReference(blobName).DeleteIfExists();
                using (var stream = new MemoryStream(content))
                {
                    container.GetBlobReference(blobName).UploadFromStream(stream);
                    stream.Close();
                    stream.Dispose();
                }
                var blob = container.GetBlobReference(blobName);
                var meta = blob.GetMetadata();
                meta.Public = true;
                blob.SetMetadata(meta);
            }
            catch (Exception ex)
            {
                TraceLog.LogException(ex);
            }
        }

        /// <summary>
        /// Liest einen Blob
        /// </summary>
        /// <param name="blobName">Name des Blob-Objektes</param>
        /// <param name="path">Pfad im BlobStorage</param>
        /// <returns></returns>
        public byte[] GetBlob(string blobName, string path)
        {
            try
            {
                if (!BlobExists(blobName, path)) return null;
                if (!String.IsNullOrEmpty(path)) blobName = String.Join("/", path, blobName);

                return GetBlob(blobName);
            }
            catch (Exception ex)
            {
                TraceLog.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Liest einen Blob
        /// </summary>
        /// <param name="fullBlobName">vollständiger Name des Blob inklusive Pfad</param>
        /// <returns></returns>
        public byte[] GetBlob(string fullBlobName)
        {
            try
            {
                var container = GetContainer();
                if (container == null)
                    throw new Exception("BlobStorage not available");

                using (var stream = new MemoryStream())
                {
                    container.GetBlobReference(fullBlobName).DownloadToStream(stream);
                    var content = stream.ToArray();
                    stream.Close();
                    stream.Dispose();
                    return content;
                }
            }
            catch (Exception ex)
            {
                TraceLog.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Lieft einen Blob als File (not implemented)
        /// </summary>
        /// <param name="fullBlobName"></param>
        /// <returns></returns>
        public string GetBlobAsFile(string fullBlobName)
        {
            try
            {
                var container = GetContainer();
                if (container == null)
                    throw new Exception("ErrorBlobStorageUnavailable");

                var fileName = "test"; //Constants.CreateZugferdTmpFileName();
                if (File.Exists(fileName)) File.Delete(fileName);
                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    container.GetBlobReference(fullBlobName).DownloadToStream(stream);
                    stream.Close();
                    stream.Dispose();
                }
                return fileName;
            }
            catch (Exception ex)
            {
                TraceLog.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Prüft, ob ein Blob-Objekt existiert
        /// </summary>
        /// <param name="blobName">Name des Blob-Objektes</param>
        /// <param name="path">Pfad im BlobStorage</param>
        /// <returns></returns>
        public bool BlobExists(string blobName, string path)
        {
            try
            {
                if (!String.IsNullOrEmpty(path)) blobName = String.Join("/", path, blobName);
                var container = GetContainer();
                if (container == null)
                    throw new Exception("ErrorBlobStorageUnavailable");

                return container.GetBlobReference(blobName).Exists();
            }
            catch (Exception ex)
            {
                TraceLog.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Löscht ein Blob-Objekt
        /// </summary>
        /// <param name="blobName">Name des Blob-Objektes</param>
        /// <param name="path">Pfad im BlobStorage</param>
        /// <returns></returns>
        public bool DeleteBlob(string blobName, string path)
        {
            try
            {
                if (!String.IsNullOrEmpty(path)) blobName = String.Join("/", path, blobName);
                var container = GetContainer();
                if (container == null)
                    throw new Exception("ErrorBlobStorageUnavailable");

                return container.GetBlobReference(blobName).DeleteIfExists();
            }
            catch (Exception ex)
            {
                TraceLog.LogException(ex);
                return false;
            }
        }
    }
}