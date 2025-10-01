# Tangerine Automation System - VSCode风格UI更新

## 概述

本次更新实现了类似VSCode的UI界面和完整的流程编辑功能，满足了以下需求：

1. ✅ 平台可以添加多个平台任务
2. ✅ 平台任务拥有自己的内部流程编辑器
3. ✅ 产线可以添加多个工艺流程
4. ✅ 工艺流程由平台任务、中转、模块动作组合而成
5. ✅ 模块功能表决定流程节点的可用类型
6. ✅ 流程编辑器支持拖拽和缩放

## 主要功能

### 1. 模块功能表 (Module Function Catalog)

**位置：** 实验室节点 → 模块功能表标签页

**功能：**
- 定义所有可用的模块功能
- 支持添加/删除功能
- 每个功能包含名称、显示名称、模块类型、描述和参数定义
- 预置了H5U、仓库、AGV、机器人等常用模块的功能

**默认功能包括：**
- H5U模块：移动到位置、拾取物品、放置物品
- 仓库模块：存储到仓库、从仓库取出
- AGV模块：AGV移动、AGV装载、AGV卸载
- 机器人模块：机器人初始化、执行程序
- 通用功能：等待、发送信号

### 2. 平台任务管理

**位置：** 平台节点 → 平台任务标签页

**功能：**
- 一个平台可以添加多个任务
- 每个任务有独立的名称和描述
- 每个任务拥有自己的内部流程编辑器
- 任务流程由模块动作和资源节点组成

**使用方法：**
1. 选择平台节点
2. 点击"平台任务"标签
3. 点击"添加平台任务"按钮
4. 输入任务名称和描述
5. 使用流程编辑器设计任务流程

### 3. 产线工艺流程

**位置：** 产线节点 → 工艺流程标签页

**功能：**
- 一个产线可以添加多个工艺流程
- 工艺流程可以引用平台任务
- 支持添加中转节点进行流程控制
- 可以直接添加模块动作节点

**使用方法：**
1. 选择产线节点
2. 点击"工艺流程"标签
3. 点击"添加工艺流程"按钮
4. 输入流程名称和描述
5. 使用流程编辑器设计工艺流程

### 4. 流程编辑器

**功能特性：**

#### 节点类型
- **平台任务节点** (蓝色) - 引用平台内定义的任务
- **模块动作节点** (绿色) - 执行具体的模块功能
- **中转节点** (橙色) - 流程控制和调度
- **资源节点** (紫色) - 仓库、AGV、机器人等

#### 交互操作
- **拖拽移动** - 鼠标左键拖动节点
- **创建连接** - 右键点击起始节点，再右键点击目标节点
- **选择节点** - 左键单击节点

#### 工具栏
- Add Node - 添加通用节点
- Add Platform Task - 添加平台任务节点
- Add Transfer - 添加中转节点
- Add Module Action - 添加模块动作节点
- Auto-Layout - 自动布局节点
- Zoom In/Out - 缩放控制
- Reset Zoom - 重置缩放

#### 缩放功能
- Ctrl + 鼠标滚轮 - 缩放画布
- 缩放按钮 - 点击放大/缩小
- 缩放范围：25% - 300%
- 画布尺寸：2400 x 1800 像素

#### 连接可视化
- 节点之间用虚线连接
- 连接线随节点移动自动更新
- 清晰显示流程流向

## 架构说明

### 数据流

```
LaboratoryModel
  ├── ModuleFunctionCatalog (模块功能表)
  ├── ModuleDefinitions (模块定义)
  └── ProductionLines
        └── ProductionLineModel
              ├── Platforms
              │     └── PlatformModel
              │           └── PlatformTasks
              │                 └── PlatformTask
              │                       └── InternalFlow (平台内部流程)
              └── ProductionLineProcesses
                    └── ProductionLineProcess
                          └── MainFlow (产线主流程)
```

### 流程类型区分

1. **平台内部流程 (InternalFlow)**
   - `IsPlatformLevel = true`
   - 包含模块动作和资源节点
   - 在平台任务编辑器中编辑

2. **产线主流程 (MainFlow)**
   - `IsPlatformLevel = false`
   - 可以包含平台任务、中转、模块动作、资源节点
   - 在工艺流程编辑器中编辑

### 核心组件

#### Models (数据模型)
- `ModuleFunctionCatalog` - 模块功能目录
- `ModuleFunction` - 单个模块功能
- `PlatformTask` - 平台任务
- `ProductionLineProcess` - 产线工艺流程
- `ProcessFlow` - 流程定义
- `FlowNode` - 流程节点
- `Connection` - 节点连接

#### Views (视图)
- `PlatformTaskEditor` - 平台任务编辑器
- `ProductionLineProcessEditor` - 工艺流程编辑器
- `ModuleFunctionCatalogEditor` - 模块功能表编辑器
- `NodifyHostControl` - 流程画布控件

#### ViewModels (视图模型)
- `ProcessEditorViewModel` - 流程编辑器视图模型
- `MainWindowViewModel` - 主窗口视图模型
- `TreeNodeViewModel` - 树节点视图模型

#### Services (服务)
- `ModuleFunctionCatalogInitializer` - 模块功能目录初始化服务
- `PlatformCallGrpcClient` - gRPC客户端（用于执行平台任务）

## 技术栈

- **.NET 8** - 最新的.NET框架
- **WPF** - Windows Presentation Foundation
- **MVVM** - Model-View-ViewModel模式
- **MaterialDesign** - Material Design主题
- **Nodify** - 节点编辑器库
- **Newtonsoft.Json** - JSON序列化

## 文件变更

### 新增文件
- `Models/ModuleFunctionCatalog.cs` - 模块功能目录模型
- `Views/PlatformTaskEditor.xaml(.cs)` - 平台任务编辑器
- `Views/ProductionLineProcessEditor.xaml(.cs)` - 工艺流程编辑器
- `Views/ModuleFunctionCatalogEditor.xaml(.cs)` - 模块功能表编辑器
- `Services/ModuleFunctionCatalogInitializer.cs` - 初始化服务
- `docs/ARCHITECTURE.md` - 架构文档

### 修改文件
- `Models/Entities.cs` - 添加了PlatformTasks、ProductionLineProcesses、ModuleFunctionCatalog
- `Models/PlatformTask.cs` - 添加InternalFlow属性
- `Models/ProductionLineProcess.cs` - 添加MainFlow属性
- `Models/Connection.cs` - 添加可视化属性
- `Views/Controls/NodifyHostControl.xaml(.cs)` - 增强缩放和节点类型支持
- `ViewModels/ProcessEditorViewModel.cs` - 添加连接位置自动更新
- `ViewModels/TreeNodeViewModel.cs` - 添加ModuleFunctionCatalog访问
- `ViewModels/MainWindowViewModel.cs` - 初始化默认模块功能
- `MainWindow.xaml` - 添加平台任务和工艺流程编辑器
- `TangerineAutomationSystem.csproj` - 修复项目文件

## 使用示例

### 创建完整的工作流

#### 步骤1：定义模块功能
1. 打开应用程序
2. 选择"默认项目-实验室1"节点
3. 切换到"模块功能表"标签
4. 查看预置的模块功能（H5U、仓库、AGV等）
5. 根据需要添加自定义功能

#### 步骤2：创建平台任务
1. 展开树 → 产线-A → 平台-1
2. 选择"平台-1"节点
3. 切换到"平台任务"标签
4. 点击"添加平台任务"
5. 命名任务为"取料任务"
6. 在流程编辑器中：
   - 点击"Add Module Action"添加"移动到位置"节点
   - 再添加"拾取物品"节点
   - 右键点击"移动到位置"，再右键点击"拾取物品"创建连接

#### 步骤3：创建工艺流程
1. 选择"产线-A"节点
2. 切换到"工艺流程"标签
3. 点击"添加工艺流程"
4. 命名流程为"全自动取放料流程"
5. 在流程编辑器中：
   - 添加"平台任务"节点（引用"取料任务"）
   - 添加"中转"节点
   - 添加另一个"平台任务"节点
   - 连接各个节点形成完整流程
6. 使用"Auto-Layout"优化布局
7. 使用缩放功能查看完整流程

#### 步骤4：保存项目
1. 点击工具栏的"保存项目"按钮
2. 选择保存位置
3. 输入文件名（如"MyProject.json"）
4. 点击保存

## 下一步开发

可能的改进方向：
- [ ] 节点配置面板（参数编辑）
- [ ] 流程模拟执行
- [ ] 从模块功能表拖拽创建节点
- [ ] 连接线样式改进（曲线连接）
- [ ] 节点搜索和过滤
- [ ] 流程模板保存和加载
- [ ] 实时gRPC调试
- [ ] 多流程对比视图

## 故障排除

### 流程编辑器不显示
- 确保选择了正确的节点（平台或产线）
- 切换到正确的标签页（平台任务或工艺流程）
- 先添加任务或流程

### 无法创建连接
- 确保右键点击节点（不是空白区域）
- 先点击起始节点，再点击目标节点
- 两次点击之间不要点击其他地方

### 缩放不工作
- 使用Ctrl + 鼠标滚轮
- 或使用工具栏的缩放按钮
- 确保鼠标在画布区域内

## 联系方式

如有问题或建议，请联系开发团队。

## 许可证

本项目遵循项目根目录下的LICENSE文件。
