// using System;
//
// namespace General.Binder
// {
//     public class BindDataActComparable<T> : BindDataAct<T> where T : IComparable
//     {
//         T startEvery;
//
//         public BindDataActComparable(Observable<T> osv, T startEvery = default) : base(osv)
//         {
//             this.startEvery = startEvery;
//         }
//
//         public BindDataActComparable<T> CulminateEvery(T every, int everyMaxCount = 1000) 
//         {
//             BeforeTo();
//             var tempAct = act;
//             startEvery = osv.Value;
//             act = (newV) =>
//             {
//                 int tempCount = 0;
//                 while (true)
//                 {
//                     if (tempCount >= everyMaxCount)
//                         break;
//                     if (osv.Value?.CompareTo((dynamic)startEvery + every) < 0)
//                         break;
//                     tempAct(newV);
//                     startEvery += (dynamic)every;
//                     tempCount++;
//                 }
//             };
//             AfterTo();
//             return this;
//         }
//     }
// }