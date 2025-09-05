using System.Collections.Generic;
using System.Threading.Tasks;

namespace PT.ContentManager
{
    public class AddressableExternalCatalogManager
    {
        private Dictionary<string, CatalogLoader> _onDemandCatalogs = new();
        
        public async Task LoadCatalog(string catalogKey)
        {
            while (!AddressableManager.Instance.IsFullyInitialized)
            {
                await Task.Yield();
            }

            if (_onDemandCatalogs.ContainsKey(catalogKey))
            {
                return; // Already loaded.
            }

            CatalogLoader catalogLoader = new CatalogLoader(catalogKey);
            _onDemandCatalogs.Add(catalogLoader.CatalogFileName, catalogLoader);
            await catalogLoader.Load();
        }

        public CatalogLoader GetCatalogLoader(string catalogFileName)
        {
            return _onDemandCatalogs.GetValueOrDefault(catalogFileName);
        }
    }
}