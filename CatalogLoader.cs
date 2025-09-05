using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PT.ContentManager
{
    public class CatalogLoader
    {
        public string CatalogFileName { get; }
        private string CatalogPath => AddressableWebRequestOverride.DlcURLId + CatalogFileName;
        public bool IsLoaded => _resourceLocator != null;

        private IResourceLocator _resourceLocator;

        public CatalogLoader(string catalogFileName)
        {
            CatalogFileName = catalogFileName;
        }

        public async Task Load()
        {
            var catalogHandle = Addressables.LoadContentCatalogAsync(CatalogPath, false);

            catalogHandle.Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _resourceLocator = catalogHandle.Result;
                }

                Addressables.Release(handle);
            };

            await catalogHandle.Task;
        }

        public void Unload()
        {
            if (!IsLoaded)
            {
                return;
            }

            Addressables.RemoveResourceLocator(_resourceLocator);
        }

        public IEnumerable<object> GetAvailableKeys()
        {
            if (_resourceLocator == null)
            {
                return Array.Empty<object>();
            }
            
            return _resourceLocator.Keys;
        }
    }
}
