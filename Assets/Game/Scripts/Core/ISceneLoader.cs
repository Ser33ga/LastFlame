using Cysharp.Threading.Tasks;

namespace Game.Core
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(string sceneName, bool useFader = true, float fadeDuration = 0.35f);
        UniTask LoadSceneAsync(int sceneBuildIndex, bool useFader = true, float fadeDuration = 0.35f);
        string GetActiveSceneName();
    }
}
