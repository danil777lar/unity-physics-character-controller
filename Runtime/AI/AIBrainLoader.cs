using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core;
using Larje.Core.Extensions;
using UnityEngine;

namespace Larje.Character.AI
{
    public sealed class AIBrainLoader : MonoBehaviour
    {
        private IObjectInjector _injector;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IObjectInjector injector)
        {
            _injector = injector;
        }

        public void SetBrain(AIBrain brainPrefab)
        {
            transform.DestroyAllChildren();
            if (brainPrefab == null)
            {
                return;
            }

            GameObject instance = _injector != null
                ? _injector.Instantiate(brainPrefab.gameObject, transform)
                : Instantiate(brainPrefab.gameObject, transform);
            instance.GetComponent<AIBrain>().ResetBrain();
        }

#if LARJE_ADDRESSABLES
        public void SetBrain(string brainKey)
        {
            SetBrainAsync(brainKey, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid SetBrainAsync(string brainKey, CancellationToken cancellationToken)
        {
            GameObject loaded = await UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(brainKey).ToUniTask(cancellationToken: cancellationToken);
            if (loaded != null)
            {
                SetBrain(loaded.GetComponent<AIBrain>());
            }
        }
#endif
    }
}
