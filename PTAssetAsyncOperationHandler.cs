using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PT.ContentManager
{
    /// <summary>
    /// Encapsulates async operation handlers for Addressables, centralizing interaction with the Addressables API.
    /// This design makes it easier to switch to a different loading system or update Unity, as changes are reflected in one place.
    /// </summary>
    public struct PTAssetAsyncOperationHandler<T>
    {
        private readonly AsyncOperationHandle<T> _operationHandle;

        public PTAssetAsyncOperationHandler(AsyncOperationHandle<T> operationHandle)
        {
            _operationHandle = operationHandle;
        }
        
        public T Result => _operationHandle.Result;

        public void GetAssetViaCallBack(Action<T> callback)
        {
            if (_operationHandle.IsDone)
            {
                callback?.Invoke(_operationHandle.Result);
            }

            _operationHandle.Completed += handle => { callback?.Invoke(handle.Result); };
        }

        public async Task<T> GetAssetViaTask()
        {
            if (_operationHandle.IsDone)
            {
                return _operationHandle.Result;
            }

            await _operationHandle.Task;
            return _operationHandle.Result;
        }

        public void Release()
        {
            Addressables.Release(_operationHandle);
        }

        public float GetLoadProgress()
        {
            return _operationHandle.PercentComplete;
        }
    }
}
