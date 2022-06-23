using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class CListener 
    {
        public GameObject Target { get; set; }
        public string Key { get; set; }
        public string FunctionName { get; set; }
        
        public CListener(GameObject target, string key, string functionName)
        {
            this.Target = target;
            this.Key = key;
            this.FunctionName = functionName;
        }
    }

    public class CHandler
    {
        public object Target { get; set; }
        public string Key { get; set;}
        public Action<object> Callback { get; set; }

        public CHandler(object target, string key, Action<object> callback)
        {
            this.Target = target;
            this.Key = key;
            this.Callback = callback;
        }
    }

    public class CNotification
    {
        public string Result { get; set; }
        public object Data { get; set; }
        
        public CNotification(string result = "", object data = null)
        {
            Result = result;
            Data = data;
        }
    }
    
    public class CNotificationCenter 
    {
        private static CNotificationCenter instance;

        private List<CHandler> handlers = new List<CHandler>();

        private System.Object thisLock = new System.Object();

        public static CNotificationCenter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CNotificationCenter();
                    MainThreadDispatcher.Initialize();
                }
                
                return instance;
            }
        }
        
        private CNotificationCenter() {}
        
        public void AddHandler(object target, string key, Action<object> callback)
        {
            if (IsExistHandler(target, key, callback))
            {
                return;
            }
            
            handlers.Add(new CHandler(target, key, callback));
        }

        private class _NotificationRemover:MonoBehaviour
        {			
            public event System.Action DisposeEvent;
            void OnDestroy()
            {
                DisposeEvent?.Invoke();
            }
        }

        public void AddHandlerWithAutoDisposal(MonoBehaviour target, string key, Action<object> callback)
        {
            if (IsExistHandler(target, key, callback))
            {
                return;
            }
            handlers.Add(new CHandler(target,key,callback));

            //MonoBehaviour의 OnDestroy가 불렸을때 자동으로 제거해주도록 컴퍼넌트 attach.
            _NotificationRemover autoRemover = target.GetComponent<_NotificationRemover>();
            if (autoRemover == null)
            {
                autoRemover = target.gameObject.AddComponent<_NotificationRemover>();
            }
            autoRemover.DisposeEvent += ()=>{
                this.RemoveHandler(target,key);
            };
        }

        public int RemoveHandler(object target)
        {
            int count = 0;
            if (target == null)
            {
                CheckHandlerObject();
                return count;
            }

            foreach (var f in handlers.FindAll(l => l.Target == target))  
            {
                count++;
                handlers.Remove(f);
            }

            return count;
        }

        public int RemoveHandler(object target, string key)
        {
            int count = 0;
            if (target == null)
            {
                CheckHandlerObject();
                return count;
            }

            foreach (var f in handlers.FindAll(l => l.Target == target && l.Key.Equals(key)))  
            {
                count++;
                handlers.Remove(f);
            }

            return count;
        }

        public int RemoveHandler(object target, string key, Action<object> callback)
        {
            int count = 0;
            if (target == null)
            {
                CheckHandlerObject();
                return count;
            }

            foreach (var f in handlers.FindAll(l => l.Target == target && l.Key.Equals(key) && l.Callback == callback))  
            {
                count++;
                handlers.Remove(f);
            }

            return count;
        }

        public IEnumerator WaitForNotify(object target, string key)
        {
            bool isNotificationReceived = false;
            object arg = null;
            if (target == null)
            {
                target = new object();
            }
            AddHandler(target,key,obj=>{
                isNotificationReceived = true;
                arg = obj;
            });
            while(isNotificationReceived == false)
            {
                yield return null;
            }
            RemoveHandler(target);
        }

        public async Task WaitForNotifyTask(object target, string key, System.Threading.CancellationToken? token = null)
        {
            bool isNotificationReceived = false;
            object arg = null;
            if (target == null)
            {
                target = new object();
            }
            AddHandler(target,key,obj=>{
                isNotificationReceived = true;
                arg = obj;
            });
            while(isNotificationReceived == false)
            {
                if (token != null && token.Value.IsCancellationRequested)
                {
                    break;
                }
                await UniTask.Yield();
            }
            RemoveHandler(target);
        }

        private void CheckHandlerObject()
        {
            CLogger.Log("CheckHandlerObject()");
            if (handlers != null && handlers.Count > 0)
            {
                for (int i = (handlers.Count - 1); i >= 0 ; i--)
                {
                    CHandler item = handlers[i];
                    if (item == null)
                    {
                        handlers.RemoveAt(i);
                        continue;
                    }
                    
                    if (item.Target == null)
                    {
                        handlers.RemoveAt(i);
                        continue;
                    }
                    
                    if (item.Callback == null)
                    {
                        handlers.RemoveAt(i);
                    }
                }
            }
        }

        public void PostNotification(string key, object args = null, bool uiThread = true)
        {
            lock (thisLock)
            {
                foreach (var f in handlers.FindAll(l => l.Key.Equals(key)))  
                {
                    if (f.Callback == null)
                    {
                        handlers.Remove(f);
                        continue;
                    }
                    if (uiThread)
                    {
                        MainThreadDispatcher.Post(_=>f.Callback(args), null);
                    }
                    else
                    {
                        f.Callback(args);
                    }
                }
            }
        }

        private bool IsExistHandler(object target, string key, Action<object> callback)
        {
            List<CHandler> temp = handlers.FindAll (l => l.Target == target && l.Key.Equals(key) && l.Callback == callback);

            if (temp != null && temp.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
