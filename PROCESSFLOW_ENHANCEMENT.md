# Process Flow Enhancement Documentation

## Overview

This document describes the enhancement to the Tangerine Automation System's process flow (工艺流程) capabilities. The changes enable process flows to combine Platform Tasks, Transfer Operations, and Direct Module Actions into unified, orchestrated workflows.

## Problem Statement

Previously, the `ProcessflowConfig` only supported platform tasks:
- Limited flexibility in composing complex workflows
- Could not directly include transfer operations between platforms
- Could not include standalone module actions
- Did not align with the "everything is a module" (万物皆是模块) architecture

## Solution Architecture

### New Data Model Components

#### 1. TransferStepConfig
Represents a transfer operation in a process flow.

```csharp
public class TransferStepConfig
{
    public long StepId { get; set; }
    public int StepOrder { get; set; }
    public string StepDescription { get; set; }
    public long TransferModuleId { get; set; }
    public TransferDirection TransferDirection { get; set; }
    public long SourcePlatformId { get; set; }
    public long TargetPlatformId { get; set; }
}

public enum TransferDirection
{
    Forward = 0,   // 正向移动
    Backward = 1   // 反向移动
}
```

#### 2. ModuleActionStepConfig
Represents a direct module action in a process flow.

```csharp
public class ModuleActionStepConfig
{
    public long StepId { get; set; }
    public int StepOrder { get; set; }
    public string StepDescription { get; set; }
    public string ModuleName { get; set; }
    public string ModuleSerialNumber { get; set; }
    public Guid ModuleActionId { get; set; }
    public string ActionName { get; set; }
    public string ActionDescription { get; set; }
    public List<ParameterItem> ActionParameters { get; set; }
}
```

#### 3. Enhanced ProcessflowConfig

```csharp
public class ProcessflowConfig
{
    public Guid ProcessId { get; set; }
    public string ProcessCode { get; set; }
    public string ProcessName { get; set; }
    public ProcessStep ProcessType { get; set; }
    
    // Platform tasks
    public List<PlatformTaskProfile> PlatformTaskConfigs { get; set; }
    
    // NEW: Transfer operations
    public List<TransferStepConfig> TransferStepConfigs { get; set; }
    
    // NEW: Direct module actions
    public List<ModuleActionStepConfig> ModuleActionStepConfigs { get; set; }
}
```

### Unified Step Ordering

All step types now include a `StepOrder` field for consistent execution ordering:
- `PlatformTaskProfile.StepOrder`
- `TransferStepConfig.StepOrder`
- `ModuleActionStepConfig.StepOrder`

This allows interleaving different step types in a single workflow while maintaining execution order.

## Data Flow

### Configuration Layer
1. **UI Layer**: `GrpcProjectProcessflowModel`
   - `AddTransferStep()` - Adds a transfer operation to the workflow
   - `AddModuleActionStep()` - Adds a module action to the workflow
   - Observable collections for data binding

2. **Options Layer**: `GrpcProjectProcessflowOptions`
   - `List<GrpcProjectTransferStepOptions> TransferSteps`
   - `List<GrpcProjectModuleActionStepOptions> ModuleActionSteps`

3. **Runtime Layer**: `ProcessflowInfo`
   - `List<TransferStepInfo> TransferSteps`
   - `List<ModuleActionStepInfo> ModuleActionSteps`

4. **Configuration Layer**: `ProcessflowConfig`
   - `List<TransferStepConfig> TransferStepConfigs`
   - `List<ModuleActionStepConfig> ModuleActionStepConfigs`

## Usage Examples

### Example 1: Simple Process Flow with Transfer
```
Process Flow: Sample Preparation
├── Step 1 (Order: 0) - Platform Task: "Load Samples" on Platform A
├── Step 2 (Order: 1) - Transfer: Platform A → Platform B (Forward)
└── Step 3 (Order: 2) - Platform Task: "Analyze Samples" on Platform B
```

### Example 2: Complex Multi-Platform Process
```
Process Flow: Complete Analysis Workflow
├── Step 1 (Order: 0) - Platform Task: "Initialize" on Platform A
├── Step 2 (Order: 1) - Module Action: "Calibrate Sensor" on Module X
├── Step 3 (Order: 2) - Platform Task: "Prepare Sample" on Platform A
├── Step 4 (Order: 3) - Transfer: Platform A → Platform B (Forward)
├── Step 5 (Order: 4) - Platform Task: "Primary Analysis" on Platform B
├── Step 6 (Order: 5) - Module Action: "Record Data" on Module Y
├── Step 7 (Order: 6) - Transfer: Platform B → Platform C (Forward)
└── Step 8 (Order: 7) - Platform Task: "Final Processing" on Platform C
```

## Implementation Details

### Modified Files

1. **ConsoleApp1/Laboratory/ModuleDocument.cs**
   - Added `TransferStepConfig` class
   - Added `ModuleActionStepConfig` class
   - Added `TransferDirection` enum
   - Enhanced `ProcessflowConfig` with new lists
   - Added `StepOrder` to `PlatformTaskProfile`

2. **ConsoleApp1/Common/Common/GrpcProjectService.cs**
   - Added `GrpcProjectTransferStepOptions` class
   - Added `GrpcProjectModuleActionStepOptions` class
   - Enhanced `GrpcProjectProcessflowOptions` with new lists
   - Added `StepOrder` to `GrpcProjectPlatformsInOrderConfigOptions`

3. **ConsoleApp1/Laboratory/AppLabOsLogicService.cs**
   - Added `TransferStepInfo` class
   - Added `ModuleActionStepInfo` class
   - Enhanced `ProcessflowInfo` with new lists
   - Added `StepOrder` to `PlatformTaskInfo`
   - Updated `ConfigureProcessflow()` to load transfer and module action steps

4. **Equipment.Bqjx.StandardPlatformSystem/Models/GrpcProjectProcessflowModel.cs**
   - Added `TransferStepsInOrder` observable collection
   - Added `ModuleActionStepsInOrder` observable collection
   - Added `AddTransferStep()` command
   - Added `RemoveTransferStep()` command
   - Added `AddModuleActionStep()` command
   - Added `RemoveModuleActionStep()` command

5. **Tangerine.Grpc.Foundation/Services/Smart_Lab_OSService.cs**
   - Updated `GetLaboratoryConfigs()` to serialize transfer and module action steps
   - Added step order handling in serialization

## Benefits

1. **Flexibility**: Process flows can now combine different types of operations
2. **Modularity**: Each step type is independent and reusable
3. **Clarity**: Explicit step ordering makes execution sequence transparent
4. **Scalability**: Architecture supports adding new step types in the future
5. **Interoperability**: Aligns with the "everything is a module" philosophy

## Migration Guide

### For Existing Process Flows
Existing process flows with only platform tasks will continue to work without changes. The new fields are additive and optional.

### For New Process Flows
To create a new process flow with transfers and module actions:

1. Create platform tasks as before
2. Add transfer steps using `AddTransferStep()`
3. Add module actions using `AddModuleActionStep()`
4. Set appropriate `StepOrder` values to control execution sequence

### JSON Configuration Example
```json
{
  "ProcessId": "...",
  "ProcessCode": "SampleWorkflow",
  "ProcessName": "Sample Processing Workflow",
  "ProcessType": "Pretreatment",
  "PlatformTaskConfigs": [
    {
      "PlatformId": 1,
      "PlatformTaskId": 100,
      "StepOrder": 0,
      "PlatformTaskCode": "LoadSample",
      "PlatformTaskDescription": "Load samples onto platform"
    }
  ],
  "TransferStepConfigs": [
    {
      "StepId": 200,
      "StepOrder": 1,
      "StepDescription": "Transfer to analysis platform",
      "TransferModuleId": 5,
      "TransferDirection": 0,
      "SourcePlatformId": 1,
      "TargetPlatformId": 2
    }
  ],
  "ModuleActionStepConfigs": [
    {
      "StepId": 300,
      "StepOrder": 2,
      "StepDescription": "Calibrate sensor",
      "ModuleName": "SensorModule",
      "ModuleActionId": "...",
      "ActionName": "Calibrate"
    }
  ]
}
```

## Future Enhancements

Potential future improvements:
1. Conditional branching in process flows
2. Parallel execution of independent steps
3. Loop constructs for repetitive operations
4. Error handling and recovery steps
5. Dynamic parameter passing between steps
6. Visual workflow designer in UI

## Conclusion

This enhancement transforms process flows from simple platform task sequences into sophisticated, multi-platform orchestration workflows. The architecture maintains backward compatibility while enabling powerful new capabilities for complex automation scenarios.
