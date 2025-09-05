using UnityEngine.AddressableAssets;

namespace PT.ContentManager
{
    /// <summary>
    /// This class is designed to encapsulate the load function of Addressables so that, if we want to switch to a new system later, our loading logic remains centralized.
    /// It also centralizes our interaction with the Addressables loading API, making it easier to update the Unity version and reflect changes in one place.
    /// </summary>
    public class AddressableLoader
    {
        public PTAssetAsyncOperationHandler<T> LoadAssetAsync<T>(string key)
        {
            var operationHandle = Addressables.LoadAssetAsync<T>(key);
            PTAssetAsyncOperationHandler<T> handler = new PTAssetAsyncOperationHandler<T>(operationHandle);
            return handler;
        }
    }
}