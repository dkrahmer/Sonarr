using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles
{
    public class DownloadedEpisodesCommandService : IExecute<DownloadedEpisodesScanCommand>
    {
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDiskProvider _diskProvider;
        private readonly ICompletedDownloadService _completedDownloadService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DownloadedEpisodesCommandService(IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                                ITrackedDownloadService trackedDownloadService,
                                                IDiskProvider diskProvider,
                                                IConfigService configService,
                                                ICompletedDownloadService completedDownloadService,
                                                Logger logger)
        {
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _trackedDownloadService = trackedDownloadService;
            _diskProvider = diskProvider;
            _completedDownloadService = completedDownloadService;
            _configService = configService;
            _logger = logger;
        }

        private List<ImportResult> ProcessDroneFactoryFolder()
        {
            var downloadedEpisodesFolder = _configService.DownloadedEpisodesFolder;

            if (string.IsNullOrEmpty(downloadedEpisodesFolder))
            {
                _logger.Trace("Drone Factory folder is not configured");
                return new List<ImportResult>();
            }

            if (!_diskProvider.FolderExists(downloadedEpisodesFolder))
            {
                _logger.Warn("Drone Factory folder [{0}] doesn't exist.", downloadedEpisodesFolder);
                return new List<ImportResult>();
            }

            return _downloadedEpisodesImportService.ProcessRootFolder(new DirectoryInfo(downloadedEpisodesFolder));
        }

        private List<ImportResult> ProcessPath(DownloadedEpisodesScanCommand message)
        {
            if (!_diskProvider.FolderExists(message.Path) && !_diskProvider.FileExists(message.Path))
            {
                _logger.Warn("Folder/File specified for import scan [{0}] doesn't exist.", message.Path);
                return new List<ImportResult>();
            }

            if (message.DownloadClientId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(message.DownloadClientId);

                if (trackedDownload != null)
                {
                    _logger.Debug("External directory scan request for known download {0}. [{1}]", message.DownloadClientId, message.Path);

                    var importResults = _downloadedEpisodesImportService.ProcessPath(message.Path, message.ImportMode, trackedDownload.RemoteEpisode.Series, trackedDownload.DownloadItem);

                    _completedDownloadService.VerifyImport(trackedDownload, importResults);

                    return importResults;
                }
                 
                _logger.Warn("External directory scan request for unknown download {0}, attempting normal import. [{1}]", message.DownloadClientId, message.Path);
            }

            return _downloadedEpisodesImportService.ProcessPath(message.Path, message.ImportMode);
        }

        public void Execute(DownloadedEpisodesScanCommand message)
        {
            List<ImportResult> importResults;
            bool isDroneImport = false;

            if (message.Path.IsNotNullOrWhiteSpace())
            {
                importResults = ProcessPath(message);
            }
            else
            {
                importResults = ProcessDroneFactoryFolder();
                isDroneImport = true;
            }

            if (importResults == null || importResults.All(v => v.Result != ImportResultType.Imported))
            {
                // Atm we don't report it as a command failure, coz that would cause the download to be failed.
                _logger.ProgressDebug(isDroneImport ? "Drone Factory did not find anything to import" : "Failed to import");
            }
        }
    }
}
