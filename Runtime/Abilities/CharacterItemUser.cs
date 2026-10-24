using Larje.Character.Abilities;
using Larje.Core.LevelObjects;
using Cysharp.Threading.Tasks;
using System.Threading;
using Larje.Character.AI;
using System;
using System.Collections.Generic;
using Larje.Core;
using Larje.Core.CompositeProperties;
using UnityEngine;
using UnityEngine.Serialization;

namespace Larje.Character
{
    public class CharacterItemUser : CharacterAbility, ILevelObjectDataUser
    {
        [SerializeField] private Transform itemRoot;
        [SerializeField] private string overrideItemLayer;
        [SerializeField] private UsePositionOverride usePositionOverride = UsePositionOverride.None;

        private IObjectInjector _injector;
        private ICursorService _cursorService;

        private Data _data;
        private UsableItem _currentItem;

        public UsableItem CurrentItem => _currentItem;

        public GameObject GameObject => gameObject;
        public System.Type DataType => typeof(Data);

        public PrioritizedProperty<Vector3> OriginPosition;
        public PrioritizedProperty<Vector3> TargetPosition;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ICursorService cursorService, IObjectInjector injector)
        {
            _cursorService = cursorService;
            _injector = injector;
        }

        public void InjectData(object data)
        {
            _data = (Data)data;
            if (!string.IsNullOrEmpty(_data?.currentItemKey))
            {
                SetItem(_data.currentItemKey);
            }
        }

        public object ReadData()
        {
            return _data;
        }

        public void StartAction(int actionId)
        {
            if (!Permitted) return;
            
            _currentItem?.StartAction(actionId);
        }

        public void StopAction(int actionId)
        {
            _currentItem?.StopAction(actionId);
        }
        
        public void SetItem(UsableItem itemPrefab)
        {
            if (itemPrefab != null)
            {
                _data.currentItemKey = itemPrefab.name;
                InstantiateItem(itemPrefab);
            }
        }

#if LARJE_ADDRESSABLES
        public void SetItem(string itemKey)
        {
            if (!string.IsNullOrEmpty(itemKey))
            {
                _data.currentItemKey = itemKey;
                SetItemAsync(itemKey, this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        private async UniTaskVoid SetItemAsync(string itemKey, CancellationToken cancellationToken)
        {
            GameObject loaded = await UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(itemKey).ToUniTask(cancellationToken: cancellationToken);
            if (loaded != null)
            {
                InstantiateItem(loaded.GetComponent<UsableItem>());
            }
        }
#endif

        public void RemoveItem(bool changeKey, Action onRemoved)
        {
            if (changeKey)
            {
                _data.currentItemKey = null;
            }

            if (_currentItem != null)
            {
                _currentItem.Unequip(() =>
                {
                    Destroy(_currentItem.gameObject);
                    onRemoved?.Invoke();
                });
            }
            else
            {
                onRemoved?.Invoke();
            }
        }
        
        protected override void OnInitialized()
        {
            if (_data == null)
            {
                _data = new Data();
                RemoveItem(false, () => {});
            }
        }

        protected virtual void Update()
        {
            itemRoot.gameObject.SetActive(Permitted);
        }

        protected virtual void InstantiateItem(UsableItem itemPrefab)
        {
            RemoveItem(false, () =>
            {
                if (itemPrefab != null)
                {
                    _currentItem = _injector != null
                        ? _injector.Instantiate(itemPrefab.gameObject, itemRoot).GetComponent<UsableItem>()
                        : Instantiate(itemPrefab, itemRoot);
                    _currentItem.Equip();
                    _currentItem.transform.localPosition = Vector3.zero;
                    _currentItem.transform.localRotation = Quaternion.identity;

                    if (usePositionOverride == UsePositionOverride.Camera)
                    {
                        Camera cam = Camera.main;
                        _currentItem.UseOriginPosition = () => true;
                        _currentItem.OriginPosition = () => cam.transform.position;
                        _currentItem.UseTargetPosition = () => true;
                        _currentItem.TargetPosition = () => cam.transform.position + cam.transform.forward * 10f;
                    }
                    else if (usePositionOverride == UsePositionOverride.Cursor && _cursorService != null)
                    {
                        _currentItem.UseOriginPosition = () => true;
                        _currentItem.OriginPosition = () => _cursorService.Origin;
                        _currentItem.UseTargetPosition = () => true;
                        _currentItem.TargetPosition = () => _cursorService.Origin + _cursorService.Direction * 10f;
                    }
                    else if (usePositionOverride == UsePositionOverride.ScriptDriven)
                    {
                        _currentItem.UseOriginPosition = () => OriginPosition.TryGetValue(out _);
                        _currentItem.OriginPosition = () =>
                        {
                            OriginPosition.TryGetValue(out Vector3 originPosition);
                            return originPosition;
                        };
                        _currentItem.UseTargetPosition = () => TargetPosition.TryGetValue(out _);
                        _currentItem.TargetPosition = () =>
                        {
                            TargetPosition.TryGetValue(out Vector3 targetPosition);
                            return targetPosition;
                        };
                    }

                    if (!string.IsNullOrEmpty(overrideItemLayer))
                    {
                        foreach (Transform child in _currentItem.GetComponentsInChildren<Transform>())
                        {
                            child.gameObject.layer = LayerMask.NameToLayer(overrideItemLayer);
                        }
                    }
                }
            });
        }


        public enum UsePositionOverride
        {
            None,
            Camera,
            Cursor,
            ScriptDriven
        }

        [Serializable]
        public class Data
        {
            public string currentItemKey;
        }
    }
}
