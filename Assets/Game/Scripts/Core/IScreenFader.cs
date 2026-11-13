using Cysharp.Threading.Tasks;

namespace Game.Core
{
    public interface IScreenFader
    {
        float CurrentAlpha { get; }
        UniTask FadeInAsync(float duration = 0.4f);
        UniTask FadeOutAsync(float duration = 0.4f);
        void SetAlpha(float alpha);
    }
}
