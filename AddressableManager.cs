using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PT.ContentManager
{
    public class AddressableManager : Singleton<AddressableManager>
    {
        #region Fields
        
        public bool IsLocalOrFullyInitialized => CurrentState is AddressableState.LocalInitialized or AddressableState.FullyInitialized;
        public bool IsFullyInitialized => CurrentState == AddressableState.FullyInitialized;
        public AddressableState CurrentState { get; set;  } = AddressableState.Uninitialized;
        
        private AddressableWebRequestOverride _addressableWebRequestOverride;
        
        private ModuleLog _logger = ModuleLog.Create("AddressableManager", false);
        
        public AddressableExternalCatalogManager CatalogManager { get; private set; } = new AddressableExternalCatalogManager();


        public AddressableLoader Loader {get; private set; } = new AddressableLoader();

        #endregion

        #region Initialization

        public void Initialize()
        {
            if (CurrentState != AddressableState.Uninitialized)
            {
                return;
            }
            
            InitializeAddressablesWithDLCAsync();
        }

        private async void InitializeAddressablesWithDLCAsync()
        {
             CurrentState = AddressableState.Initializing;
             
             AsyncOperationHandle handle = Addressables.InitializeAsync(false);
             await handle.Task;
             CurrentState = handle.Status == AsyncOperationStatus.Succeeded ? AddressableState.LocalInitialized : AddressableState.Failed;
             Addressables.Release(handle);
             
             if (CurrentState == AddressableState.Failed)
             {
                 _logger.LogWrite("Addressables failed to initialize, check the logs for more details.", WithBuddies.Common.LogLevel.Fatal);
                 return;
             }
             
             _addressableWebRequestOverride = new AddressableWebRequestOverride();
             
             await _addressableWebRequestOverride.Initialize();
          
             CurrentState = AddressableState.FullyInitialized;
        }
        
        #endregion
    }
}