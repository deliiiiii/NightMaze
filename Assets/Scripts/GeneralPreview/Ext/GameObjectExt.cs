using System.Diagnostics;
using System.Threading;
using General;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GeneralPreview;
[DebuggerStepThrough]
public static class GameObjectExt
{
    extension(GameObject self)
    {
        public void SetActiveTrue() => self.SetActive(true);
        public void SetActiveFalse() => self.SetActive(false);
        public void SetActiveReverse() => self.SetActive(!self.activeSelf);
        public MyOption<T> MyGetCom<T>() where T : Component
        {
            var com = self.GetComponent<T>();
            return com != null ? com : None;
        }
        public T GetOrAddCom<T>() where T : Component => self.GetComponent<T>() ?? self.AddComponent<T>();
    }

    extension(Component self)
    {
        public void SetActiveTrue() => self.gameObject.SetActive(true);
        public void SetActiveFalse() => self.gameObject.SetActive(false);
        public T GetOrAddCom<T>() where T : Component => self.GetComponent<T>() ?? self.gameObject.AddComponent<T>();
    }
    
    extension(MonoBehaviour self)
    {
        public void BindEvtTrg(EventTriggerType type, UnityAction<BaseEventData> callback, CancellationToken? ct = null)
        {
            if (self.GetComponent<Collider2D>() == null)
            {
                MyDebug.LogError($"{self.name} 必须拥有2D碰撞体, 才能绑定eventData事件.");
                return;
            }
            ct ??= self.destroyCancellationToken;
            var trigger = self.gameObject.GetOrAddCom<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(callback);
            ct.Value.Register(() => trigger.triggers.Remove(entry));
            trigger.triggers.Add(entry);
        }
    }
}