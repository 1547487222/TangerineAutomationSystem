# VSCode风格UI与流程编辑器功能说明

## 概述

本系统现已实现了类似VSCode的UI结构，支持两种类型的流程编辑器：
1. **平台内流程编辑器** - 用于编辑平台任务的内部工作流
2. **产线工艺流程编辑器** - 用于编辑产线级别的工艺流程

## 架构设计

### 核心概念：万物皆模块

系统采用模块化设计，所有功能单元都被抽象为模块：
- **实验室资源**：仓库、AGV、机器人等
- **平台任务**：可独立执行的平台级别任务
- **模块动作**：各个模块提供的具体功能
- **中转节点**：用于流程控制的特殊节点

### 层级结构

```
实验室 (Laboratory)
├── 模块功能表 (ModuleFunctionCatalog) - 定义所有可用的模块功能
├── 模块定义 (ModuleDefinitions) - 模块的元数据
├── 资源定义 (LabResources) - 实验室级别资源
└── 产线 (ProductionLine)
    ├── 平台 (Platform)
    │   ├── 平台任务 (PlatformTask)
    │   │   └── 内部流程 (InternalFlow) - 平台级流程，由模块动作和资源组成
    │   ├── 模块实例 (Modules)
    │   └── 平台资源 (PlatformResources)
    └── 工艺流程 (ProductionLineProcess)
        └── 主流程 (MainFlow) - 产线级流程，由平台任务+中转+模块动作组成
```

## 功能特性

### 1. 模块功能表 (Module Function Catalog)

模块功能表是系统的核心，定义了所有可用的模块功能。每个功能包含：
- **功能名称** (Name) - 唯一标识符
- **显示名称** (DisplayName) - 用户友好的名称
- **模块类型** (ModuleType) - 关联到特定的模块类型
- **描述** (Description) - 功能说明
- **参数定义** (Parameters) - 功能所需的参数

**使用场景：**
- 在实验室节点的"模块功能表"标签页中管理
- 决定流程编辑器中可用的节点类型
- 提供平台流程节点的枚举

### 2. 平台任务 (Platform Task)

平台可以包含多个平台任务，每个任务都有自己的内部流程。

**特点：**
- 每个平台可以添加多个任务
- 每个任务有独立的内部流程 (InternalFlow)
- 内部流程由模块动作和资源节点组成
- 支持拖拽编辑、缩放查看

**编辑方式：**
1. 在树形视图中选择平台节点
2. 切换到"平台任务"标签页
3. 点击"添加平台任务"创建新任务
4. 在任务列表中选择任务
5. 在右侧流程编辑器中编辑任务流程

### 3. 产线工艺流程 (Production Line Process)

产线可以包含多个工艺流程，每个流程组合了平台任务、中转节点和模块动作。

**特点：**
- 一个产线可以有多个工艺流程
- 工艺流程由以下节点类型组成：
  - **平台任务节点** - 引用已定义的平台任务
  - **中转节点** - 用于流程控制和调度
  - **模块动作节点** - 直接调用模块功能
  - **资源节点** - 引用实验室或平台资源

**编辑方式：**
1. 在树形视图中选择产线节点
2. 切换到"工艺流程"标签页
3. 点击"添加工艺流程"创建新流程
4. 在流程列表中选择流程
5. 在右侧流程编辑器中编辑流程

### 4. 流程编辑器功能

#### 节点操作
- **拖拽移动** - 左键拖拽节点到任意位置
- **创建连接** - 右键点击起始节点，再右键点击目标节点
- **选择节点** - 左键点击节点进行选择

#### 节点类型
- **平台任务** (蓝色边框) - 引用平台内定义的任务
- **模块动作** (绿色边框) - 执行具体的模块功能
- **中转** (橙色边框) - 流程控制节点
- **资源** (紫色边框) - 仓库、AGV、机器人等

#### 工具栏功能
- **Add Node** - 添加通用节点
- **Add Platform Task** - 添加平台任务节点
- **Add Transfer** - 添加中转节点
- **Add Module Action** - 添加模块动作节点
- **Auto-Layout** - 自动布局所有节点
- **Zoom In/Out** - 放大/缩小画布
- **Reset Zoom** - 重置缩放到100%

#### 缩放功能
- **鼠标滚轮** - Ctrl + 滚轮进行缩放
- **缩放按钮** - 使用工具栏的放大/缩小按钮
- **缩放范围** - 25% 到 300%
- **实时显示** - 工具栏显示当前缩放比例

#### 画布尺寸
- 画布大小：2400 x 1800 像素
- 支持滚动查看完整画布
- 适合复杂的工作流设计

## 数据模型

### 核心模型类

#### ModuleFunction
```csharp
public class ModuleFunction
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string ModuleType { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
}
```

#### PlatformTask
```csharp
public class PlatformTask
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ProcessFlow InternalFlow { get; set; }  // 平台内部流程
}
```

#### ProductionLineProcess
```csharp
public class ProductionLineProcess
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ProcessFlow MainFlow { get; set; }  // 产线主流程
}
```

#### ProcessFlow
```csharp
public class ProcessFlow
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsPlatformLevel { get; set; }  // 是否为平台级流程
    public List<FlowNode> Nodes { get; set; }
    public List<Connection> Connections { get; set; }
}
```

#### FlowNode
```csharp
public class FlowNode
{
    public string Id { get; set; }
    public string Name { get; set; }
    public FlowNodeKind Kind { get; set; }  // PlatformTask, ModuleAction, Transfer, Resource
    public double X { get; set; }
    public double Y { get; set; }
    public string ConfigJson { get; set; }
}
```

## 与gRPC接口的集成

系统通过 `PlatformCallGrpcClient` 与外部gRPC服务集成：

```csharp
public class PlatformCallGrpcClient
{
    public async Task<bool> CallExecutePlatformTaskAsync(
        string platformId, 
        string taskId, 
        string payloadJson)
    {
        // 调用gRPC服务执行平台任务
    }
}
```

**集成点：**
- 平台任务执行时调用gRPC接口
- 支持传递任务配置的JSON参数
- 异步执行，支持长时间运行的任务

## UI布局

### 左侧面板 (解决方案资源管理器)
- 树形结构显示项目层级
- 支持展开/折叠节点
- 右键菜单操作（添加、删除、重命名）

### 右侧面板 (编辑区域)
根据选中节点类型显示不同的编辑器：
- **实验室节点** - 基本信息 + 模块功能表
- **产线节点** - 基本信息 + 工艺流程编辑器
- **平台节点** - 基本信息 + 平台任务编辑器
- **模块节点** - 模块配置编辑器

## 使用流程示例

### 创建平台任务
1. 在树中选择"平台-1"节点
2. 点击"平台任务"标签页
3. 点击"添加平台任务"
4. 输入任务名称和描述
5. 在流程编辑器中添加模块动作节点
6. 拖拽节点到合适位置
7. 右键创建节点之间的连接
8. 保存项目

### 创建工艺流程
1. 在树中选择"产线-A"节点
2. 点击"工艺流程"标签页
3. 点击"添加工艺流程"
4. 输入流程名称和描述
5. 添加平台任务节点（引用已创建的平台任务）
6. 添加中转节点和模块动作节点
7. 连接各个节点形成完整流程
8. 使用自动布局功能优化布局
9. 保存项目

## 最佳实践

1. **先定义模块功能表** - 在开始创建流程前，先在实验室节点中定义好所有需要的模块功能

2. **模块化设计** - 将复杂任务分解为多个小的平台任务，然后在工艺流程中组合

3. **命名规范** - 使用清晰的命名规范，如：
   - 平台任务：`Task_取料` `Task_放料`
   - 工艺流程：`Process_全自动流程` `Process_半自动流程`

4. **连接清晰** - 使用自动布局功能保持流程图的清晰性

5. **测试验证** - 在实际部署前，通过gRPC接口测试每个平台任务的执行

## 技术特点

- **WPF MVVM架构** - 清晰的数据绑定和命令模式
- **可扩展性** - 易于添加新的节点类型和功能
- **序列化支持** - 使用JSON格式保存和加载项目
- **实时更新** - 连接线随节点移动自动更新
- **响应式设计** - 支持不同分辨率的显示器

## 未来扩展

可能的功能扩展方向：
- 节点配置表单自动生成
- 流程执行模拟和调试
- 流程模板库
- 拖拽式节点创建（从左侧面板拖到画布）
- 多人协作编辑
- 版本控制集成
- 更丰富的节点样式和图标
