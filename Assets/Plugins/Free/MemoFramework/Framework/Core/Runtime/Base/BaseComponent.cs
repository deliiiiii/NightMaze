using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MemoFramework
{
    /// <summary>
    /// 基础组件，用于掌控游戏的基础设置
    /// </summary>
    public class BaseComponent : MemoFrameworkComponent
    {
        public int MaxFrameRate
        {
            get => Application.targetFrameRate;
            set => Application.targetFrameRate = value;
        }

        public float GameSpeed
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            MFLogger.SetLogHelper(new DefaultLogger());
            UniTaskScheduler.UnobservedTaskException +=
                ex => MFLogger.LogError(MFUtils.Text.Format("执行UniTask时发生异常：{0}\nStackTrace:\n{1}", ex.Message,
                    ex.StackTrace));
        }

        private void Start()
        {
            MaxFrameRate = 165;
            GameSpeed = 1;
        }
    }
}