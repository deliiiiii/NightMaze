用于让 Unity 使用项目配置的最新 C# 版本。

#### 使用步骤

1. 关闭Unity 编辑器。

2. 安装 [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)。

3. 确保Unity Hub的Installs列表中有Unity `6000.3.19f1`。

4. 打开 PowerShell（管理员），cd到项目根目录（项目根目录中应有 `Assets`、`Packages` 和
   `ProjectSettings` 文件夹）。
```powershell
cd E:\XXX\XXX\CatMetro
```

5. 执行：

```powershell
.\tools\Patch-UnityCSharp.ps1 -AllowPrerelease
```



##### ——————————其他指令——————————

#### 预览而不修改

使用 `-WhatIf` 检查路径和环境：

```powershell
.\tools\Patch-UnityCSharp.ps1 -WhatIf -AllowPrerelease
```

#### 恢复补丁

关闭 Unity 后执行：

```powershell
.\tools\Patch-UnityCSharp.ps1 -Action Revert
```

补丁修改 Unity 编辑器安装目录，因此使用同一个 Unity 编辑器的其他项目也会受影响。

#### C# 配置

通常不需要修改。如果需要为某个程序集指定版本，在 `.asmdef` 同目录创建 `csc.rsp`：

```text
-langVersion:preview
-nullable:enable
```

不要把 `.asmdef` 和 `csc.rsp` 放在 `Assets/` 根目录，应放在子目录中。
