using System;
using System.Collections.Generic;
using MemoFramework.UI;
using UnityEngine;

namespace MemoFramework
{
    public class UIComponent : MemoFrameworkComponent
    {
        public Canvas RootCanvas;

        public CoroutineAgent CoroutineAgent { get; private set; }

        private List<ViewBase> _allViews = new List<ViewBase>();

        // UI栈相关
        private Stack<ViewBase> _historyViews = new();
        private ViewBase _homeView = null;

        protected override void Awake()
        {
            base.Awake();
            CoroutineAgent = new GameObject("CoroutineAgent").AddComponent<CoroutineAgent>();
            CoroutineAgent.transform.SetParent(transform);
        }

        /// <summary>
        /// 实例化并用Viewmodel初始化一个View
        /// </summary>
        /// <param name="identifier">View的Identifier</param>
        /// <param name="viewPrefab">View的Prefab</param>
        /// <param name="viewModel">View对应的ViewModel</param>
        /// <param name="show">是否立即显现</param>
        /// <returns>加载出的View</returns>
        public ViewBase RegisterView(string identifier, GameObject viewPrefab, IViewModel viewModel, bool show)
        {
            if (_allViews.FindIndex(x => x.Identifier == identifier) >= 0)
            {
                MFLogger.LogError("已经注册过Identifier为{0}的View，请勿重复注册", identifier);
                return null;
            }

            ViewBase view = Instantiate(viewPrefab, RootCanvas.transform).GetComponent<ViewBase>();
            if (view is null)
            {
                MFLogger.LogError($"注册的View[{viewPrefab.name}]上未挂载继承自ViewBase的组件");
                return null;
            }

            view.Identifier = identifier;
            _allViews.Add(view);
            view.Init(viewModel);
            view.Show = false;
            view.Visibility = false;
            if (show)
            {
                view.ShowView();
            }

            return view;
        }

        public void UnRegisterView(string identifier)
        {
            ViewBase view = GetView(identifier);
            if (view is null)
            {
                MFLogger.LogError($"未找到Identifier为{identifier}的View");
                return;
            }
            view.Unregister();
            _allViews.Remove(view);
            Destroy(view.gameObject);
        }

        public void UnRegisterView(ViewBase view)
        {
            if (view is null) return;
            view.Unregister();
            _allViews.Remove(view);
            Destroy(view.gameObject);
        }

        public void HideAllViews()
        {
            for (int i = _allViews.Count - 1; i >= 0; i--)
            {
                _allViews[i].HideView();
            }
        }

        /// <summary>
        /// 显示一个View
        /// </summary>
        /// <param name="identifier">View的资源Identifier</param>
        public void ShowView(string identifier)
        {
            var view = GetView(identifier);
            if (view is null)
            {
                MFLogger.LogError($"未找到Identifier为{identifier}的View");
                return;
            }

            view.ShowView();
        }

        /// <summary>
        /// 根据Identifier获取匹配的第一个View
        /// </summary>
        /// <param name="identifier">View的Identifier</param>
        /// <returns>找到的View</returns>
        public ViewBase GetView(string identifier)
        {
            foreach (var view in _allViews)
            {
                if (view.Identifier == identifier) return view;
            }

            MFLogger.LogError($"未找到Identifier为{identifier}的View");
            return null;
        }

        /// <summary>
        /// 切换到一个已开启的View
        /// 建议用ViewBase的实例调用该方法
        /// </summary>
        /// <param name="self">当前界面</param>
        /// <param name="identifier">待开启的View的Identifier</param>
        /// <param name="recordHistory">是否记录到UI栈中以便返回</param>
        public void NavigateTo(ViewBase self, string identifier, bool recordHistory = false)
        {
            ViewBase target = GetView(identifier);
            if (target is null)
            {
                MFLogger.LogError(MFUtils.Text.Format("没有找到Identifier为{0}的View，请先注册", identifier));
                return;
            }

            if (recordHistory) _historyViews.Push(self);
            self.HideView();
            target.ShowView();
        }

        /// <summary>
        /// 设置Home界面，GoHome时会返回到此界面
        /// </summary>
        /// <param name="home">Home界面</param>
        public void SetHome(ViewBase home)
        {
            _homeView = home;
        }

        /// <summary>
        /// 返回上一级View，历史UI栈在Navigate到其他界面时会自动记录
        /// 建议用ViewBase的实例调用该方法
        /// </summary>
        /// <param name="current">当前View</param>
        public void GoBack(ViewBase current)
        {
            if (_historyViews.Count <= 0 || current == _homeView) return;
            ViewBase last = _historyViews.Pop();
            current.HideView();
            last.ShowView();
        }

        /// <summary>
        /// 返回设置的Home界面
        /// 建议用ViewBase的实例调用该方法
        /// </summary>
        /// <param name="current">当前View</param>
        public void GoHome(ViewBase current)
        {
            if (_homeView is null)
            {
                MFLogger.LogError("请先设置HomeView");
                return;
            }

            _historyViews.Clear();
            current.HideView();
            _homeView.ShowView();
        }
    }
}