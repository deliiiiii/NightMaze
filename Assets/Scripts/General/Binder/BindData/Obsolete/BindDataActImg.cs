// using System;
// using UnityEngine.UI;
//
// public class BindDataActImg<T> : BindDataAct<T>
// {
//     public BindDataActImg(Observable<T> osv, Image img, Func<float, float> func) : base(osv)
//     {
//         this.img = img;
//         this.func = func;
//         act = _ => img.fillAmount = func(osv);
//     }
//
//     readonly Image img;
//     readonly Func<float, float> func;
//     // float deltaPerSecond = float.MaxValue;
//     public BindDataActImg<T> Fluent(float deltaPerSecond)
//     {
//         BeforeTo();
//         // this.deltaPerSecond = deltaPerSecond;
//         act = _ => img.DoFluentFill(func(osv), deltaPerSecond);
//         AfterTo();
//         return this;
//     }
//
//     public new BindDataActImg<T> Immediate()
//     {
//         base.Immediate();
//         return this;
//     }
// }