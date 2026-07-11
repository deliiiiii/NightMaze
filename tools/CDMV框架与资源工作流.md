# 框架与工作流

## 2. 总体架构与依赖方向

分层框架：

- **C — Config**：策划配置和静态定义。
- **M — Model**：运行时数据、状态机、存档数据。
- **E — Event**：`UniAction`、`EvtBase`、`UniEvt` 组成的行为与消息通道。
- **V — View**：Unity 场景对象、UI、动画。

### 2.1 各目录职责

| 目录 | 职责 | 依赖关系 |
| --- | --- | --- |
| `Assets/Scripts/General` | 相对稳定的项目无关工具：资源、存档、日志、绑定、Tween、单例等 | 部分依赖UnityEditor |
| `Assets/Scripts/GeneralPreview` | 正在演进的框架层：Model/View 生命周期、事件、Action、Option、Factory、扩展方法 | 部分依赖UnityEditor |
| `Assets/Scripts/CM/Config` | 策划配置 | 配置可引用 Unity Asset，不应引用 View |
| `Assets/Scripts/CM/Model | 游戏状态、规则、运行时 | 不应引用 View |
| `Assets/Scripts/CM/View` | UI、Prefab 实例、动画、玩家输入和数据展示 | 最上层，无限制 |
| `Assets/Scripts/CM/Editor` | 仅编辑器使用的创建器、Drawer、Addressable 同步工具。 | 若不在Editor文件夹内引用了Editor文件夹内的这些代码，则需#if UNITY_EDITOR |

### 2.2 程序集依赖规则

```text
General / GeneralPreview ---底层
          ↑
        Config
          ↑
         Model
          ↑
         View			---上层
```

1. Model 可以读取 Config，但不能直接调用 View层。
2. View 可以读取 Model 和 Config，但不应直接修改 Model 的数据；应调用公开方法、提交 `UniAction`，或发出输入事件。
3. Model 到 View 的通知使用 `EvtBase` / `EvtForgetBase`。
4. View 到 Model 的输入可以使用输入事件，由 Model 中的 `UniEvt` 监听；明确属于单一数据对象的操作，也可以调用该对象的公开命令入口。

### 2.3 启动链

主入口是 `Assets/Scripts/CM/View/Launcher.cs`：

```text
Launcher.Awake
└─ 清理上次 Play Mode 遗留的 Bus 监听

Launcher.Start
├─ MyInput.Init
├─ Loader.LoadAllAsync
│  ├─ ConfigLoader 加载 CMConfig 标签下的全部 ConfigBase
│  └─ ItemResLoader 加载 CMItemSprite 标签下的全部 Sprite
├─ MigrateStepRegister.Init
├─ ViewStatic.BindAll
├─ GameRoot 注册到 Launcher 生命周期
└─ GameRoot.ChangeStateAsync(new GameTitle(), false)
```

Addressable 文件夹规则的同步发生在进入 Play Mode、运行 `Launcher.Start` 之前。因此正常流程是：先同步 Addressables，再由 Loader 按标签加载资源。

## 3. Config：策划配置层

核心代码：

- `Assets/Scripts/GeneralPreview/CDEV/ConfigBase.cs`
- `Assets/Scripts/CM/Config/ConfigLoader.cs`
- `Assets/Scripts/CM/Config/Item/ItemConfig.cs`

### 3.1 三种基类

#### `ConfigBase`

所有会被统一加载的配置基类，继承 Odin 的 `SerializedScriptableObject`。`ConfigLoader` 按 Addressable 标签加载后，通过 `OfType<T>()` 查询具体配置。

#### `ConfigSingle<T>`

适合全项目只有一份的配置，例如科技树总配置：

```csharp
public class TechTreeConfig : ConfigSingle<TechTreeConfig>
{
    public List<TechNodeConfig> NodeList = [];
}
```

读取：

```csharp
TechTreeConfig config = ConfigLoader.Acquire<TechTreeConfig>();
```

如果没有任何该类型配置，`Acquire` 会抛异常。

#### `ConfigMulti<T>`

适合同类型多份、通过 ID 区分的配置，例如 `ItemConfig`：

```csharp
public class ItemConfig : ConfigMulti<ItemConfig>
{
    public override string PrefixName => "Symbol";
}
```

它提供：

- `Name`：策划可读名称。
- `ID`：运行时稳定标识。
- `PrefixName`：文件名前缀。
- Inspector 修改名称或 ID 后，自动重命名为 `Prefix_ID_Name.asset`。
- 编辑器内检查名称有效性和同目录、同前缀的 ID 重复。

读取：

```csharp
ItemConfig config = ConfigLoader.Acquire<ItemConfig>(id);
IEnumerable<ItemConfig> symbols = ConfigLoader.AcquireSome<ItemConfig>(c => c.IsSymbol);
MyOption<ItemConfig> optional = ConfigLoader.AcquireOptional<ItemConfig>(id);
```

注意：当前 `Acquire<T>(id)` 找不到指定 ID 时，会回退到同类型第一份配置；只有一份都没有时才抛异常。需要严格校验 ID 的业务不能把这个回退当成“查找成功”。

### 3.2 配置加载时机

`ConfigLoader.Bind` 使用：

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
```

它在每次 Player 子系统初始化时把 `LoadToRuntimeAsync` 注册到 `Loader.OnLoad`。这样即使 Unity 关闭 Domain Reload，每次进入 Play Mode 也会重新建立加载入口。

加载过程：

1. 使用 `Const.Res.AddrTag.ConfigTag`，当前值为 `CMConfig`。
2. 调用 `Resourcer.LoadAssetsByTagAsync<ConfigBase>()`。
3. 保存到进程内 `configList`。
4. `Acquire` 系列方法从列表按类型、ID 查询。

Editor 菜单 `Tools/Reload Editor resources` 会清理 Addressable 缓存，并通知 ConfigLoader 重新加载，适合 Play Mode 中修改配置后手动刷新。

### 3.3 新增配置类型的程序规范

单例配置：

```csharp
[CreateAssetMenu(fileName = "新全局配置", menuName = "CM/全局配置")]
public sealed class GlobalConfig : ConfigSingle<GlobalConfig>
{
    public int Value;
}
```

多实例配置：

```csharp
public sealed class EnemyConfig : ConfigMulti<EnemyConfig>
{
    public override string PrefixName => "Enemy";
    public int Hp;
}
```

程序需要同时完成：

1. 定义类型和字段。
2. 提供 `CreateAssetMenu` 或专用创建菜单，避免策划从空白 ScriptableObject 开始。
3. 使用 Odin 的 `Required`、`ValidateInput`、`ValueDropdown` 等约束尽量前置报错。
4. 确保资产位于 `Assets/Config` 目录树内，才能被 `CMConfig` 文件夹规则覆盖。
5. 明确 ID 段、命名方式和缺省值。

## 4. Model：运行时数据与状态生命周期

核心代码：

- `Assets/Scripts/GeneralPreview/CDEV/ModelBase.cs`
- `Assets/Scripts/CM/Model/FSM/GameRoot.cs`
- `Assets/Scripts/CM/Model/FSM/Playing/GamePlaying.cs`

### 4.1 `ModelBase<TThis>` 生命周期

每个 Model 节点拥有独立 `CancellationTokenSource`，并实现 `IHasCt`：

```csharp
public CancellationToken CurCt => cts.Token;
```

创建顺序是固定的：

```text
OnCreateAsync(isThisFromLoad)
├─ 反射注册该 Model 上的全部 UniEvt
├─ 将 OnSelfTick 注册到全局 Updater
├─ 如果是新数据：OnCreateFreshModel
├─ await EvtOnEnter(this)
└─ await OnLaunchCom(isThisFromLoad)
```

移除顺序：

```text
OnRemove
├─ OnReleaseCom
├─ 取消自己的 CurCt
└─ EvtOnExit.Forget()
```

这意味着：

- 注册到 `CurCt` 的事件、Tick、异步工作会随 Model 退出而取消或解绑。
- `EvtOnEnter` 是可等待事件，View 完成进入表现后，Model 才继续 `OnLaunchCom`。
- `EvtOnExit` 是无上下文、不可等待的通知；监听器不能从事件参数直接取得刚退出的数据。

### 4.2 状态（状态机）管理

`_ChangeAsync(ref field, node, isNewFromLoad)` 完成：

1. 移除旧节点。
2. 替换字段。
3. 如果子节点实现 `IHasBelong<T>`，注入父节点引用。
4. 调用新节点 `OnCreateAsync`。

例如`GameRoot` 用它维护当前根状态：

```csharp
await GameRoot.ChangeStateAsync(new GamePlaying(playerName), false);
```

查询状态：

```csharp
bool playing = GameRoot.IsState<GamePlaying>();
或
MyOption<GamePlaying> state = GameRoot.GetStateOptional<GamePlaying>();
```

### 4.3 Model 编写原则

- Model 不保存 MonoBehaviour、GameObject、Tween、UI 控件引用。
- 只读查询公开为属性或方法；改变状态的函数不可直接调用，而需按第5章的写法。
- 异步逻辑必须传CancellationToken。
- `OnCreateFreshModel` 只初始化新建数据；反序列化恢复后的逻辑在`OnLaunchCom`。

## 5. UniAction 行为流与源码生成

核心代码：

- `Assets/Scripts/GeneralPreview/CDEV/UniAction.cs`
- `Assets/Scripts/CM/Generator/Attr.cs`
- `Assets/Scripts/CM/Generator/YuanSheng.dll`
- `Assets/Scripts/CM/Model/FSM/Playing/Act_*.cs`

### 5.1 `UniAction<TThis>` 的用途

`UniAction` 表示“由某个 Model 上下文执行、可以等待、可以取消、可以序列化进待办队列”的业务动作。

它将自己的 CancellationTokenSource和 `Self.CurCt` 链接：

- `await action`：执行并等待完成。
- `action.Forget()`：发起但不等待。
- `action.CancelSelfly()`：只取消该 Action，并标记 `IsCancelledSelfly`。
- Model 退出：`Self.CurCt` 取消，Action 随之结束。

`GamePlaying` 和 `PlaySpin` 的 `toDoList` 就是 `IUniAction` 队列。执行器每次等待队首 Action，完成后移除，再继续下一项。

### 5.2 自动生成 Action 的写法

该Model类必须：

- 标记 `[ActContainer]`。
- 声明为 `partial class`。
- 文件名以 `Act` 开头。

被生成的方法必须：

- 是 `private`（省略访问修饰符即为 private）。
- 返回 `UniTask`。
- 方法名以 `Async` 结尾。
- 最后一个参数的类型是 `CancellationToken`。

示例源码：（注意到这个动作名为SpawnItemAtPos，我们将见到SpawnItemAtPos**Async**、**Act**SpawnItemAtPos和**Evt**SpawnItemAtPos）

```csharp
[ActContainer]
public partial class GamePlaying
{
    [Obsolete("尝试在某位置生成某物体")]
    async UniTask SpawnItemAtPosAsync(
        long id,
        Vector2Int pos,
        ResultWrap? resultWrap,
        CancellationToken ct)
    {
        // 业务实现
    }
}
```

执行动作时，不调用这个函数（因为Obsolete特性，强制调用反而报警告），而是使用：

```csharp
await new GamePlaying.ActSpawnItemAtPos(gamePlaying)
{
    Id = id,
    Pos = pos,
    ResultWrap = resultWrap
};
```

生成器还会生成对应的执行完毕事件**Evt**SpawnItemAtPos，便于观察动作完成。`[MuteActEvt]` 用于不需要生成与发布该事件。

`[Obsolete("说明")]` 在这里同时承担两个目的：

1. 给生成的 Action/事件提供可读说明。
2. 阻止其他业务代码绕过 Action，直接调用原始 `XxxAsync` 方法。

## 6. E：事件的收发， `EvtBase`、`EvtForgetBase`、`UniEvt`

核心代码：

- `Assets/Scripts/GeneralPreview/CDEV/EvtBase.cs`
- `Assets/Scripts/GeneralPreview/CDEV/UniEvt.cs`

### 6.1 `EvtBase<THasCt>`：发送 可等待流程事件

```csharp
定义：
public record EvtMoveItem(GamePlaying WhoHasCt, MyItem Item)
    : EvtBase<GamePlaying>(WhoHasCt);
发送：
var ret = await new EvtMoveItem(this, item);
或不拿返回值直接写：
await new EvtMoveItem(this, item);
```

上面的代码实现了，model层发送事件让view层执行删除物体，删除动画播放完后，model层继续其他业务。this为上下文，减轻了view层要自己保存上下文的负担。

特点：

- 携带实现 `IHasCt` 的上下文。
- 使用 `WhoHasCt.CurCt`。
- 发布者可以等待全部监听器完成。
- 监听器按注册列表顺序串行执行。
- 一个监听器抛异常时，后续监听器不会继续，异常返回发布者。

### 6.2 `EvtForgetBase`：无等待通知

```csharp
定义：
public record EvtClickExit : EvtForgetBase;
发送：
new EvtClickExit().Forget();
```

特点：

- 不携带统一上下文。
- 使用 `CancellationToken.None` 发布。
- 发布者不等待处理完成。

### 6.3 监听事件

在 `ViewBase` 或 `ModelBase` 派生类中声明 `UniEvt<T>`类型的 **只读属性**，其中T为EvtBase的子类：

```csharp
UniEvt<GamePlaying.EvtMoveItem> OCMoveItem => new()
{
    Invoke = (evt, ct) =>
    {
        MoveItem(evt.Item);
        return UniTask.CompletedTask;
    },
    Des = "移动物体表现"
};
```

`IUniEvt.BindAll(this, ct)` 会**反射查找实例属性**，访问 getter。`new UniEvt<T>()` 的构造函数立即注册到全局 Bus。

### 6.4 非常有用！等待输入事件

直接举例：

```csharp
var evt = await Bus.WaitForAsync<EvtClickSelectSymbol>(
    "等待选择棋子",
    ct);
```

上面的代码实现了：model层的业务执行函数到一半，需要等待玩家点击按钮，点完后有些操作如经验+3。

EvtClickSelectSymbol定义在Model层，但由View层发送。

### 6.5 `[EvtChanged]` 自动属性事件

```csharp
[EvtChanged]
public partial int TurnCount { get; private set; } = 1;
```

源码生成器会生成 setter 逻辑，在set结束后发出事件：

```csharp
EvtTurnCountChanged(this, oldValue, newValue)
```

View 可以监听：

```csharp
UniEvt<GamePlaying.EvtTurnCountChanged> OnTurnCountChanged => new()
{
    Invoke = (evt, ct) =>
    {
        TxtTurnCount.text = evt.NewValue.ToString();
        return UniTask.CompletedTask;
    },
    Des = "更新回合数文本"
};
```

标记 了`[EvtChanged]` 的**属性**和所属**类**都必须写 `partial` 。

## 7. V：View 生命周期、绑定与输入

核心代码：

- `Assets/Scripts/GeneralPreview/CDEV/ViewBase.cs`
- `Assets/Scripts/CM/View/ViewStatic.cs`
- `Assets/Scripts/CM/View/FSM/Play/PlayView.cs`

### 7.1 `ViewBase`

`Bind(ct)` 会：

1. 防止同一 View 重复绑定。
2. 将传入 Token 与 `destroyCancellationToken` 链接。
3. 绑定 `BindList()` 返回的 UnityEvent/Update Binder。
4. 反射注册 View 中所有 `UniEvt`。

`Unbind()` 会取消该 View 的手动 CTS，从而自动移除绑定和事件监听。

`Awake()` 默认自动调用 `Bind()`；`ViewStatic.BindAll(ct)` 再调用时因 guard 保持幂等。如果某个 View 必须绑定到特定业务 Token，应明确控制首次 Bind 的时机，不能等自动 Awake 已经绑定后再传 Token。

### 7.2 `ViewBase<TModel>`

它只增加：

```csharp
public TModel Model { get; set; }
```

典型做法是在 `EvtOnEnter` 中注入：

```csharp
Model = evt.WhoHasCt;
```

退出时清理：

```csharp
Model = null!;
```

View 内后续逻辑优先使用自己的 `Model`，不要重新从 `GameRoot` 或 `PlayViewIns` 获取全局对象。子 View 应由父 View 显式 `Refresh(Model)`、`BindModel(Model)` 或构造上下文，而不是随处访问全局单例。

### 7.3 `BindList()` 与 UnityEvent

```csharp
protected override IEnumerable<BindModelBase> BindList() =>
[
    BtnExit.onClick.EvtBindTo(
        () => new GamePlaying.EvtClickExit().Forget()),
    BtnSetting.onClick.EvtBindTo(
        () => SettingViewIns.SetActiveTrue())
];
```

Binder 会在 View 生命周期结束时**自动移除 UnityEvent 监听**，避免手写 `OnEnable +=` / `OnDisable -=`。

### 7.4 View 编写原则

​	事件驱动。

## 8. General 高频通用代码

### 8.1 `Resourcer`：Addressables 加载与缓存

文件：`Assets/Scripts/General/IO/Resourcer.cs`

常用 API：

```csharp
T? asset = Resourcer.LoadAsset<T>(address); // 同步，非必要不使用

var (assets, level, info) =
    await Resourcer.LoadAssetsByTagAsync<T>(tag, ct);

Resourcer.ReleaseTag(tag);
```

内部行为：

- `assetHandleCache` 按资源地址缓存 `AsyncOperationHandle`。
- `tagLocationsCache` 按标签缓存资源位置。
- 同一地址再次读取优先返回缓存。
- 标签加载先找 locations，再并发等待各资源。
- Editor 退出 Play Mode 时清空字典。
- `Tools/Reload Editor resources` 会 Release 有效 Handle、清空缓存并通知 Editor 资源使用者重载。

使用规则：

- 业务初始化优先使用异步标签加载。
- Token 必须来自 Launcher、Model 或 View 生命周期。
- 不要自行 `Addressables.Release` Resourcer 管理的 Handle，否则缓存会持有失效句柄。
- 需要释放一整类资源时调用 `ReleaseTag`。
- 当前公共单资源加载只有同步 `LoadAsset`；新增运行时按地址异步需求时，应先扩展统一 Resourcer API，不要在业务层散写 Addressables。

### 8.2 `Loader`：项目启动加载编排

文件：`Assets/Scripts/CM/Static/Loader.cs`

`Loader.OnLoad` 是启动阶段加载器列表，`LoadAllAsync` 按委托注册顺序串行等待。

推荐注册模板：

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void Bind()
{
    Loader.OnLoad -= LoadAsync;
    Loader.OnLoad += LoadAsync;
}

static async UniTask<(ELogLevel, string)> LoadAsync(CancellationToken ct)
{
    // 加载并构建运行时索引
    return (ELogLevel.Info, "加载完成");
}
```

必须先 `-=` 再 `+=`，防止关闭 Domain Reload 时每进入一次 Play Mode 多注册一次。不要使用无法移除的临时 lambda 反复订阅静态事件。

### 8.3 `Saver` / `JsonIO`：存档

文件：

- `Assets/Scripts/General/IO/Saver.cs`
- `Assets/Scripts/General/IO/JsonIO.cs`
- `Assets/Scripts/General/IO/IMigrateStep.cs`

业务层只调用 `Saver`：

```csharp
await Saver.SaveAsync(folder, name, Model);
T? Model = Saver.Load<T>(folder, name);
T? Model = await Saver.LoadAsync<T>(folder, name, ct);
T? Model = await Saver.LoadWithVerAsync<T>(folder, name, ct);
Saver.Delete(folder, name);
```

`JsonIO` 使用 Newtonsoft.Json，当前设置包括：

- 缩进输出。
- `TypeNameHandling.Auto`，支持多态节点。
- 保存对象引用，支持对象图中的共享引用和循环。
- 自定义 ContractResolver，补充 private 可写属性和 private 字段。
- 忽略委托。
- 反序列化遇到废弃/无法识别节点时记录错误并跳过。

读档时会进入 `BusDisposable.MuteScope`，避免反序列化 setter 触发的事件污染正常流程。

版本迁移：

1. 可迁移数据实现 `IHasVersion`。
2. 当前版本来自 `General.Const.Version`。
3. 每个 `IMigrateStepJson` 描述 `FromVersion → ToVersion`。
4. `MigrateStepRegister.Init()` 在读取版本存档前注册步骤。
5. 版本必须形成连续链；缺步骤时停止迁移并记录错误。

### 8.4 `MyDebug`

文件：`Assets/Scripts/General/IO/MyDebug.cs`

统一使用：

```csharp
MyDebug.Log(message);
MyDebug.LogWarning(message);
MyDebug.LogError(message);
```

不要在业务代码混用大量 `UnityEngine.Debug`。`MyDebugConfig` 和 Editor Window 可以统一控制日志级别和类型。

### 8.5 Binder 与 `Updater`

文件：`Assets/Scripts/General/Binder/BindModel/*`

两类常用 Binder：

```csharp
// UnityEvent的绑定如下，第一行这时还没真正绑定
BindDataEvt bindData = button.onClick.EvtBindTo(OnClick);
bindData.Bind(ct); // 这时才绑好
bindData.UnBind(); // 解绑了

// Action（如下面的tick函数）通过Updater静态类自动绑定与执行
Action<float> tick = dt => Tick(dt);
// 还没真正绑定
BindDataUpdate bindData = tick.ActBindTo(EUpdatePri.Fsm);
bindData.Bind(ct); // 这时才绑好
bindData.UnBind(); // 解绑了
```

`Updater` 维护 `SortedDictionary<int, HashSet<BindModelUpdate>>`，按优先级执行（如上面的EUpdatePri.Fsm）。CancellationToken 取消时 Binder 自动 `UnBind`。

### 8.6 单例`Singleton<T>`

文件：`Assets/Scripts/General/Singleton/Singleton.cs`

`Instance` 会先在场景中查找，没有时在运行状态创建 GameObject。`GlobalOnScene` 控制 `DontDestroyOnLoad`。

- 派生类 `Awake` 必须调用 `base.Awake()`。

### 8.7 DOTweenSequence / DoTweenSeqMutex

文件：

- `Assets/Scripts/General/DoTweenSequence.cs`
- `Assets/Scripts/General/DoTweenSeqMutex.cs`
- `Assets/Scripts/General/DoTweenSequenceExt.cs`

`DOTweenSequence` 将常用 Tween 序列暴露到 Inspector，并提供 `PlayAsync(ct)` 等异步入口。`DoTweenSeqMutex` 在播放新序列前 Kill 当前序列，适合同一 UI 状态只能播放一个过渡动画的场景。

View 中播放时应传入 CancellationToken。

### 8.8 `ObjectPool<T>`

文件：`Assets/Scripts/General/ObjectPool.cs`

提供预热、异步实例化和 `MyDestroy` 回池。新增高频生成对象前应优先评估现有池是否满足：

- 回池时只做了 `SetActive(false)`，重置状态没写。
- 从池中取时，没有一个取完后的回调，这个也没写。
- 池对象依赖 prefab 和父 Transform。

## 9. GeneralPreview 高频通用代码

### 9.1 `MyOption<T>`

文件：`Assets/Scripts/GeneralPreview/MyFP/Impl/Option.cs`

表示“有值或无值”：

```csharp
MyOption<GamePlaying> optional = GameRoot.GetStateOptional<GamePlaying>();

var name = (
    from Model in optional
    select Model.PlayerName)
    | "默认名称";

if (optional is MySome<GamePlaying> { Value: var Model })
{
    // 使用 Model
}
```

当前实现是引用类型 record：

- 非空值隐式转换会 `new MySome<T>`。
- `Map/Select/Bind` 会创建新的 Option。
- `None` 常量本身复用，但部分 Map 路径仍会 `new MyNone<T>`。

由于是record引用类型，还是适合低频业务组合，不适合 `Update`。热路径推荐提供：

```csharp
public bool TryGetSpin(out PlaySpin spin);
public bool IsWaitClickNextTurn => inSpin?.IsWaitClickNextTurn ?? false;
```

### 9.2 `MyEither<TLeft, TRight>`

文件：`Assets/Scripts/GeneralPreview/MyFP/Impl/Either.cs`

表示两种互斥结果，支持 `Map1`、`Bind1`、`Match`、LINQ 查询语法和 `IsLeft`。适合“成功数据或错误信息”等明确二选一返回。当前使用频率不高，新代码只有在它比普通 Result 类型更清晰时才使用。

### 9.3 `MyPrelude`

文件：`Assets/Scripts/GeneralPreview/MyFP/Static/MyPrelude.cs`

通过 global using 提供：

- `None`：转换为 `MyOption<T>.None`。
- `RTask`、`RTrue`、`RFalse`、`Rid` 等常用无状态函数。
- `Compose` / `ComposeA`。

这些是为函数组合和减少临时 lambda 准备的。代码中如果可读性下降，应优先写清晰的具名方法。

### 9.4 一些有的没的的扩展

| 文件 | 常用能力 | 注意事项 |
| --- | --- | --- |
| `GameObjectExt.cs` | `SetActiveTrue/False`、`GetOrAddCom`、`BindEvtTrg` | `MyGetCom` 会构造 Option；热路径慎用 |
| `TransformExt.cs` | 子节点遍历、清理、禁用 | `Destroy` 延迟到帧末；遍历时注意对象仍存在 |
| `IEnumerableExt.cs` | `FirstOptional`、串行 `ForEachAsync` | `FirstOptional` 当前会先 `ToList`，有分配 |
| `ListExt.cs` | 随机、权重随机、Shuffle | 多数方法会创建过滤列表或副本 |
| `EnumExt.cs` | 枚举取值 | 适合 Editor 和低频初始化 |
| `DeepCopyExt.cs` | 反射深拷贝 | 成本较高，不用于帧循环 |
| `TypeExt.cs` | 查找派生类型 | 反射扫描，只用于初始化 |

## 10. ？美术资源对接流程

=

## 11. 策划资源对接流程

### 11.1 新建物体配置

Unity 菜单已经提供：

```text
Assets/Create/CM/1_新棋子
Assets/Create/CM/2_新建筑
Assets/Create/CM/3_新资源
Assets/Create/CM/4_新事件
Assets/Create/CM/5_新地块
```

推荐流程：

1. 从 SVN 更新。
2. 在对应目录中创建配置：
   - 棋子：`Assets/Config/1_Symbol`
   - 建筑：`Assets/Config/2_Building`
   - 资源：`Assets/Config/3_Resource`
   - 事件：`Assets/Config/4_Event`
   - 地块：`Assets/Config/5_Grid`
3. 填写唯一 ID 和名称。
4. Unity 会按类型自动整理文件名，例如 `Symbol_10001_名称.asset`。
5. 配置对应美术图片时，图片名称使用相同 ID，例如 `10001.png`。
6. 补全 Inspector 中所有 Required 字段、Tag、位置和词条。
7. 进入 Play Mode 验证。
8. 提交 `.asset` 和 `.asset.meta`；如果同时新增美术资源，也提交对应资源和 `.meta`。

### 11.2 修改配置

- 修改字段后直接保存工程，不需要 Apply Rules。
- Play Mode 已经加载过配置时，可退出后重新进入；也可以在 Editor 菜单执行 `Tools/Reload Editor resources`。
- 修改 ID 会触发资产重命名，也会改变美术查找 Key。必须同步修改图片名及其他使用该 ID 的配置。
- 修改 Name 只影响文件可读名称，不应被程序作为稳定 Key。

### 11.3 Tag 与管理器配置

`ItemConfig` 的通用、棋子、建筑、资源、事件、地块 Tag 下拉来自 `Assets/Config/Mgr` 中的管理器资产。策划可以选择已有 Tag；新增 Tag 分类或调整 ID 前需要程序/主程确认，因为它可能影响枚举、筛选逻辑和既有存档。

### 11.4 策划禁止操作

- 不手工修改 Addressables Groups。
- 不为每个 Config 单独勾 Addressable。
- 不修改 `CMConfig` / `CMItemSprite` 标签拼写。
- 不复制一份配置后保留重复 ID。
- 不在文件管理器里只移动 `.asset` 而遗漏 `.meta`。

## 12. 程序资源接入方法

### 12.1 当前资源管理模型

每个 `AddressableFolderRule` 管理一个文件夹：

```text
Folder  → Addressable Folder Entry
Tag     → Entry.address
Tag     → Entry.label（唯一一个分类标签）
```

当前规则：

| 规则资产 | 文件夹 | Address/Label | 运行时使用者 |
| --- | --- | --- | --- |
| `Assets/Config/Tags/NMConfig.asset` | `Assets/Config` | `NMConfig` | `ConfigLoader` |
| `Assets/Config/Tags/NMItemSprite.asset` | `Assets/Art/Sprite/Item` | `NMItemSprite` | `ItemResLoader` |

目录是 Entry 后，子资源会作为该目录的 Addressable 内容被标签定位。好处是新增、删除普通子资源通常不改 Group YAML，降低 SVN 冲突；代价是不能再通过本项目规则为每个子文件分别配置独立 Address 和 Label。

本项目运行时主要按标签和子资源 `PrimaryKey` 加载，因此这个代价目前不明显。

### 12.2 使用已有类别

配置：

```csharp
ItemConfig config = ConfigLoader.Acquire<ItemConfig>(id);
```

物体图片：

```csharp
Sprite sprite = ItemResLoader.Acquire(id);
```

通用标签加载：

```csharp
var (assets, level, info) =
    await Resourcer.LoadAssetsByTagAsync<MyAsset>(
        Const.Res.AddrTag.MyAssetTag,
        ct);
```

### 12.3 新增资源类别

只有“新增一种此前不存在的资源集合”才需要程序处理。往已有目录里加文件不需要新增规则。

步骤：

1. 创建专用目录，例如：

   ```text
   Assets/Art/Audio/BGM
   ```

2. 打开：

   ```text
   Tools/CM/AddressableBatchProcessor
   ```

3. 点击 `Create Folder Rule`，将规则资产保存在 `Assets/Config/Tags`。
4. 设置：

   ```text
   Enable = true
   Folder = Assets/Art/Audio/BGM
   Tag = CMBgm
   ```

5. 在 `Assets/Scripts/CM/Static/Const.cs` 的 `Const.Res.AddrTag` 增加常量。字段名遵守以 `Tag` 结尾的项目约定：

   ```csharp
   public const string BgmTag = "CMBgm";
   ```

6. 编写 Loader，建立运行时索引：

   ```csharp
   public static class BgmLoader
   {
       static Dictionary<string, AudioClip> clips = [];

       [RuntimeInitializeOnLoadMethod(
           RuntimeInitializeLoadType.SubsystemRegistration)]
       static void Bind()
       {
           Loader.OnLoad -= LoadAsync;
           Loader.OnLoad += LoadAsync;
       }

       static async UniTask<(ELogLevel, string)> LoadAsync(
           CancellationToken ct)
       {
           var (list, level, info) =
               await Resourcer.LoadAssetsByTagAsync<AudioClip>(
                   Const.Res.AddrTag.BgmTag,
                   ct);

           clips = list.ToDictionary(x => x.name);
           return (level, info);
       }

       public static AudioClip Acquire(string id) =>
           clips.GetValueOrDefault(id)
           ?? throw new KeyNotFoundException($"BGM 不存在：{id}");
   }
   ```

7. 进入 Play Mode，让 `AddressableBatchProcessor` 自动同步。
8. 验证 Address、Label、加载数量和重复 Key。
9. 提交：目录及 `.meta`、规则资产及 `.meta`、Const/Loader 代码；如果同步确实改变了 Addressables 配置，再一并提交相关 Group/Settings 文件。

### 12.5 SVN 协作规则

| 场景 | 谁负责 | Addressable Group 是否通常变化 |
| --- | --- | --- |
| 已有受管目录新增/删除普通资源 | 美术/策划 | 否 |
| 修改普通资源内容或配置字段 | 美术/策划 | 否 |
| 新增资源类别/Folder Rule | 程序 | 是 |
| 修改 Tag 或 Folder | 程序/主程 | 是 |
| 删除 Folder Rule | 程序/主程 | 是 |

## 13. 示例 新增一个带配置的玩法对象

1. 在 Config 定义或扩展 `ConfigMulti<T>`。
2. 给策划提供菜单、下拉和校验。
3. 在 Model 中定义运行时实体；只保存 ID 或明确可迁移的数据。
4. 需要改变状态的步骤定义为 Action。
5. Model 状态改变后发流程事件。
6. View 监听事件，创建/移动/删除表现对象。
7. View 输入发输入事件，Model 监听并判定是否合法。

Model：

```csharp
[ActContainer]
public partial class GamePlaying
{
    [Obsolete("改变金币")]
    async UniTask ChangeGoldAsync(long delta, CancellationToken ct)
    {
        Gold += delta;
        await new EvtGoldChanged(this, Gold);
    }

    public record EvtGoldChanged(
        GamePlaying WhoHasCt,
        long Value)
        : EvtBase<GamePlaying>(WhoHasCt);
}
```

View订阅事件：

```csharp
UniEvt<GamePlaying.EvtGoldChanged> OnGoldChanged => new()
{
    Invoke = (evt, ct) =>
    {
        TxtGold.text = evt.Value.ToString();
        return UniTask.CompletedTask;
    },
    Des = "刷新金币文本"
};
```

某处发送事件：

```csharp
await new GamePlaying.ActChangeGold(Model)
{
    Delta = 10
};
```

## 14. ？常见问题与排查

## 16. 关键源码索引

| 主题 | 路径 |
| --- | --- |
| Config 基类 | `Assets/Scripts/GeneralPreview/CDEV/ConfigBase.cs` |
| Model 生命周期 | `Assets/Scripts/GeneralPreview/CDEV/ModelBase.cs` |
| View 生命周期 | `Assets/Scripts/GeneralPreview/CDEV/ViewBase.cs` |
| 事件 Bus | `Assets/Scripts/GeneralPreview/CDEV/EvtBase.cs` |
| 事件监听 | `Assets/Scripts/GeneralPreview/CDEV/UniEvt.cs` |
| Action | `Assets/Scripts/GeneralPreview/CDEV/UniAction.cs` |
| Option | `Assets/Scripts/GeneralPreview/MyFP/Impl/Option.cs` |
| 启动入口 | `Assets/Scripts/CM/View/Launcher.cs` |
| 根状态 | `Assets/Scripts/CM/Model/FSM/GameRoot.cs` |
| 配置加载 | `Assets/Scripts/CM/Config/ConfigLoader.cs` |
| 物体图片加载 | `Assets/Scripts/CM/Config/Item/ItemResLoader.cs` |
| Addressable 规则 | `Assets/Scripts/CM/Editor/Window/AddressableFolderRule.cs` |
| Addressable 同步 | `Assets/Scripts/CM/Editor/Window/AddressableBatchProcessor.cs` |
| 资源加载缓存 | `Assets/Scripts/General/IO/Resourcer.cs` |
| 存档 | `Assets/Scripts/General/IO/Saver.cs`、`JsonIO.cs` |
| 存档迁移 | `Assets/Scripts/General/IO/IMigrateStep.cs` |
| Binder | `Assets/Scripts/General/Binder/BindModel` |
| 项目常量 | `Assets/Scripts/CM/Static/Const.cs` |
| 源码生成标记 | `Assets/Scripts/CM/Generator/Attr.cs` |


