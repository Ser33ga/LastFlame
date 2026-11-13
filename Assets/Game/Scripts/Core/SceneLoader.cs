using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    /// <summary>
    /// Asynchronous scene transition service using UniTask and an injected screen fader.
    /// </summary>
    public sealed class SceneLoader : ISceneLoader
    {
        private readonly IScreenFader _screenFader;
        private bool _isLoading;

        public SceneLoader(IScreenFader screenFader)
        {
            _screenFader = screenFader;
        }

        public string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public UniTask LoadSceneAsync(string sceneName, bool useFader = true, float fadeDuration = 0.35f)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[SceneLoader] Cannot load empty scene name.");
                return UniTask.CompletedTask;
            }

            return LoadInternalAsync(
                () => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single),
                sceneName,
                useFader,
                fadeDuration);
        }

        public UniTask LoadSceneAsync(int sceneBuildIndex, bool useFader = true, float fadeDuration = 0.35f)
        {
            if (sceneBuildIndex < 0)
            {
                Debug.LogError($"[SceneLoader] Invalid scene build index: {sceneBuildIndex}.");
                return UniTask.CompletedTask;
            }

            return LoadInternalAsync(
                () => SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Single),
                $"BuildIndex:{sceneBuildIndex}",
                useFader,
                fadeDuration);
        }

        private async UniTask LoadInternalAsync(
            Func<AsyncOperation> loadOperationFactory,
            string sceneLabel,
            bool useFader,
            float fadeDuration)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Ignored concurrent load request for '{sceneLabel}'.");
                return;
            }

            _isLoading = true;
            try
            {
                if (useFader && _screenFader != null)
                {
                    await _screenFader.FadeOutAsync(fadeDuration);
                }

                AsyncOperation operation = loadOperationFactory();
                if (operation == null)
                {
                    Debug.LogError($"[SceneLoader] Failed to start loading scene '{sceneLabel}'.");
                    return;
                }

                while (!operation.isDone)
                {
                    await UniTask.Yield();
                }

                if (useFader && _screenFader != null)
                {
                    await _screenFader.FadeInAsync(fadeDuration);
                }
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
