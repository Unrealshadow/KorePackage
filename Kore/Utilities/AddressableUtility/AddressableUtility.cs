using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AddressableUtility
{
    private static Dictionary<string, AsyncOperationHandle> loadedAssets = new Dictionary<string, AsyncOperationHandle>();

    public static async Task<T> LoadAddressableAsset<T>(string assetName) where T : Object
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetName);
        await handle.Task;
        loadedAssets[assetName] = handle;
        return handle.Result;
    }

    public static async Task<T> LoadAddressableAssetAsync<T>(string assetName) where T : Object
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetName);
        await handle.Task;
        loadedAssets[assetName] = handle;
        return handle.Result;
    }

    public static async Task PreloadAddressables(IEnumerable<string> assetNames)
    {
        foreach (string assetName in assetNames)
        {
            if (!loadedAssets.ContainsKey(assetName))
            {
                AsyncOperationHandle handle = Addressables.LoadAssetAsync<object>(assetName);
                await handle.Task;
                loadedAssets[assetName] = handle;
            }
        }
    }

    public static T GetFromPool<T>(string assetName) where T : Object
    {
        if (loadedAssets.TryGetValue(assetName, out AsyncOperationHandle handle))
        {
            return handle.Result as T;
        }
        else
        {
            Debug.LogWarning($"No asset with name {assetName} is currently loaded.");
            return null;
        }
    }

    public static Task UnloadAddressable(string assetName)
    {
        if (loadedAssets.TryGetValue(assetName, out AsyncOperationHandle handle))
        {
            loadedAssets.Remove(assetName);
            Addressables.Release(handle);
        }
        else
        {
            Debug.LogWarning($"No asset with name {assetName} is currently loaded.");
        }

        return Task.CompletedTask;
    }

    public static void UnloadAllAddressables()
    {
        foreach (KeyValuePair<string, AsyncOperationHandle> pair in loadedAssets)
        {
            Addressables.Release(pair.Value);
        }
        loadedAssets.Clear();
    }

}
