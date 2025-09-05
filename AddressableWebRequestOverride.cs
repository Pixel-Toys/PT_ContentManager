using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using WithBuddies.Client.Services;
using WithBuddies.Common;

namespace PT.ContentManager
{
    /// <summary>
    /// Handles custom web request overrides for Unity Addressables, enabling support for DLC URLs.
    /// </summary>
    public class AddressableWebRequestOverride
    {
        public const string DlcURLId = "dlc://";

        private DownloadableContentService _downloadableContentService;

        private ModuleLog _logger = ModuleLog.Create("AddressableWebRequestOverride", false);

        public async Task Initialize()
        {
            while (!CommonServiceFactory.ServiceExists<DownloadableContentService>())
            {
                await Task.Yield();
            }
            
            _downloadableContentService = CommonServiceFactory.GetService<DownloadableContentService>();

            while (!_downloadableContentService.IsContentReady)
            {
                await Task.Yield();
            }

            Addressables.WebRequestOverride = TransformWebRequestForCustomUris;
        }

        private void TransformWebRequestForCustomUris(UnityWebRequest request)
        {
            if (!ShouldProcessURL(request.url))
            {
                _logger.LogWrite($"Request didin't need changing: {request.url}");
                return;
            }
           
            var id = GetIdFromUrl(request.url);

            if (!TryGetContentItemFromId(id, out var contentItem))
            {
                ModuleLog.LogError(
                    $"[{nameof(AddressableManager)}] ContentItem not found: {id} will fail the asset loading process, make sure that content is uploaded with the correct id in catalog.");
                return;
            }

            request.url = contentItem.Url;

            _logger.LogWrite($"Mapped {id} to {request.url}");
        }

        #region Private Methods

        private bool ShouldProcessURL(string url)
        {
            return url.StartsWith(DlcURLId);
        }

        /// <summary>
        /// Extracts the content item ID from a custom DLC URL by removing the "dlc://" prefix and any trailing slash.
        /// </summary>
        private string GetIdFromUrl(string url)
        {
            string id = url.Remove(0, DlcURLId.Length);
            if (id.EndsWith("/"))
            {
                id = id.Remove(id.Length - 1);
            }

            return id;
        }
        
        private bool TryGetContentItemFromId(string id, out ContentItem contentItem)
        {
            contentItem = _downloadableContentService.GetContentManifestItem(id, true);

            return contentItem != null;
        }
        
        #endregion
    }
}