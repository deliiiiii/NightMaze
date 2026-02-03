using System;
using UnityEngine;
using UnityEngine.UI;

namespace MemoFramework.UI
{
    public abstract class ViewBase : MonoBehaviour
    {
        [SerializeField] private int sortingOrder;
        public string Identifier { get; set; }
        public RectTransform RectTransform { get; protected set; }
        public CanvasGroup CanvasGroup { get; protected set; }
        public Canvas Canvas { get; protected set; }

        public int SortingOrder
        {
            get => sortingOrder;
            set
            {
                Canvas.sortingOrder = value;
                sortingOrder = value;
            }
        }

        public bool Show { get; set; }

        public virtual bool Visibility
        {
            get { return this.gameObject != null ? this.gameObject.activeSelf : false; }
            set
            {
                if (this.gameObject == null)
                    return;
                if (this.gameObject.activeSelf == value)
                    return;

                this.gameObject.SetActive(value);
            }
        }


        /// <summary>
        /// 当View被Register的时候会调用一次
        /// </summary>
        /// <param name="viewModel">相应的ViewModel</param>
        public virtual void Init(IViewModel viewModel)
        {
            Canvas = gameObject.GetOrAddComponent<Canvas>();
            Canvas.sortingOrder = SortingOrder;
            CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
            // 设置RectTransform
            RectTransform = GetComponent<RectTransform>();
            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.one;
            RectTransform.anchoredPosition = Vector2.zero;
            RectTransform.sizeDelta = Vector2.zero;
            OnInit();
        }

        public virtual void Unregister()
        {
        }

        public void ShowView()
        {
            if (Show) return;
            Show = true;
            Visibility = true;
            CanvasGroup.alpha = 1;
            OnShow();
        }

        public void HideView()
        {
            if (!Show) return;
            Show = false;
            Visibility = false;
            OnHide();
        }

        public void ShowView(UIAnimation showAnimation)
        {
            Visibility = true;
            CanvasGroup.alpha = 1;
            OnShow();
            showAnimation.OnEnd(() =>
            {
                Show = true;
                OnShowAnimEnd();
            });
            showAnimation.Play(this);
        }

        public void HideView(UIAnimation hideAnimation)
        {
            OnHide();
            hideAnimation.OnEnd(() =>
            {
                Visibility = false;
                Show = false;
                OnHideAnimEnd();
            });
            hideAnimation.Play(this);
        }

        private void Update()
        {
            if (!Visibility) return;
            OnUpdate();
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnShowAnimEnd()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnHideAnimEnd()
        {
        }

        protected void NavigateTo(string address, bool recordHistory = false)
        {
            MemoFrameworkEntry.GetComponent<UIComponent>().NavigateTo(this, address, recordHistory);
        }

        protected void SetHome()
        {
            MemoFrameworkEntry.GetComponent<UIComponent>().SetHome(this);
        }
    }

    public abstract class ViewBase<T> : ViewBase where T : class, IViewModel
    {
        public T ViewModel { get; set; }

        public sealed override void Init(IViewModel viewModel)
        {
            Canvas = gameObject.GetOrAddComponent<Canvas>();
            CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
            // 设置RectTransform
            RectTransform = GetComponent<RectTransform>();
            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.one;
            RectTransform.anchoredPosition = Vector2.zero;
            RectTransform.sizeDelta = Vector2.zero;
            OnInit();
            try
            {
                ViewModel = (T)viewModel;
                OnBindViewModel(ViewModel);
            }
            catch (Exception e)
            {
                MFLogger.LogError(MFUtils.Text.Format("初始化{0}时发生错误：{1}", typeof(T).Name, e.Message));
                throw;
            }
        }

        public sealed override void Unregister()
        {
            OnUnbindViewModel(ViewModel);
        }


        /// <summary>
        /// MF中View的Viewmodel绑定统一只在Init时进行，如有需求可以自行改造为动态绑定
        /// 参考网址：https://www.cnblogs.com/OceanEyes/p/unity3d_framework_designing_get_started_with_mvvm_part1.html
        /// </summary>
        /// <param name="viewModel"></param>
        protected abstract void OnBindViewModel(T viewModel);

        protected abstract void OnUnbindViewModel(T viewModel);
    }
}